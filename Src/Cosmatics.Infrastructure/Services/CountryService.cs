using Cosmatics.Infrastructure.Services;
using Cosmatics.Infrastructure.Persistense.Data;
using Cosmatics.Models;

namespace Cosmatics.Infrastructure.Services;

public class CountryService : ICountryService
{
    private readonly IRepository<CountryCode> _repository;

    public CountryService(IRepository<CountryCode> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CountryCode>> GetAllCountriesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CountryCode?> GetCountryByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<(bool success, string message, CountryCode? country)> CreateCountryAsync(CountryCode country)
    {
        var existing = await _repository.FindAsync(c => c.Code == country.Code || c.Name == country.Name);
        if (existing.Any())
        {
            return (false, "Country code or name already exists.", null);
        }

        await _repository.AddAsync(country);
        return (true, "Country created successfully.", country);
    }

    public async Task<(bool success, string message, CountryCode? country)> UpdateCountryAsync(int id, CountryCode countryDto)
    {
        var country = await _repository.GetByIdAsync(id);
        if (country == null)
        {
            return (false, "Country not found.", null);
        }

        var existingConflict = await _repository.FindAsync(c =>
            c.Id != id && (c.Code == countryDto.Code || c.Name == countryDto.Name));

        if (existingConflict.Any())
        {
            return (false, "Country code or name already exists.", null);
        }

        country.Code = countryDto.Code;
        country.Name = countryDto.Name;

        await _repository.UpdateAsync(country);

        return (true, "Country updated successfully.", country);
    }

    public async Task<(bool success, string message)> DeleteCountryAsync(int id)
    {
        var country = await _repository.GetByIdAsync(id);
        if (country == null)
        {
            return (false, "Country not found.");
        }

        await _repository.DeleteAsync(country);
        return (true, "Country deleted successfully.");
    }
}