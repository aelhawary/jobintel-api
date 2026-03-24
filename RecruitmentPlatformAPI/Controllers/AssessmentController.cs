using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RecruitmentPlatformAPI.DTOs.Assessment;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.Services.Assessment;

namespace RecruitmentPlatformAPI.Controllers
{
    /// <summary>
    /// Controller for job seeker skill assessments
    /// </summary>
    [ApiController]
    [Route("api/assessment")]
    [Authorize]
    [Produces("application/json")]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<AssessmentController> _logger;

        public AssessmentController(
            IAssessmentService assessmentService,
            ILogger<AssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        /// <summary>
        /// Check if user is eligible to start an assessment
        /// </summary>
        /// <returns>Eligibility status with details about requirements</returns>
        [HttpGet("eligibility")]
        [ProducesResponseType(typeof(ApiResponse<EligibilityResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckEligibility()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentService.CheckEligibilityAsync(userId);
            return Ok(new ApiResponse<EligibilityResponseDto>(result));
        }

        /// <summary>
        /// Start a new assessment
        /// </summary>
        /// <returns>Assessment details including question count and time limit</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(ApiResponse<StartAssessmentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> StartAssessment()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentService.StartAssessmentAsync(userId);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Cannot start assessment. Please check your eligibility first."));
            }

            _logger.LogInformation("Assessment started for user {UserId}, attempt {AttemptId}", userId, result.AttemptId);
            return Ok(new ApiResponse<StartAssessmentResponseDto>(result, "Assessment started successfully"));
        }

        /// <summary>
        /// Get current in-progress assessment status
        /// </summary>
        /// <returns>Current assessment status including progress and time remaining</returns>
        [HttpGet("current")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentStatusResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentStatus()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentService.GetCurrentStatusAsync(userId);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("No assessment in progress"));
            }

            return Ok(new ApiResponse<AssessmentStatusResponseDto>(result));
        }

        /// <summary>
        /// Get the next unanswered question
        /// </summary>
        /// <returns>Question details with options</returns>
        [HttpGet("question")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNextQuestion()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentService.GetNextQuestionAsync(userId);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("No more questions or assessment not found"));
            }

            return Ok(new ApiResponse<QuestionResponseDto>(result));
        }

        /// <summary>
        /// Submit an answer for a question
        /// </summary>
        /// <param name="dto">Answer submission with question ID and selected option index</param>
        /// <returns>Submission result with progress (no correctness info in exam mode)</returns>
        [HttpPost("answer")]
        [ProducesResponseType(typeof(ApiResponse<SubmitAnswerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _assessmentService.SubmitAnswerAsync(userId, dto);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Failed to submit answer. Assessment may have expired or question is invalid."));
            }

            return Ok(new ApiResponse<SubmitAnswerResponseDto>(result));
        }

        /// <summary>
        /// Complete the assessment and get results
        /// </summary>
        /// <returns>Assessment result with scores and performance level</returns>
        [HttpPost("complete")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentResultResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CompleteAssessment()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentService.CompleteAssessmentAsync(userId);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Failed to complete assessment. No assessment in progress."));
            }

            _logger.LogInformation("Assessment completed for user {UserId}, score {Score}", userId, result.OverallScore);
            return Ok(new ApiResponse<AssessmentResultResponseDto>(result, "Assessment completed successfully"));
        }

        /// <summary>
        /// Abandon the current assessment
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("abandon")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AbandonAssessment()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentService.AbandonAssessmentAsync(userId);
            if (!result)
            {
                return BadRequest(new ApiErrorResponse("Failed to abandon assessment. No assessment in progress."));
            }

            _logger.LogInformation("Assessment abandoned for user {UserId}", userId);
            return Ok(new ApiResponse<bool>(true, "Assessment abandoned successfully"));
        }

        /// <summary>
        /// Get assessment history
        /// </summary>
        /// <returns>List of all assessment attempts with scores</returns>
        [HttpGet("history")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentHistoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentService.GetHistoryAsync(userId);
            return Ok(new ApiResponse<AssessmentHistoryResponseDto>(result));
        }

        /// <summary>
        /// Get detailed result for a specific assessment attempt
        /// </summary>
        /// <param name="attemptId">The assessment attempt ID</param>
        /// <returns>Detailed result with question-by-question breakdown</returns>
        [HttpGet("result/{attemptId}")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentResultResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetResult(int attemptId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentService.GetResultAsync(userId, attemptId);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("Assessment result not found or not completed yet"));
            }

            return Ok(new ApiResponse<AssessmentResultResponseDto>(result));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
