using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Document_Management_System.Models.DTOS.UserDTOs;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // the entire controller is Admin-only — managing roles is a sensitive operation
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    Roles = roles.ToList()
                });
            }

            return Ok(result);
        }

        [HttpGet("roles")]
        public ActionResult<IEnumerable<string>> GetAllRoles()
        {
            return Ok(_roleManager.Roles.Select(r => r.Name).ToList());
        }

        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found.");

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return BadRequest($"Role '{dto.Role}' does not exist.");

            if (await _userManager.IsInRoleAsync(user, dto.Role))
                return BadRequest($"User already has the '{dto.Role}' role.");

            var result = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok();
        }

        [HttpDelete("{id}/roles/{role}")]
        public async Task<IActionResult> RemoveRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found.");

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok();
        }
    }
}
