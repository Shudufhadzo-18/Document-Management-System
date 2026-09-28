using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Document_Management_System.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentResponseDTO>>> GetAll()
        {
            return Ok(await _departmentService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentResponseDTO>> GetById(int id)
        {
            var dept = await _departmentService.GetByIdAsync(id);
            if (dept == null) return NotFound();
            return Ok(dept);
        }

        [HttpPost]
        public async Task<ActionResult<DepartmentResponseDTO>> Create([FromBody] DepartmentCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _departmentService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
