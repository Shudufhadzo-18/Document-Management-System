using static Document_Management_System.Models.DTOS.DocumentCommentDTOs;

namespace Document_Management_System.Interfaces
{
    public interface IDocumentCommentService
    {
        Task<IEnumerable<DocumentCommentResponseDto>> GetForDocumentAsync(int documentId);
        Task<DocumentCommentResponseDto> AddAsync(int documentId, string content, string userId);
        Task<bool> DeleteAsync(int commentId, string requestingUserId, bool isAdmin);
    }
}
