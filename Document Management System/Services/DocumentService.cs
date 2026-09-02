using Document_Management_System.Data;
using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Document_Management_System.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Document_Management_System.Models.DTOS.DocumentResponseDto;

namespace Document_Management_System.Services
{
    public class DocumentService: IDocumentService
    {

        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLog;

        public DocumentService(ApplicationDbContext context, IAuditLogService auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<IEnumerable<DocumentSummaryDto>> GetAllAsync()
        {
            return await _context.Documents
                .Select(d => new DocumentSummaryDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    Status = d.Status,
                    CategoryId = d.CategoryId,
                    CategoryName = _context.Categories
                        .Where(c => c.Id == d.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<DocumentResponseDto> GetByIdAsync(int id)
        {
            // Join category name and current version number in one query
            // rather than lazy-loading navigation properties — keeps this
            // explicit and avoids accidental N+1 queries later
            var doc = await _context.Documents
                .Where(d => d.Id == id)
                .Select(d => new DocumentResponseDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    CategoryName = _context.Categories
                        .Where(c => c.Id == d.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault(),
                    OwnerName = d.OwnerId, // placeholder until Identity user lookup is wired in
                    Status = d.Status,
                    CurrentVersionNumber = _context.DocumentVersions
                        .Where(v => v.Id == d.CurrentVersionId)
                        .Select(v => (int?)v.VersionNumber)
                        .FirstOrDefault(),
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return doc;
        }

        public async Task<DocumentResponseDto> CreateAsync(DocumentCreateDto dto, string ownerId, string? ipAddress)
        {
            var document = new Document
            {
                Title = dto.Title,
                CategoryId = dto.CategoryId,
                OwnerId = ownerId,
                Status = "Draft", // every new document starts as Draft — status changes go through a separate workflow action
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(ownerId, document.Id, "Create",ipAddress);

            return await GetByIdAsync(document.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return false;

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        // Defines which transitions are legal — prevents e.g. jumping straight
        // from Draft to Archived, or "un-archiving" without a deliberate path
        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            ["Draft"] = new[] { "Active" },
            ["Active"] = new[] { "Archived", "Draft" },
            ["Archived"] = new[] { "Active" }
        };

        public async Task<DocumentResponseDto> UpdateStatusAsync(int id, string newStatus, string userId)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return null;

            var validNextStatuses = AllowedTransitions.GetValueOrDefault(document.Status, Array.Empty<string>());
            if (!validNextStatuses.Contains(newStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot change status from '{document.Status}' to '{newStatus}'.");
            }

            document.Status = newStatus;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(userId, id, $"StatusChange:{newStatus}", ipAddress: null);

            return await GetByIdAsync(id);
        }

    }
}
