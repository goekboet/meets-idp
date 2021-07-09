using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Ids.Invite
{
    public class Invitation
    {
        [Required]
        [EmailAddress]
        [MaxLength(320)]
        public string Email { get; set; }
        public Guid UserId {get;set;}
        public string EmailConfirmationToken {get;set;}
    }

    public interface IInvitation
    {
        Task<Result<Invitation>> Invite(Invitation invitee);
    }
}