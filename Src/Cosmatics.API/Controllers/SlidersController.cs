using Cosmatics.Infrastructure.Services;
using Cosmatics.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cosmatics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlidersController : ControllerBase
    {
        private readonly ISlidersService _slidersService;

        public SlidersController(ISlidersService slidersService)
        {
            _slidersService = slidersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _slidersService.GetAllSlidersAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var slider = await _slidersService.GetSliderByIdAsync(id);
            if (slider == null) return NotFound(new { message = "Slider not found." });
            return Ok(slider);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSliderDto createSliderDto)
        {
            var slider = await _slidersService.CreateSliderAsync(createSliderDto);
            return CreatedAtAction(nameof(GetById), new { id = slider.Id }, slider);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSliderDto updateSliderDto)
        {
            var slider = await _slidersService.UpdateSliderAsync(id, updateSliderDto);
            if (slider == null) return NotFound(new { message = "Slider not found." });
            return Ok(slider);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _slidersService.DeleteSliderAsync(id);
            if (!result) return NotFound(new { message = "Slider not found." });
            return Ok(new { message = "Slider deleted." });
        }
    }
}
