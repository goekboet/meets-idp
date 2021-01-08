using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ids.Invite
{
    [ApiController]
    [Authorize(Policy = "OpenId")]
    public class InviteController : ControllerBase
    {
        private IInvitation Invitation { get; }
        private ILogger<InviteController> Logger { get; }

        public InviteController(
            IInvitation invitation,
            ILogger<InviteController> logger
        )
        {
            Invitation = invitation;
            Logger = logger;
        }

        [HttpPost]
        [Route("/api/invite")]
        public async Task<IActionResult> Index(InvitationJson p)
        {
            if (ModelState.IsValid)
            {
                var sub = await Invitation.Invite(p.Email);
            
                if (sub is Ok<Guid> okSub)
                {
                    var r = new StatusJson
                    {
                        Invited = okSub.Value.ToString(),
                    };

                    return Ok(r); 
                }
                else if (sub is Error<Guid> errSub)
                {
                    Logger.LogError(errSub.Description);
                    return StatusCode(500);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(sub));
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}