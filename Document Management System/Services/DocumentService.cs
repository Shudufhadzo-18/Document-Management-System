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
        private readonly INotificationService _notificationService;

        public DocumentService(ApplicationDbContext context, IAuditLogService auditLog, INotificationService notificationService)
        {
            _context = context;
            _auditLog = auditLog;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<DocumentSummaryDto>> GetAllAsync()
        {
            return await _context.Documents
                .Select(d => new DocumentSummaryDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    ReviewDate = d.ReviewDate,
                    Status = d.Status,
                    CategoryId = d.CategoryId,
                    CategoryName = _context.Categories
                        .Where(c => c.Id == d.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? "",
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentId == null
                        ? null
                        : _context.Departments
                            .Where(dep => dep.Id == d.DepartmentId)
                            .Select(dep => dep.Name)
                            .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<DocumentResponseDto?> GetByIdAsync(int id)
        {
            var doc = await _context.Documents
                .Where(d => d.Id == id)
                .Select(d => new DocumentResponseDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    DocumentType = d.DocumentType,
                    Tags = d.Tags,
                    EffectiveDate = d.EffectiveDate,
                    ReviewDate = d.ReviewDate,
                    CategoryName = _context.Categories
                        .Where(c => c.Id == d.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? "",
                    DepartmentName = d.DepartmentId == null
                        ? null
                        : _context.Departments
                            .Where(dep => dep.Id == d.DepartmentId)
                            .Select(dep => dep.Name)
                            .FirstOrDefault(),
                    OwnerName = d.OwnerId,
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

        public async Task<DocumentResponseDto> CreateAsync(DocumentCreateDto dto, string ownerId)
        {
            var document = new Document
            {
                Title = dto.Title,
                Description = dto.Description,
                DocumentType = dto.DocumentType,
                Tags = dto.Tags,
                EffectiveDate = dto.EffectiveDate,
                ReviewDate = dto.ReviewDate,
                CategoryId = dto.CategoryId,
                DepartmentId = dto.DepartmentId,
                OwnerId = ownerId,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(ownerId, document.Id, "Create", ipAddress: null);

            return await GetByIdAsync(document.Id) ?? throw new InvalidOperationException("Failed to load created document.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return false;

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        // The full document lifecycle. Each key maps to the statuses it's legally
        // allowed to move to next — this is the single source of truth for what
        // transitions are valid, enforced server-side regardless of what the UI offers.
        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            ["Draft"] = new[] { "Submitted" },
            ["Submitted"] = new[] { "UnderReview" },
            ["UnderReview"] = new[] { "Approved", "Rejected" },
            ["Approved"] = new[] { "Published" },
            ["Rejected"] = new[] { "Draft" },       // rejected documents go back for revision
            ["Published"] = new[] { "Archived", "Expired" },
            ["Expired"] = new[] { "Archived" },
            ["Archived"] = new[] { "Published" }    // restore
        };

        public async Task<DocumentResponseDto?> UpdateStatusAsync(int id, string newStatus, string userId, string? reason = null)
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

            var action = string.IsNullOrWhiteSpace(reason)
                ? $"StatusChange:{newStatus}"
                : $"StatusChange:{newStatus} — {reason}";

      
            await _auditLog.LogAsync(userId, id, action, ipAddress: null);

            // notify the owner, unless they're the one who made the change themselves
            if (document.OwnerId != userId)
            {
                var message = newStatus == "Rejected" && !string.IsNullOrWhiteSpace(reason)
                    ? $"Your document \"{document.Title}\" was rejected: {reason}"
                    : $"Your document \"{document.Title}\" status changed to {newStatus}.";

                await _notificationService.CreateAsync(document.OwnerId, message, id);
            }

            return await GetByIdAsync(id);
        }

        public async Task<IEnumerable<DocumentSummaryDto>> GetExpiringAsync(int withinDays)
        {
            var cutoff = DateTime.UtcNow.AddDays(withinDays);

            return await _context.Documents
                .Where(d => d.ReviewDate != null && d.ReviewDate <= cutoff && d.ReviewDate >= DateTime.UtcNow)
                .Select(d => new DocumentSummaryDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    ReviewDate = d.ReviewDate,
                    Status = d.Status,
                    CategoryId = d.CategoryId,
                    CategoryName = _context.Categories
                        .Where(c => c.Id == d.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? "",
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentId == null
                        ? null
                        : _context.Departments
                            .Where(dep => dep.Id == d.DepartmentId)
                            .Select(dep => dep.Name)
                            .FirstOrDefault()
                })
                .OrderBy(d => d.ReviewDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentSummaryDto>> GetExpiredAsync()
        {
            // "expired" here means the review date has passed but the document
            // hasn't actually been moved to the Expired status yet — these are
            // documents overdue for someone to act on
            return await _context.Documents
                .Where(d => d.ReviewDate != null && d.ReviewDate < DateTime.UtcNow && d.Status != "Expired" && d.Status != "Archived")
                .Select(d => new DocumentSummaryDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    DocumentType = d.DocumentType,
                    ReviewDate = d.ReviewDate,
                    Status = d.Status,
                    CategoryId = d.CategoryId,
                    CategoryName = _context.Categories
                        .Where(c => c.Id == d.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? "",
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentId == null
                        ? null
                        : _context.Departments
                            .Where(dep => dep.Id == d.DepartmentId)
                            .Select(dep => dep.Name)
                            .FirstOrDefault()
                })
                .OrderBy(d => d.ReviewDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentSummaryDto>> GetReviewDueAsync()
        {
            // same as GetExpiringAsync with a fixed 30-day window — kept as its
            // own method since "review due" is a distinct concept the UI/reports
            // will want to call directly without specifying a day count
            return await GetExpiringAsync(30);
        }

    }
}
