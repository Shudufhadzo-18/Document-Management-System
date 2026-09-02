using Document_Management_System.Models.DTOS;

namespace Document_Management_System.Interfaces
{
    public interface IAuditLogService
    {
        // Called internally by other services (e.g. DocumentService) whenever
        // an action happens — never exposed directly to the client
        Task LogAsync(string userId, int documentId, string action, string ipAddress);

        Task<IEnumerable<AuditLogResponseDto>> GetForDocumentAsync(int documentId);
    }
}
