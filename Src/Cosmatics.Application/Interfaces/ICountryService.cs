using Cosmatics.Models;

namespace Cosmatics.Infrastructure.Services;

public interface ICountryService
{
    Task<IEnumerable<CountryCode>> GetAllCountriesAsync();
    Task<CountryCode?> GetCountryByIdAsync(int id);
    Task<(bool success, string message, CountryCode? country)> CreateCountryAsync(CountryCode country);
    Task<(bool success, string message, CountryCode? country)> UpdateCountryAsync(int id, CountryCode countryDto);
    Task<(bool success, string message)> DeleteCountryAsync(int id);
}