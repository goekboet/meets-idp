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
        public async Task<IActionResult> Index(
            Invitation p)
        {
            if (ModelState.IsValid)
            {
                var sub = await Invitation.Invite(p);
            
                if (sub is Ok<Invitation> okSub)
                {
                    return Ok(p); 
                }
                else if (sub is Error<Invitation> errSub)
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