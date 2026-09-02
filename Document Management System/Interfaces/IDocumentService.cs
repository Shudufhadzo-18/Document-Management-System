using Document_Management_System.Models.DTOS;
using static Document_Management_System.Models.DTOS.DocumentResponseDto;

namespace Document_Management_System.Interfaces
{
    public interface IDocumentService
    {
        Task<IEnumerable<DocumentSummaryDto>> GetAllAsync();
        Task<DocumentResponseDto> GetByIdAsync(int id);
        Task<DocumentResponseDto> UpdateStatusAsync(int id, string newStatus, string userId);
        Task<DocumentResponseDto> CreateAsync(DocumentCreateDto dto, string ownerId, string? ipAddress);
        Task<bool> DeleteAsync(int id);
       
    }
}
