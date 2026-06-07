using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;

namespace RecruitmentPlatformAPI.Controllers.Common
{
    [ApiController]
    [Route("api/countries")]
    [Produces("application/json")]
    public class CountriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CountriesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of all countries ordered alphabetically.
        /// </summary>
        /// <param name="lang">Language code for localization (en or ar)</param>
        [HttpGet]
        public async Task<IActionResult> GetCountries([FromQuery] string lang = "en")
        {
            bool isArabic = lang.ToLower() == "ar";

            var countries = await _context.Countries
                .AsNoTracking()
                .Select(c => new CountryDto 
                { 
                    Id = c.Id, 
                    Name = isArabic ? (c.NameAr ?? c.NameEn) : c.NameEn 
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(countries);
        }

        /// <summary>
        /// Returns a list of cities belonging to the specified country, ordered alphabetically.
        /// </summary>
        /// <param name="id">The country ID</param>
        /// <param name="lang">Language code for localization (en or ar)</param>
        [HttpGet("{id}/cities")]
        public async Task<IActionResult> GetCitiesByCountry(int id, [FromQuery] string lang = "en")
        {
            bool isArabic = lang.ToLower() == "ar";

            var cities = await _context.Cities
                .AsNoTracking()
                .Where(c => c.CountryId == id)
                .Select(c => new CityDto 
                { 
                    Id = c.Id, 
                    Name = isArabic ? (c.NameAr ?? c.NameEn) : c.NameEn 
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(cities);
        }
    }
}
