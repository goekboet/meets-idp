using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;

namespace Ids.ResetPassword
{
    public class RegistrationRequest
    {
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
            string email,
            string userId,
            string code
        )
        {
            Email = email;
            UserId = userId;
            VerificationCode = code;
        }

        public string Email { get; }
        public string UserId { get; }
        public string VerificationCode { get; }
    }

    public class UnregisteredAccount
    {
        public UnregisteredAccount(string email)
        {
            Email = email;
        }

        public string Email { get; }
    }



    public interface IAccountRegistration
    {
        public Task<Result<UnverifiedAccount>> RegisterAccount(
            UnregisteredAccount a);
    }

    public interface IAccountActivation
    {
        public Task<Result<Unit>> ActivateAccount(
            UnverifiedAccount a);
    }

    public interface ICodeDistribution
    {
        public Task<Result<string>> Send(
            HttpResponse r,
            UnverifiedAccount a);
    }
}