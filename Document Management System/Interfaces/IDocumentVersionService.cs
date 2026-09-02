using Document_Management_System.Models.DTOS;
using Document_Management_System.Models.Entities;

namespace Document_Management_System.Interfaces
{
    public interface IDocumentVersionService
    {
        // fileStream/fileName come separately from the DTO since the file itself
        // arrives as an IFormFile in the controller, not as part of the JSON body
        Task<DocumentVersionResponseDto> UploadAsync(DocumentVersionUploadDto dto,
            Stream fileStream,
            string FileName,
            string contentType,
            string UploadedByName);

       
        Task<IEnumerable<DocumentVersionResponseDto>> GetVersionsForDocumentAsync(int documentId);
        Task<DocumentVersion> GetVersionByIdAsync(int versionId);
    }
}
