using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public class LoginDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class RegisterDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors); // returns Identity's built-in password/validation errors

            // sign in immediately after registering, so the user isn't forced
            // to log in again right after creating their account
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(new { user.Id, user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // lockoutOnFailure: true enables Identity's built-in brute-force
            // lockout after repeated failed attempts
            var result = await _signInManager.PasswordSignInAsync(
                dto.Email, dto.Password, isPersistent: false, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return StatusCode(423, "Account locked due to multiple failed login attempts.");

            if (!result.Succeeded)
                return Unauthorized("Invalid email or password.");

            return Ok(new { message = "Login successful." });
            // the auth cookie is set automatically by SignInManager — nothing
            // else needs to be returned to the client for it to be authenticated
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out." });
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var user = await _userManager.GetUserAsync(User);
            return Ok(new { user.Id, user.Email });
        }
    }
}
