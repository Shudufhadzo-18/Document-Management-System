using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/documents/{documentId}/versions")] // nested under documents — a version doesn't make sense without its parent
    [Authorize]
    public class DocumentVersionsController : ControllerBase
    {
        private readonly IDocumentVersionService _versionService;
        private readonly IFileStorageService _fileStorage;
        private readonly IDocumentPermissionService _permissionService;

        public DocumentVersionsController(
            IDocumentVersionService versionService,
            IFileStorageService fileStorage,
            IDocumentPermissionService permissionService)
        {
            _versionService = versionService;
            _fileStorage = fileStorage;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentVersionResponseDto>>> GetVersions(int documentId)
        {
            var versions = await _versionService.GetVersionsForDocumentAsync(documentId);
            return Ok(versions);
        }

        [HttpPost]
        [RequestSizeLimit(50_000_000)] // 50MB cap — adjust to whatever SETA's expected max document size is
        public async Task<ActionResult<DocumentVersionResponseDto>> Upload(
            int documentId,
            [FromForm] DocumentVersionUploadDto dto,
            IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file was uploaded.");

            dto.DocumentId = documentId;

            var uploadedBy = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(uploadedBy)) return Unauthorized();

            using var stream = file.OpenReadStream();
            var result = await _versionService.UploadAsync(dto, stream, file.FileName, file.ContentType, uploadedBy);

            return CreatedAtAction(nameof(GetVersions), new { documentId }, result);

        }

        [HttpGet("{versionId}/download")]
        public async Task<IActionResult> Download(int documentId, int versionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                var hasAnyRestriction = (await _permissionService.GetForDocumentAsync(documentId)).Any();
                if (hasAnyRestriction)
                {
                    var canDownload = await _permissionService.HasPermissionAsync(documentId, userId, "Download");
                    if (!canDownload) return Forbid();
                }
            }

            var version = await _versionService.GetVersionByIdAsync(versionId);
            if (version == null || version.DocumentId != documentId) return NotFound();

            var stream = await _fileStorage.GetFileAsync(version.FilePath);
            return File(stream, version.MimeType ?? "application/octet-stream", version.OriginalFileName);
        }
    }
}
