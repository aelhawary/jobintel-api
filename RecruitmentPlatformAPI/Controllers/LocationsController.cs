using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get list of all active countries with bilingual support (65 countries, Arab countries prioritized)
        /// </summary>
        /// <param name="lang">Language code: "en" for English, "ar" for Arabic (default: "en")</param>
        /// <returns>List of countries with localized names, ISO codes, and phone codes</returns>
        [HttpGet("countries")]
        [ProducesResponseType(typeof(ApiResponse<List<CountryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCountries([FromQuery] string lang = "en")
        {
            var countries = await _context.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.NameEn)
                .ToListAsync();

            var result = countries.Select(c => new CountryDto
            {
                Id = c.Id,
                Name = lang.ToLower() == "ar" ? c.NameAr : c.NameEn,
                IsoCode = c.IsoCode,
                PhoneCode = c.PhoneCode
            }).ToList();

            return Ok(new ApiResponse<List<CountryDto>>(result));
        }

        /// <summary>
        /// Get list of all active languages with bilingual support (50 languages, Arabic and English prioritized)
        /// </summary>
        /// <param name="lang">Language code: "en" for English, "ar" for Arabic (default: "en")</param>
        /// <returns>List of languages with localized names and ISO codes</returns>
        [HttpGet("languages")]
        [ProducesResponseType(typeof(ApiResponse<List<LanguageDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLanguages([FromQuery] string lang = "en")
        {
            var languages = await _context.Languages
                .Where(l => l.IsActive)
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.NameEn)
                .ToListAsync();

            var result = languages.Select(l => new LanguageDto
            {
                Id = l.Id,
                Name = lang.ToLower() == "ar" ? l.NameAr : l.NameEn,
                IsoCode = l.IsoCode
            }).ToList();

            return Ok(new ApiResponse<List<LanguageDto>>(result));
        }
    }
}
