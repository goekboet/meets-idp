using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.Login
{
    public class LoginInput
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }
        
        [Required]
        [MinLength(8)]
        [MaxLength(64)]
        public string Password { get; set; }
        
        [MaxLength(4096)]
        public string ReturnUrl { get; set; }
    }
    
    public interface IVerifyCredentials
    {
        public Task<Result<Unit>> Verify(string userName, string password);
    }

    public enum UserRecordStatus { Unknown, AwaitingEmailConfirmation, Known }

    public static class Extensions
    {
        public static async Task<(IdsUser, UserRecordStatus)> GetUserRecordStatus(
            this UserManager<IdsUser> u,
            string username
        )
        {
            var user = await u.FindByNameAsync(username);
            if (user == null)
            {
                return (user, UserRecordStatus.Unknown);
            }
            var email = await u.IsEmailConfirmedAsync(user);
            var hasPwd = await u.HasPasswordAsync(user);
            if (!email || !hasPwd)
            {
                return (user, UserRecordStatus.AwaitingEmailConfirmation);
            }

            return (user, UserRecordStatus.Known);
        }
    }
}