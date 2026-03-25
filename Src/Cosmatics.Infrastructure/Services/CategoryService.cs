
using Cosmatics.Infrastructure.Persistense.Data;
using Cosmatics.Application.DTOs;
using Cosmatics.Models;

namespace Cosmatics.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepo;

    public CategoryService(IRepository<Category> categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _categoryRepo.GetAllAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _categoryRepo.GetByIdAsync(id);
    }

    public async Task<Category> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Title = dto.Title,
            ImageUrl = dto.ImageUrl
        };
        await _categoryRepo.AddAsync(category);
        return category;
    }

    public async Task UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category != null)
        {
            category.Title = dto.Title;
            category.ImageUrl = dto.ImageUrl;
            await _categoryRepo.UpdateAsync(category);
        }
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category != null)
        {
            await _categoryRepo.DeleteAsync(category);
        }
    }
}
