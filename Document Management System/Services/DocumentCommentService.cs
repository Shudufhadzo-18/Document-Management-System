using Document_Management_System.Data;
using Document_Management_System.Interfaces;
using Document_Management_System.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Document_Management_System.Models.DTOS.DocumentCommentDTOs;

namespace Document_Management_System.Services
{
   
        public class DocumentCommentService : IDocumentCommentService
        {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditLogService _auditLog;
        private readonly INotificationService _notificationService;

        public DocumentCommentService(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IAuditLogService auditLog,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _auditLog = auditLog;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<DocumentCommentResponseDto>> GetForDocumentAsync(int documentId)
            {
                var comments = await _context.DocumentComments
                    .Where(c => c.DocumentId == documentId)
                    .OrderBy(c => c.CreatedAt) // oldest first — reads like a conversation thread
                    .ToListAsync();

                var result = new List<DocumentCommentResponseDto>();
                foreach (var c in comments)
                {
                    var user = await _userManager.FindByIdAsync(c.UserId);
                    result.Add(new DocumentCommentResponseDto
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        UserEmail = user?.Email ?? "Unknown user",
                        Content = c.Content,
                        CreatedAt = c.CreatedAt
                    });
                }
                return result;
            }

            public async Task<DocumentCommentResponseDto> AddAsync(int documentId, string content, string userId)
            {
                var comment = new DocumentComment
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DocumentComments.Add(comment);
                await _context.SaveChangesAsync();

            await _auditLog.LogAsync(userId, documentId, "Comment", ipAddress: null);

            var document = await _context.Documents.FindAsync(documentId);
            if (document != null && document.OwnerId != userId)
            {
                await _notificationService.CreateAsync(
                    document.OwnerId,
                    $"New comment on \"{document.Title}\".",
                    documentId);
            }

            var user = await _userManager.FindByIdAsync(userId);
                return new DocumentCommentResponseDto
                {
                    Id = comment.Id,
                    UserId = userId,
                    UserEmail = user?.Email ?? "Unknown user",
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt
                };
            }

            public async Task<bool> DeleteAsync(int commentId, string requestingUserId, bool isAdmin)
            {
                var comment = await _context.DocumentComments.FindAsync(commentId);
                if (comment == null) return false;

                // only the comment's author or an Admin can delete it — not just anyone with access to the document
                if (comment.UserId != requestingUserId && !isAdmin) return false;

                _context.DocumentComments.Remove(comment);
                await _context.SaveChangesAsync();
                return true;
            }
        }
}
