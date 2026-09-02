using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Document_Management_System.Models.DTOS.DocumentResponseDto;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // documents are the core sensitive resource — require auth on every action here
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
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
            var document = await _documentService.GetByIdAsync(id);
            if (document == null) return NotFound();
            return Ok(document);
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<DocumentResponseDto>> UpdateStatus(int id, [FromBody] DocumentStatusUpdateDto dto)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var updated = await _documentService.UpdateStatusAsync(id, dto.NewStatus, userId);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                // invalid transition (e.g. Draft -> Archived directly) — 400, not 500,
                // since this is a client input problem, not a server failure
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

            var created = await _documentService.CreateAsync(dto, ownerId , ipAddress);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);

            // DocumentsController.cs — Create action
            
           
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _documentService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
    }
