using Document_Management_System.Interfaces;
using Document_Management_System.Models.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace Document_Management_System.Controllers
{
  
        [ApiController]
        [Route("api/[controller]")]
        public class CategoriesController : ControllerBase
        {
            private readonly ICategoryService _categoryService;

            public CategoriesController(ICategoryService categoryService)
            {
                _categoryService = categoryService;
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
            {
                var categories = await _categoryService.GetAllAsync();
                return Ok(categories);
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<CategoryResponseDto>> GetById(int id)
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null) return NotFound(); // service returns null, controller translates that to a 404
                return Ok(category);
            }

            [HttpPost]
            public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CategoryCreateDto dto)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var created = await _categoryService.CreateAsync(dto);
                // 201 with a Location header pointing at GetById — standard REST practice for POST
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(int id)
            {
                var deleted = await _categoryService.DeleteAsync(id);
                if (!deleted) return NotFound();
                return NoContent(); // 204 — successful delete, nothing to return
            }
        }
}
