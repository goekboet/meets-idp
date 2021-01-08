using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Ids.Invite
{
    public class InvitationJson
    {
        [Required]
        [EmailAddress]
        [MaxLength(320)]
        public string Email { get; set; }
    }

    public class StatusJson
    {
        public string Invited { get; set; }
    }

    public interface IInvitation
    {
        Task<Result<Guid>> Invite(string email);
    }
}