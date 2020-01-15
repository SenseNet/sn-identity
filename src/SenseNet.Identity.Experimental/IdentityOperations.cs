using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Identity.Experimental
{
    public static class IdentityOperations
    {
        [ODataAction]
        [ContentTypes(N.CT.PortalRoot)]
        [AllowedRoles(N.R.All)]
        public static async Task<object> ValidateCredentials(Content content, HttpContext context, string userName, string password)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (userName == null)
                throw new ArgumentNullException(nameof(userName));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (!(context.RequestServices.GetService(typeof(UserManager<SnIdentityUser>)) is UserManager<SnIdentityUser> um))
                throw new InvalidOperationException("User service not available.");

            var user = await um.FindByNameAsync(userName);

            if (user != null && await um.CheckPasswordAsync(user, password))
            {
                return new
                {
                    user.Id,
                    user.Email,
                    user.UserName
                };
            }

            throw new SenseNetSecurityException("Invalid username or password.");
        }
    }
}
