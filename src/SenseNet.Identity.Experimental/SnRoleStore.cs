using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SenseNet.Identity.Experimental
{
    public class SnRoleStore : IRoleStore<SnIdentityRole>
    {
        public Task<IdentityResult> CreateAsync(SnIdentityRole role, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }

        public Task<IdentityResult> DeleteAsync(SnIdentityRole role, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }

        public void Dispose()
        {
        }

        public Task<SnIdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }

        public Task<SnIdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }

        public Task<string> GetNormalizedRoleNameAsync(SnIdentityRole role, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }

        public Task<string> GetRoleIdAsync(SnIdentityRole role, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }

        public Task<string> GetRoleNameAsync(SnIdentityRole role, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }

        public Task SetNormalizedRoleNameAsync(SnIdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }

        public Task SetRoleNameAsync(SnIdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }

        public Task<IdentityResult> UpdateAsync(SnIdentityRole role, CancellationToken cancellationToken)
        {
            throw new SnNotSupportedException("Identity Role functionality is not supported yet.");
        }
    }
}
