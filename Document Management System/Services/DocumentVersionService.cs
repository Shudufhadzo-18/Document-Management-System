using Document_Management_System.Data;
using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Document_Management_System.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Document_Management_System.Services
{
    public class DocumentVersionService: IDocumentVersionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly IAuditLogService _auditLog;

        public DocumentVersionService(
            ApplicationDbContext context,
            IFileStorageService fileStorage,
            IAuditLogService auditLog)
        {
            _context = context;
            _fileStorage = fileStorage;
            _auditLog = auditLog;
        }

        public async Task<DocumentVersionResponseDto> UploadAsync(
            DocumentVersionUploadDto dto,
            Stream fileStream,
            string fileName,
            string contentType,
            string uploadedBy)
        {
            // work out the next version number for this document
            var latestVersion = await _context.DocumentVersions
                .Where(v => v.DocumentId == dto.DocumentId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();

            var nextVersionNumber = (latestVersion?.VersionNumber ?? 0) + 1;

            var storedPath = await _fileStorage.SaveFileAsync(fileStream, fileName);

            var version = new DocumentVersion
            {
                DocumentId = dto.DocumentId,
                VersionNumber = nextVersionNumber,
                FilePath = storedPath,

                OriginalFileName = fileName,
                FileSize = fileStream.Length,
                MimeType = contentType, // controller should set this from IFormFile.ContentType
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.UtcNow,
                Notes = dto.Notes
            };

            _context.DocumentVersions.Add(version);
            await _context.SaveChangesAsync();

            // point the parent Document at this new version as the "current" one
            var document = await _context.Documents.FindAsync(dto.DocumentId);
            if (document != null)
            {
                document.CurrentVersionId = version.Id;
                document.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await _auditLog.LogAsync(uploadedBy, dto.DocumentId, "Upload", ipAddress: null);

            return new DocumentVersionResponseDto
            {
                Id = version.Id,
                VersionNumber = version.VersionNumber,
                FileName = fileName, // original name for display, not the stored GUID-prefixed one
                FileSize = version.FileSize,
                MimeType = version.MimeType,
                UploadedByName = uploadedBy,
                UploadedAt = version.UploadedAt,
                Notes = version.Notes
            };
        }

        public async Task<IEnumerable<DocumentVersionResponseDto>> GetVersionsForDocumentAsync(int documentId)
        {
            return await _context.DocumentVersions
                .Where(v => v.DocumentId == documentId)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new DocumentVersionResponseDto
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    FileName = v.FilePath, // ideally strip the GUID prefix before showing this — revisit in controller/mapping
                    FileSize = v.FileSize,
                    MimeType = v.MimeType,
                    UploadedByName = v.UploadedBy,
                    UploadedAt = v.UploadedAt,
                    Notes = v.Notes
                })
                .ToListAsync();
        }

        public async Task<DocumentVersion> GetVersionByIdAsync(int versionId)
        {
            return await _context.DocumentVersions.FindAsync(versionId);
        }
    }
}
