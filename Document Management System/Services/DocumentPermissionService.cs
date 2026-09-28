using Document_Management_System.Data;
using Document_Management_System.Interfaces;
using Document_Management_System.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Document_Management_System.Models.DTOS.DocumentPermissionDTOs;

namespace Document_Management_System.Services
{
    public class DocumentPermissionService : IDocumentPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DocumentPermissionService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<DocumentPermissionResponseDto>> GetForDocumentAsync(int documentId)
        {
            var permissions = await _context.DocumentPermissions
                .Where(p => p.DocumentId == documentId)
                .ToListAsync();

            var result = new List<DocumentPermissionResponseDto>();
            foreach (var p in permissions)
            {
                var user = await _userManager.FindByIdAsync(p.UserId);
                result.Add(new DocumentPermissionResponseDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserEmail = user?.Email ?? "Unknown user",
                    CanView = p.CanView,
                    CanDownload = p.CanDownload,
                    CanEdit = p.CanEdit,
                    CanDelete = p.CanDelete,
                    CanApprove = p.CanApprove,
                    GrantedAt = p.GrantedAt
                });
            }
            return result;
        }

        public async Task<DocumentPermissionResponseDto> GrantAsync(int documentId, DocumentPermissionGrantDto dto, string grantedBy)
        {
            // one permission row per user per document — re-granting updates
            // the existing row instead of creating duplicates
            var existing = await _context.DocumentPermissions
                .FirstOrDefaultAsync(p => p.DocumentId == documentId && p.UserId == dto.UserId);

            if (existing != null)
            {
                existing.CanView = dto.CanView;
                existing.CanDownload = dto.CanDownload;
                existing.CanEdit = dto.CanEdit;
                existing.CanDelete = dto.CanDelete;
                existing.CanApprove = dto.CanApprove;
                existing.GrantedAt = DateTime.UtcNow;
                existing.GrantedBy = grantedBy;
            }
            else
            {
                existing = new DocumentPermission
                {
                    DocumentId = documentId,
                    UserId = dto.UserId,
                    CanView = dto.CanView,
                    CanDownload = dto.CanDownload,
                    CanEdit = dto.CanEdit,
                    CanDelete = dto.CanDelete,
                    CanApprove = dto.CanApprove,
                    GrantedAt = DateTime.UtcNow,
                    GrantedBy = grantedBy
                };
                _context.DocumentPermissions.Add(existing);
            }

            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(dto.UserId);
            return new DocumentPermissionResponseDto
            {
                Id = existing.Id,
                UserId = existing.UserId,
                UserEmail = user?.Email ?? "Unknown user",
                CanView = existing.CanView,
                CanDownload = existing.CanDownload,
                CanEdit = existing.CanEdit,
                CanDelete = existing.CanDelete,
                CanApprove = existing.CanApprove,
                GrantedAt = existing.GrantedAt
            };
        }

        public async Task<bool> RevokeAsync(int documentId, string userId)
        {
            var permission = await _context.DocumentPermissions
                .FirstOrDefaultAsync(p => p.DocumentId == documentId && p.UserId == userId);
            if (permission == null) return false;

            _context.DocumentPermissions.Remove(permission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasPermissionAsync(int documentId, string userId, string permission)
        {
            var record = await _context.DocumentPermissions
                .FirstOrDefaultAsync(p => p.DocumentId == documentId && p.UserId == userId);
            if (record == null) return false;

            return permission switch
            {
                "View" => record.CanView,
                "Download" => record.CanDownload,
                "Edit" => record.CanEdit,
                "Delete" => record.CanDelete,
                "Approve" => record.CanApprove,
                _ => false
            };
        }
    }
}
