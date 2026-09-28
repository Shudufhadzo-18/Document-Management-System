using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Document_Management_System.Models.DTOS.DocumentResponseDto;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // documents are the core sensitive resource — require auth on every action here
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentPermissionService _permissionService;

        public DocumentsController(IDocumentService documentService, IDocumentPermissionService permissionService)
        {
            _documentService = documentService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentSummaryDto>>> GetAll()
        {
            var documents = await _documentService.GetAllAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentResponseDto>> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Admins always pass; everyone else needs either no permission record at
            // all (meaning access is governed by role only, not restricted) or an
            // explicit CanView grant if one exists for this document
            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                var hasAnyRestriction = (await _permissionService.GetForDocumentAsync(id)).Any();
                if (hasAnyRestriction)
                {
                    var canView = await _permissionService.HasPermissionAsync(id, userId, "View");
                    if (!canView) return Forbid();
                }
            }

            var document = await _documentService.GetByIdAsync(id);
            if (document == null) return NotFound();
            return Ok(document);
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<DocumentResponseDto>> UpdateStatus(int id, [FromBody] DocumentStatusUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var updated = await _documentService.UpdateStatusAsync(id, dto.NewStatus, userId, dto.Reason);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<ActionResult<DocumentResponseDto>> Create([FromBody] DocumentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ownerId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(ownerId)) return Unauthorized();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var created = await _documentService.CreateAsync(dto, ownerId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);

            // DocumentsController.cs — Create action


        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            // Delete already requires Admin at the controller level, so no extra
            // permission check needed here — Admins bypass document-level permissions entirely
            var deleted = await _documentService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();

        }

        [HttpGet("expiring")]
        public async Task<ActionResult<IEnumerable<DocumentSummaryDto>>> GetExpiring([FromQuery] int withinDays = 30)
        {
            return Ok(await _documentService.GetExpiringAsync(withinDays));
        }

        [HttpGet("expired")]
        public async Task<ActionResult<IEnumerable<DocumentSummaryDto>>> GetExpired()
        {
            return Ok(await _documentService.GetExpiredAsync());
        }

        [HttpGet("review-due")]
        public async Task<ActionResult<IEnumerable<DocumentSummaryDto>>> GetReviewDue()
        {
            return Ok(await _documentService.GetReviewDueAsync());
        }
    }
}
