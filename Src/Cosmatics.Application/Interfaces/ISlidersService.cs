using Cosmatics.Application.DTOs;

namespace Cosmatics.Infrastructure.Services;

public interface ISlidersService
{
    Task<IEnumerable<SliderDto>> GetAllSlidersAsync();
    Task<SliderDto> GetSliderByIdAsync(int id);
    Task<SliderDto> CreateSliderAsync(CreateSliderDto createSliderDto);
    Task<SliderDto> UpdateSliderAsync(int id, UpdateSliderDto updateSliderDto);
    Task<bool> DeleteSliderAsync(int id);
}
