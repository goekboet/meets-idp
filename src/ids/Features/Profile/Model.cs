using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ids.Profile
{
    public class SpaFlags
    {
        public string OidcLoginId { get; set; }
        public string OidcLoginName { get; set; }
    }
    public class ProfileJson
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool HasPassword { get; set; }
    }

    public class Profile
    {
        public Profile(
            string name,
            string email,
            bool hasPassword
        )
        {
            Name = name;
            Email = email;
            HasPassword = hasPassword;
        }

        public string Name { get; }
        public string Email { get; }
        public bool HasPassword { get; }
    }

    public interface IGetProfile
    {
        Task<Result<Profile>> Get(ClaimsPrincipal user);
    }

    public class PasswordJson
    {
        [Required]
        [MinLength(8)]
        [MaxLength(64)]
        public string Password { get; set; }
    }

    public interface ISetPassword
    {
        Task<Result<Unit>> Set(ClaimsPrincipal p, string newPassword);
    }

    public class NameJson
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }
    }

    public interface ISetName
    {
        Task<Result<Unit>> Set(ClaimsPrincipal p, string newName);
    }

    public class ChangePasswordJson
    {
        [MinLength(8)]
        [MaxLength(64)]
        public string Old { get; set; }

        [MinLength(8)]
        [MaxLength(64)]
        public string New { get; set; }
    }

    public interface IChangePassword
    {
        Task<Result<Unit>> Change(ClaimsPrincipal p, string oldPwd, string newPwd);
    }
}