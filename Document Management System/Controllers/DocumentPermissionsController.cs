using Document_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Document_Management_System.Models.DTOS.DocumentPermissionDTOs;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/documents/{documentId}/permissions")]
    [Authorize(Roles = "Admin")] // granting/revoking access to a document is an Admin action
    public class DocumentPermissionsController : ControllerBase
    {
        private readonly IDocumentPermissionService _permissionService;

        public DocumentPermissionsController(IDocumentPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentPermissionResponseDto>>> GetForDocument(int documentId)
        {
            return Ok(await _permissionService.GetForDocumentAsync(documentId));
        }

        [HttpPost]
        public async Task<ActionResult<DocumentPermissionResponseDto>> Grant(
            int documentId, [FromBody] DocumentPermissionGrantDto dto)
        {
            var grantedBy = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "unknown";
            var result = await _permissionService.GrantAsync(documentId, dto, grantedBy);
            return Ok(result);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Revoke(int documentId, string userId)
        {
            var revoked = await _permissionService.RevokeAsync(documentId, userId);
            if (!revoked) return NotFound();
            return NoContent();
        }
    }
}
