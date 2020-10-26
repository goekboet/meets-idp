using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IdentityServer4.Quickstart.UI
{
    public class ResetInput
    {
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }
    }

    public class VerifiedResetInput
    {
        [Required]
        [Guid]
        public string UserId { get; set; }
        [Required]
        [MaxLength(512)]
        public string Code { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(64)]
        public string Password { get; set; }

        [Compare(nameof(Password))]
        public string PasswordRepeated { get; set; }
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
        Task<Result<ResetToken>> GetToken(string email);
    }

    public interface IExchange
    {
        Task<Result<Unit>> Exchange(ResetToken t);
    }

    public class VerifiedReset
    {
        public VerifiedReset(
            string userId,
            string token,
            string password
        )
        {
            UserId = userId;
            Token = token;
            Password = password;
        }
        public string UserId { get; }
        public string Token { get; }
        public string Password { get; }
    }

    public interface IReset
    {
        Task<Result<Unit>> Reset(VerifiedReset r);
    }
}