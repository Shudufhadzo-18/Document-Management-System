using static Document_Management_System.Models.DTOS.DocumentPermissionDTOs;

namespace Document_Management_System.Interfaces
{
    public interface IDocumentPermissionService
    {
        Task<IEnumerable<DocumentPermissionResponseDto>> GetForDocumentAsync(int documentId);
        Task<DocumentPermissionResponseDto> GrantAsync(int documentId, DocumentPermissionGrantDto dto, string grantedBy);
        Task<bool> RevokeAsync(int documentId, string userId);

        // the actual access check other services/controllers will call —
        // Admins bypass this entirely since they already have full access via role
        Task<bool> HasPermissionAsync(int documentId, string userId, string permission);
    }
}
