
using Document_Management_System.Models.DTOS;
namespace Document_Management_System.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllAsync();
        Task<CategoryResponseDto> GetByIdAsync(int id);
        Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
