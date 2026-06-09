using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Controllers.Common
{
    [ApiController]
        [Route("api/languages")]
    [Produces("application/json")]
    public class LanguagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LanguagesController(AppDbContext context)
        {
            _context = context;
        }



        /// <summary>
        /// Get list of all active languages with bilingual support (50 languages, Arabic and English prioritized)
        /// </summary>
        /// <param name="lang">Language code: "en" for English, "ar" for Arabic (default: "en")</param>
        /// <returns>List of languages with localized names and ISO codes</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<LanguageDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLanguages([FromQuery] string lang = "en")
        {
            var isArabic = lang.ToLower() == "ar";
            
            var result = await _context.Languages
                .AsNoTracking()
                .Where(l => l.IsActive)
                .Select(l => new LanguageDto
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = isArabic ? (l.NameAr ?? l.NameEn) : l.NameEn
                })
                .OrderBy(dto => dto.Name)
                .ToListAsync();

            return Ok(new ApiResponse<List<LanguageDto>>(result));
        }
    }
}
