using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ids.Unregister
{
    public class EmailVerificationCode
    {
        [Required]
        [MaxLength(512)]
        public string Code { get; set; }

        [MaxLength(4096)]
        public string ReturnUrl { get; set; }
    }

    public class OidcLogin
    {
        [MaxLength(4096)]
        public string ReturnUrl { get; set; }
    }

    public class UnverifiedAccount
    {
        public UnverifiedAccount(
            string email
        )
        {
            Email = email;
        }

        public string Email { get; }
    }

    public class ActiveAccount
    {
        public ActiveAccount(
            string email,
            string code)
        {
            Email = email;
            Code = code;
        }

        public string Email { get; }
        public string Code { get; }
    }

    public interface IAccountDeletion
    {
        public Task<Result<ActiveAccount>> Verify(
            ClaimsPrincipal p);

        public Task<Result<Unit>> Delete(
            ClaimsPrincipal p,
            string token);
    }

    public interface ICodeDistribution
    {
        public Task<Result<string>> Send(
            HttpResponse r,
            ActiveAccount a);
    }
}