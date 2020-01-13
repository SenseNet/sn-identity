using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Search;
using Task = System.Threading.Tasks.Task;

namespace SenseNet.Identity.Experimental
{
    public class SnUserStore : IUserPasswordStore<SnIdentityUser>, IUserEmailStore<SnIdentityUser>, IUserPhoneNumberStore<SnIdentityUser>
    {
        #region Properties
        private Func<SnIdentityUser, Task<Node>> GetParentAsync { get; }
        private Func<User, Task<Group[]>> GetGroupsAsync { get; }

        private const string DefaultParentPath = "/Root/IMS/BuiltIn/Portal";
        #endregion

        #region Constructors
        public SnUserStore()
        {
            GetParentAsync = user => LoadOrCreateParentAsync(DefaultParentPath);
            GetGroupsAsync = user => Task.FromResult(new Group[0]);
        }
        public SnUserStore(Func<SnIdentityUser, Task<Node>> getParentCallback)
        {
            GetParentAsync = getParentCallback ?? throw new ArgumentNullException(nameof(getParentCallback));
            GetGroupsAsync = user => Task.FromResult(new Group[0]);
        }
        public SnUserStore(Func<SnIdentityUser, Task<Node>> getParentCallback, Func<User, Task<Group[]>> getGroupsCallback)
        {
            GetParentAsync = getParentCallback ?? throw new ArgumentNullException(nameof(getParentCallback));
            GetGroupsAsync = getGroupsCallback ?? throw new ArgumentNullException(nameof(getGroupsCallback));
        }
        public SnUserStore(string parentPath, string[] groupPaths = null)
        {
            var groupPathsLocal = groupPaths ?? new string[0];

            GetParentAsync = user => LoadOrCreateParentAsync(parentPath);
            GetGroupsAsync = user => Task.FromResult(groupPathsLocal.Select(Node.Load<Group>).ToArray());
        }
        #endregion

        #region IUserStore<SnIdentityUser> members
        public Task<string> GetUserIdAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(SnIdentityUser user, string userName, CancellationToken cancellationToken)
        {
            // remove domain if provided
            var sepIndex = userName.IndexOf("\\", StringComparison.Ordinal);
            var un = sepIndex > -1 ? userName.Substring(sepIndex + 1) : userName;

            user.UserName = un;

            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(SnIdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            // remove domain if provided
            var sepIndex = normalizedName.IndexOf("\\", StringComparison.Ordinal);
            var un = sepIndex > -1 ? normalizedName.Substring(sepIndex + 1) : normalizedName;

            user.NormalizedUserName = un;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            using (new SystemAccount())
            {
                var name = ContentNamingProvider.GetNameFromDisplayName(user.UserName);
                var parent = await GetParentAsync(user);

                var content = Content.CreateNew("User", parent, name);
                content["DisplayName"] = user.UserName;
                content["LoginName"] = user.UserName;
                content["Email"] = user.Email;
                ((User) content.ContentHandler).PasswordHash = user.PasswordHash;
                content.Save();

                user.Id = content.Id;

                foreach (var group in await GetGroupsAsync(content.ContentHandler as User))
                {
                    try
                    {
                        group?.AddMember(content.ContentHandler as User);
                    }
                    catch (Exception e)
                    {
                        SnLog.WriteException(e, $"Error during adding new user {content.Id} ({user.UserName}) to group {group?.Path}.");
                    }
                }
            }

            return IdentityResult.Success;
        }

        public Task<IdentityResult> UpdateAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            using (new SystemAccount())
            {
                var userContent = Node.Load<User>(user.Id);
                userContent.LoginName = user.UserName;
                userContent.Email = user.Email;
                userContent.PasswordHash = user.PasswordHash;
                userContent.Save(SavingMode.KeepVersion);
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            using (new SystemAccount())
            {
                Content.DeletePhysical(user.Id);
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<SnIdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var id = Convert.ToInt32(userId);
            var user = SystemAccount.Execute(() => Node.Load<User>(id));

            return Task.FromResult(SnIdentityUser.FromUser(user));
        }

        public Task<SnIdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var user = SystemAccount.Execute(() => User.Load(normalizedUserName));

            return Task.FromResult(SnIdentityUser.FromUser(user));
        }
        #endregion

        #region IUserPasswordStore<SnIdentityUser> members
        public Task SetPasswordHashAsync(SnIdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }
        #endregion

        #region IUserEmailStore<SnIdentityUser> members
        public Task SetEmailAsync(SnIdentityUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;

            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task SetEmailConfirmedAsync(SnIdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<SnIdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var user = SystemAccount.Execute(() => ContentQuery
                .Query(IdentityQueries.UsersByEmail, QuerySettings.AdminSettings, normalizedEmail)
                .Nodes.FirstOrDefault() as User);

            return Task.FromResult(SnIdentityUser.FromUser(user));
        }

        public Task<string> GetNormalizedEmailAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(SnIdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;

            return Task.CompletedTask;
        }
        #endregion

        #region IUserPhoneNumberStore<SnIdentityUser>
        public Task SetPhoneNumberAsync(SnIdentityUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            //TODO: implement SetPhoneNumberAsync
            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            //TODO: implement GetPhoneNumberAsync
            return Task.FromResult(string.Empty);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            //TODO: implement GetPhoneNumberConfirmedAsync
            return Task.FromResult(true);
        }

        public Task SetPhoneNumberConfirmedAsync(SnIdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            //TODO: implement SetPhoneNumberConfirmedAsync
            return Task.CompletedTask;
        }
        #endregion

        public void Dispose()
        {
        }

        #region Helper methods

        private static Task<Node> LoadOrCreateParentAsync(string parentPath)
        {
            var parent = RepositoryTools.CreateStructure(parentPath, "OrganizationalUnit");
            return Task.FromResult(parent?.ContentHandler ?? Node.LoadNode(parentPath));
        }
        
        #endregion
    }
}
