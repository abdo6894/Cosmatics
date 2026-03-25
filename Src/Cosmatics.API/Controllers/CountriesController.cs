using Cosmatics.Infrastructure.Services;

using Cosmatics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cosmatics.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;

    public CountriesController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var countries = await _countryService.GetAllCountriesAsync();
        return Ok(countries);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var country = await _countryService.GetCountryByIdAsync(id);
        if (country == null)
            return NotFound(new { message = "Country not found." });

        return Ok(country);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CountryCode country)
    {
        var (success, message, createdCountry) = await _countryService.CreateCountryAsync(country);

        if (!success)
            return BadRequest(new { message });

        return CreatedAtAction(nameof(GetById), new { id = createdCountry!.Id }, createdCountry);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CountryCode countryDto)
    {
        if (id != countryDto.Id)
            return BadRequest(new { message = "ID mismatch." });

        var (success, message, updatedCountry) = await _countryService.UpdateCountryAsync(id, countryDto);

        if (!success)
        {
            if (message.Contains("not found"))
                return NotFound(new { message });

            return BadRequest(new { message });
        }

        return Ok(updatedCountry);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _countryService.DeleteCountryAsync(id);

        if (!success)
            return NotFound(new { message });

        return Ok(new { message });
    }
}