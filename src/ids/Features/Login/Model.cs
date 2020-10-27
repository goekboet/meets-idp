using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
        public bool RememberLogin { get; set; }
        
        [MaxLength(4096)]
        public string ReturnUrl { get; set; }
    }

    public static class LoginConstants
    {
        public const string InvalidCredentials = "invalid_credentials";
    }
    
    public interface IVerifyCredentials
    {
        public Task<Result<Unit>> Verify(string userName, string password);
    }
}