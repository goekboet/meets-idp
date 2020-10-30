using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ids.ChangeUsername
{
    public class ChangeUsernameInput
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string NewUsername { get; set; }
    }

    public class VerifiedChangeUsernameInput
    {        
        [Required]
        [MaxLength(512)]
        public string Code { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string NewUsername { get; set; }
    }

    public class ResetToken
    {
        public ResetToken(
            string userId,
            string email,
            string token
        )
        {
            UserId = userId;
            Email = email;
            Token = token;
        }

        public string UserId { get; }
        public string Email { get; }
        public string Token { get; }
    }

    public interface IToken
    {
        Task<Result<ResetToken>> GetToken(ClaimsPrincipal p, string newEmail);
    }

    public interface IExchange
    {
        Task<Result<string>> Exchange(
            HttpResponse r,
            ResetToken t);
    }

    public class VerifiedChangeUsername
    {
        public VerifiedChangeUsername(
            string token,
            string newUsername
        )
        {
            Token = token;
            NewUsername = newUsername;
        }
   
        public string Token { get; }
        public string NewUsername { get; }
    }

    public interface IUsername
    {
        Task<Result<Unit>> Change(
            ClaimsPrincipal p,
            VerifiedChangeUsername r);
    }
}