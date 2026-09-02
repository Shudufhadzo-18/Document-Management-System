using Document_Management_System.Data;
using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Document_Management_System.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Document_Management_System.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context= context;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
        {
            return await _context.Categories
           .Select(c => new CategoryResponseDto
           {
               Id = c.Id,
               Name = c.Name,
               ParentId = c.ParentId,
               CreatedAt = c.CreatedAt
           })
           .ToListAsync();
        }
       public async Task<CategoryResponseDto> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId,
                CreatedAt = category.CreatedAt
            };
        }
        public async Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto) {
            var category = new Category
            {
                Name = dto.Name,
                ParentId = dto.ParentId,
                CreatedAt = DateTime.UtcNow
            };
             _context.Categories.Add(category);

            await _context.SaveChangesAsync();
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId,
                CreatedAt = category.CreatedAt

            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;


        }
    }
}
