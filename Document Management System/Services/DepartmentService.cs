using Document_Management_System.Data;
using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Document_Management_System.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Document_Management_System.Services
{
    
        public class DepartmentService : IDepartmentService
        {
            private readonly ApplicationDbContext _context;

            public DepartmentService(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<DepartmentResponseDTO>> GetAllAsync()
            {
                return await _context.Departments
                    .Select(d => new DepartmentResponseDTO
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        DocumentCount = _context.Documents.Count(doc => doc.DepartmentId == d.Id),
                        CreatedAt = d.CreatedAt
                    })
                    .ToListAsync();
            }

            public async Task<DepartmentResponseDTO?> GetByIdAsync(int id)
            {
                var dept = await _context.Departments.FindAsync(id);
                if (dept == null) return null;

                return new DepartmentResponseDTO
                {
                    Id = dept.Id,
                    Name = dept.Name,
                    Description = dept.Description,
                    DocumentCount = await _context.Documents.CountAsync(d => d.DepartmentId == id),
                    CreatedAt = dept.CreatedAt
                };
            }

            public async Task<DepartmentResponseDTO> CreateAsync(DepartmentCreateDTO dto)
            {
                var department = new Department
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                return new DepartmentResponseDTO
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    DocumentCount = 0,
                    CreatedAt = department.CreatedAt
                };
            }

            public async Task<bool> DeleteAsync(int id)
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null) return false;

                // documents keep their DepartmentId as null rather than being deleted —
                // a department going away shouldn't destroy document history
                var docsInDept = await _context.Documents.Where(d => d.DepartmentId == id).ToListAsync();
                foreach (var doc in docsInDept)
                {
                    doc.DepartmentId = null;
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }

