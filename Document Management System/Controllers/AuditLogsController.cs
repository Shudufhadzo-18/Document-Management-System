using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/documents/{documentId}/audit-logs")]
    //[Authorize(Roles = "Admin,ComplianceOfficer")] // audit trail is sensitive — restrict to compliance-facing roles
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> GetForDocument(int documentId)
        {
            var logs = await _auditLogService.GetForDocumentAsync(documentId);
            return Ok(logs);
        }
    }
}
