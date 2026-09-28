using Document_Management_System.Models.DTOS;

namespace Document_Management_System.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentResponseDTO>> GetAllAsync();
        Task<DepartmentResponseDTO?> GetByIdAsync(int id);
        Task<DepartmentResponseDTO> CreateAsync(DepartmentCreateDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
