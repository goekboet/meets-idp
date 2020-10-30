using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ids.Profile
{
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IGetProfile _profile;
        private readonly ISetPassword _pwd;
        private readonly ISetName _name;
        private readonly IChangePassword _changePwd;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IGetProfile profile,
            ISetPassword pwd,
            ISetName name,
            IChangePassword changePwd,
            ILogger<ProfileController> logger
        )
        {
            _profile = profile;
            _pwd = pwd;
            _name = name;
            _changePwd = changePwd;
            _logger = logger;
        }
        
        [HttpGet, HttpHead]
        [Route("/api/profile")]
        public async Task<IActionResult> Index()
        {
            var r = await _profile.Get(User);
            if (r is Ok<Profile> okProfile)
            {
                var p = okProfile.Value;
                return Ok(new ProfileJson
                {
                    Email = p.Email,
                    Name = p.Name,
                    HasPassword = p.HasPassword
                });
            }
            else if (r is Error<Profile> errProfile)
            {
                _logger.LogError(errProfile.Description);
            }

            return BadRequest();
        }

        [Route("/api/profile/password")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddPassword(PasswordJson newPwd)
        {
            if (ModelState.IsValid)
            {
                var r = await _pwd.Set(User, newPwd.Password);

                if (r is Ok<Unit> okSetPwd)
                {
                    return Created("api/profile", "");
                }
                else if (r is Error<Unit> errorSetPwd)
                {
                    _logger.LogWarning(errorSetPwd.Description);
                    ModelState.AddModelError("Api", errorSetPwd.Description);
                }
            }

            return BadRequest(ModelState);
        }

        [Route("/api/profile/name")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SetName(NameJson n)
        {
            if (ModelState.IsValid)
            {
                var r = await _name.Set(User, n.Name);
                if (r is Ok<Unit> setNameOk)
                {
                    return Created("api/profile", new object());
                }
                else if (r is Error<Unit> setNameError)
                {
                    _logger.LogWarning(setNameError.Description);
                    ModelState.AddModelError("Api", setNameError.Description);
                }
            }
            
            return BadRequest(ModelState);
        }

        [Route("/api/profile/changePassword")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordJson p
        )
        {
            if (ModelState.IsValid)
            {
                var r = await _changePwd.Change(User, p.Old, p.New);

                if (r is Ok<Unit> changePwdOk)
                {
                    return Created("api/profile", new object());
                }
                else if (r is Error<Unit> changePwdError)
                {
                    _logger.LogWarning(changePwdError.Description);
                    ModelState.AddModelError("Api", changePwdError.Description);
                }
            }

            return BadRequest(ModelState);
        }
    }
}