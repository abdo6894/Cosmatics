
using Cosmatics.Infrastructure.Persistense.Data;
using Cosmatics.Application.DTOs;
using Cosmatics.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Cosmatics.Application.Common;

namespace Cosmatics.Infrastructure.Services
{
    public class SlidersService : ISlidersService
    {
        private readonly IRepository<Slider> _sliderRepo;
        private readonly ICacheService _cacheService;
        public SlidersService(IRepository<Slider> sliderRepo,ICacheService cacheService)
        {
            _sliderRepo = sliderRepo;
           _cacheService = cacheService;
        }

        public async Task<IEnumerable<SliderDto>> GetAllSlidersAsync()
        {
            var key = "Get_All:sliders";
            var cachedSliders = await _cacheService.GetDataAsync<IEnumerable<SliderDto>>(key);
            if (cachedSliders is not null)
            {
                Console.WriteLine("Cache Vistied");
                return cachedSliders!;

            }
            var sliders = await _sliderRepo.GetAllAsync();
                var sliderDtos = sliders.Select(s => new SliderDto
                {
                    Id = s.Id,
                    CouponCode = s.CouponCode,
                    DiscountPercent = s.DiscountPercent,
                    DescriptionTitle1 = s.DescriptionTitle1,
                    DescriptionTitle2 = s.DescriptionTitle2,
                    ImageUrl = s.ImageUrl
                }).ToList();

                await _cacheService.SetDataAsync(key, sliderDtos, TimeSpan.FromMinutes(10)); 
            return sliderDtos;
        }

        public async Task<SliderDto> GetSliderByIdAsync(int id)
        {
            var key = "Get_slider:";

            var cachedSlider = await _cacheService.GetDataAsync<SliderDto>(key);
            if (cachedSlider is null)
            {
                Console.WriteLine("Cache Vistied");
                return cachedSlider!;
            }
                var slider = await _sliderRepo.GetByIdAsync(id);
                var sliderDto = new SliderDto
                {
                    Id = slider!.Id,
                    CouponCode = slider.CouponCode,
                    DiscountPercent = slider.DiscountPercent,
                    DescriptionTitle1 = slider.DescriptionTitle1,
                    DescriptionTitle2 = slider.DescriptionTitle2,
                    ImageUrl = slider.ImageUrl
                };
                await _cacheService.SetDataAsync(key, sliderDto, TimeSpan.FromMinutes(10));
                   return sliderDto;




        }

        public async Task<SliderDto> CreateSliderAsync(CreateSliderDto createSliderDto)
        {
            var slider = new Slider
            {
                CouponCode = createSliderDto.CouponCode,
                DiscountPercent = createSliderDto.DiscountPercent,
                DescriptionTitle1 = createSliderDto.DescriptionTitle1,
                DescriptionTitle2 = createSliderDto.DescriptionTitle2,
                ImageUrl = createSliderDto.ImageUrl
            };

            await _sliderRepo.AddAsync(slider);
            await _cacheService.RemoveDataAsync("Get_All:sliders");
            await _cacheService.RemoveDataAsync($"Get_slider:{slider.Id}");

            return new SliderDto
            {
                Id = slider.Id,
                CouponCode = slider.CouponCode,
                DiscountPercent = slider.DiscountPercent,
                DescriptionTitle1 = slider.DescriptionTitle1,
                DescriptionTitle2 = slider.DescriptionTitle2,
                ImageUrl = slider.ImageUrl
            };
        }

        public async Task<SliderDto> UpdateSliderAsync(int id, UpdateSliderDto updateSliderDto)
        {
            var slider = await _sliderRepo.GetByIdAsync(id);
            if (slider == null) return null!;

            if (updateSliderDto.CouponCode != null) slider.CouponCode = updateSliderDto.CouponCode;
            if (updateSliderDto.DiscountPercent.HasValue) slider.DiscountPercent = updateSliderDto.DiscountPercent.Value;
            if (updateSliderDto.DescriptionTitle1 != null) slider.DescriptionTitle1 = updateSliderDto.DescriptionTitle1;
            if (updateSliderDto.DescriptionTitle2 != null) slider.DescriptionTitle2 = updateSliderDto.DescriptionTitle2;
            if (updateSliderDto.ImageUrl != null) slider.ImageUrl = updateSliderDto.ImageUrl;

            await _sliderRepo.UpdateAsync(slider);
            await _cacheService.RemoveDataAsync("Get_All:sliders");
            await _cacheService.RemoveDataAsync($"Get_slider:${slider.Id}");
            return new SliderDto
            {
                Id = slider.Id,
                CouponCode = slider.CouponCode,
                DiscountPercent = slider.DiscountPercent,
                DescriptionTitle1 = slider.DescriptionTitle1,
                DescriptionTitle2 = slider.DescriptionTitle2,
                ImageUrl = slider.ImageUrl
            };
        }

        public async Task<bool> DeleteSliderAsync(int id)
        {
            var slider = await _sliderRepo.GetByIdAsync(id);
            if (slider == null) return false;

            await _sliderRepo.DeleteAsync(slider);
            await _cacheService.RemoveDataAsync("Get_All:sliders");
            await _cacheService.RemoveDataAsync($"Get_slider:${slider.Id}");
            return true;
        }
    }
}
