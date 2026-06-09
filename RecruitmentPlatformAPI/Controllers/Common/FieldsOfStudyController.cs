using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;

namespace RecruitmentPlatformAPI.Controllers.Common
{
    [ApiController]
        [Route("api/fields-of-study")]
    [Produces("application/json")]
    public class FieldsOfStudyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FieldsOfStudyController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get list of all active fields of study with bilingual support
        /// </summary>
        /// <param name="lang">Language code: "en" for English, "ar" for Arabic (default: "en")</param>
        /// <returns>List of fields of study with localized names</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<FieldOfStudyDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFieldsOfStudy([FromQuery] string lang = "en")
        {
            var isArabic = lang.ToLower() == "ar";
            
            var result = await _context.FieldsOfStudy
                .AsNoTracking()
                .Where(f => f.IsActive)
                .Select(f => new FieldOfStudyDto
                {
                    Id = f.Id,
                    Name = isArabic ? (f.NameAr ?? f.NameEn) : f.NameEn
                })
                .OrderBy(dto => dto.Name)
                .ToListAsync();

            return Ok(new ApiResponse<List<FieldOfStudyDto>>(result));
        }
    }
}
