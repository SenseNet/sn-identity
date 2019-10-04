using Microsoft.AspNetCore.Identity;
using SenseNet.ContentRepository;

namespace SenseNet.Identity.Experimental
{
    public class SnIdentityUser : IdentityUser<int>
    {
        public static SnIdentityUser FromUser(User user)
        {
            if (user == null)
                return null;

            return new SnIdentityUser
            {
                Id = user.Id,
                PasswordHash = user.PasswordHash,
                UserName = user.LoginName,
                NormalizedUserName = user.LoginName,
                Email = user.Email,
                NormalizedEmail = user.Email,
                EmailConfirmed = true
            };
        }
    }
}
