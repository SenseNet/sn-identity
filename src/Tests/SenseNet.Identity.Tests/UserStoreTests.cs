using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Identity.Experimental;
using SenseNet.Tests;
using Task = System.Threading.Tasks.Task;

namespace SenseNet.Identity.Tests
{
    [TestClass]
    public class UserStoreTests : TestBase
    {
        [TestMethod]
        public async Task UserStore_GetBasicValues()
        {
            var store = new SnUserStore();
            var user = GetDefaultUser();

            Assert.AreEqual("123", await store.GetUserIdAsync(user, CancellationToken.None));
            Assert.AreEqual("test@example.com", await store.GetUserNameAsync(user, CancellationToken.None));
            Assert.AreEqual("ntest@example.com", await store.GetNormalizedUserNameAsync(user, CancellationToken.None));
            Assert.AreEqual("test@example.com", await store.GetEmailAsync(user, CancellationToken.None));
            Assert.AreEqual("ntest@example.com", await store.GetNormalizedEmailAsync(user, CancellationToken.None));

            Assert.IsTrue(await store.GetEmailConfirmedAsync(user, CancellationToken.None));

            await store.SetUserNameAsync(user, "newtest", CancellationToken.None);
            Assert.AreEqual("newtest", user.UserName);

            await store.SetNormalizedUserNameAsync(user, "domain\\newntest", CancellationToken.None);
            Assert.AreEqual("newntest", user.NormalizedUserName);

            await store.SetEmailAsync(user, "newtest@example.com", CancellationToken.None);
            Assert.AreEqual("newtest@example.com", user.Email);

            await store.SetNormalizedEmailAsync(user, "newntest@example.com", CancellationToken.None);
            Assert.AreEqual("newntest@example.com", user.NormalizedEmail);
        }

        [TestMethod]
        public async Task UserStore_Create_Simple()
        {
            var store = new SnUserStore();
            var user = GetDefaultUser();

            await Test(async () =>
            {
                await store.CreateAsync(user, CancellationToken.None);

                var userNode = Node.Load<User>(user.Id);

                Assert.AreEqual(user.UserName.Replace('@', '-'), userNode.Name);
                Assert.AreEqual( $"BuiltIn\\{user.UserName}", userNode.Username);
                Assert.AreEqual(user.Email, userNode.Email);

                // Do not use NormalizedEmail here, because it may be different from Email,
                // and we only store the latter one.
                var foundUser1 = await store.FindByEmailAsync(user.Email, CancellationToken.None);
                var foundUser2 = await store.FindByIdAsync(user.Id.ToString(), CancellationToken.None);
                var foundUser3 = await store.FindByNameAsync(user.UserName, CancellationToken.None);

                Assert.AreEqual(user.Id, foundUser1.Id);
                Assert.AreEqual(user.Id, foundUser2.Id);
                Assert.AreEqual(user.Id, foundUser3.Id);
                Assert.AreEqual(user.Email, foundUser1.Email);
                Assert.AreEqual(user.UserName, foundUser3.UserName);
            });
        }
        [TestMethod]
        public async Task UserStore_Create_CustomParent()
        {
            var parentPath = "/Root/IMS/BuiltIn/TestOrgUnit";
            var store = new SnUserStore(parentPath);
            var user = GetDefaultUser();

            await Test(async () =>
            {
                await store.CreateAsync(user, CancellationToken.None);

                var userNode = Node.Load<User>(user.Id);

                Assert.AreEqual($"{parentPath}/{user.UserName.Replace('@', '-')}", userNode.Path);
            });
        }
        [TestMethod]
        public async Task UserStore_Create_WithGroups()
        {
            var parentPath = "/Root/IMS/BuiltIn/TestOrgUnit";
            var groupName = Guid.NewGuid().ToString();
            var groupPath = "/Root/IMS/BuiltIn/" + groupName;
            var store = new SnUserStore(parentPath, new []{ groupPath });
            var user = GetDefaultUser();

            await Test(async () =>
            {
                var group = (RepositoryTools.CreateStructure(groupPath, "Group") ?? Content.Load(groupPath))
                    .ContentHandler as Group;

                await store.CreateAsync(user, CancellationToken.None);

                var userNode = Node.Load<User>(user.Id);

                Assert.IsTrue(userNode.IsInGroup(group));
            });
        }

        private static SnIdentityUser GetDefaultUser()
        {
            return new SnIdentityUser
            {
                UserName = "test@example.com",
                Id = 123,
                Email = "test@example.com",
                EmailConfirmed = true,
                NormalizedEmail = "ntest@example.com",
                NormalizedUserName = "ntest@example.com",
                PasswordHash = "abc",
                PhoneNumber = "123456789"
            };
        }
    }
}
