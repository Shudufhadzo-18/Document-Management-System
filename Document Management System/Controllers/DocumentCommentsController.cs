using Document_Management_System.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Document_Management_System.Models.DTOS.DocumentCommentDTOs;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/documents/{documentId}/comments")]
    [Authorize]
    public class DocumentCommentsController : ControllerBase
    {
        private readonly IDocumentCommentService _commentService;

        public DocumentCommentsController(IDocumentCommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentCommentResponseDto>>> GetForDocument(int documentId)
        {
            return Ok(await _commentService.GetForDocumentAsync(documentId));
        }

        [HttpPost]
        public async Task<ActionResult<DocumentCommentResponseDto>> Add(
            int documentId, [FromBody] DocumentCommentCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content)) return BadRequest("Comment cannot be empty.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _commentService.AddAsync(documentId, dto.Content, userId);
            return Ok(result);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> Delete(int documentId, int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var deleted = await _commentService.DeleteAsync(commentId, userId, isAdmin);
            if (!deleted) return Forbid();
            return NoContent();
        }
    }
}
