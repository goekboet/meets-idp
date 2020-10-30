using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ids.Unregister
{
    public class UnregisterRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }
    }

    public class EmailVerificationCode
    {
        [Required]
        [Guid]
        public string UserId { get; set; }
        [Required]
        [MaxLength(512)]
        public string Code { get; set; }
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
            string userId,
            string email,
            string code)
        {
            UserId = userId;
            Email = email;
            Code = code;
        }

        public string UserId { get; }
        public string Email { get; }
        public string Code { get; }
    }

    public interface IAccountDeletion
    {
        public Task<Result<ActiveAccount>> Verify(
            UnverifiedAccount a);

        public Task<Result<Unit>> Delete(
            ActiveAccount a);
    }

    public interface ICodeDistribution
    {
        public Task<Result<string>> Send(
            HttpResponse r,
            ActiveAccount a);
    }
}