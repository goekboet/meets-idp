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
        public Guid UserId { get; set; }
        public string EmailConfirmationToken { get; set; }
    }

    public record InvitationStatus
    {
        public bool Registered { get; init; }
        public bool HasPassword { get; init; }
        public string UserId { get; init; }
    }

    public interface IInvitation
    {
        Task<Result<Invitation>> Invite(Invitation invitee);
        Task<InvitationStatus> GetInvitationStatus(
            string email);
    }
}