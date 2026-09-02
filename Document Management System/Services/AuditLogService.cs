using Document_Management_System.Data;
using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Document_Management_System.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Document_Management_System.Services
{
    public class AuditLogService: IAuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task LogAsync(string userId, int documentId, string action, string ipAddress)
        {
            var log = new AuditLog
            {
                UserId = userId,
                DocumentId = documentId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
            // no return value — this is fire-and-forget from the caller's perspective,
            // it should never block or fail the main action it's logging

        }

        public async Task<IEnumerable<AuditLogResponseDto>> GetForDocumentAsync(int documentId)
        {
            return await _context.AuditLogs
                .Where(a => a.DocumentId == documentId)
                .OrderByDescending(a => a.Timestamp) // most recent activity first
                .Join(_context.Documents,
                    a => a.DocumentId,
                    d => d.Id,
                    (a, d) => new AuditLogResponseDto
                    {
                        Id = a.Id,
                        UserName = a.UserId, // placeholder — swap for a real user lookup once Identity is wired in
                        DocumentTitle = d.Title,
                        Action = a.Action,
                        Timestamp = a.Timestamp
                    })
                .ToListAsync();
        }
    }
}
