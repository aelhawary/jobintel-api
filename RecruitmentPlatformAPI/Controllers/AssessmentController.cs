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
        private readonly IAssessmentV2Service _assessmentV2Service;
        private readonly ILogger<AssessmentController> _logger;

        public AssessmentController(
            IAssessmentService assessmentService,
            IAssessmentV2Service assessmentV2Service,
            ILogger<AssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _assessmentV2Service = assessmentV2Service;
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

        /// <summary>
        /// Check eligibility for claimed-skills validation assessment (v2)
        /// </summary>
        [HttpGet("v2/eligibility")]
        [ProducesResponseType(typeof(ApiResponse<EligibilityV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckEligibilityV2()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentV2Service.CheckEligibilityAsync(userId);
            return Ok(new ApiResponse<EligibilityV2ResponseDto>(result));
        }

        /// <summary>
        /// Start a new claimed-skills validation assessment (v2)
        /// </summary>
        [HttpPost("v2/start")]
        [ProducesResponseType(typeof(ApiResponse<StartAssessmentV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> StartAssessmentV2()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentV2Service.StartAssessmentAsync(userId);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Cannot start v2 assessment. Please check your eligibility and claimed skills first."));
            }

            _logger.LogInformation("V2 assessment started for user {UserId}, attempt {AttemptId}", userId, result.AttemptId);
            return Ok(new ApiResponse<StartAssessmentV2ResponseDto>(result, "Assessment v2 started successfully"));
        }

        /// <summary>
        /// Get current in-progress assessment status for v2
        /// </summary>
        [HttpGet("v2/current")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentStatusResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentStatusV2()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentV2Service.GetCurrentStatusAsync(userId);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("No v2 assessment in progress"));
            }

            return Ok(new ApiResponse<AssessmentStatusResponseDto>(result));
        }

        /// <summary>
        /// Get the next unanswered question for v2
        /// </summary>
        [HttpGet("v2/question")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNextQuestionV2()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentV2Service.GetNextQuestionAsync(userId);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("No more questions or v2 assessment not found"));
            }

            return Ok(new ApiResponse<QuestionResponseDto>(result));
        }

        /// <summary>
        /// Submit an answer for v2
        /// </summary>
        [HttpPost("v2/answer")]
        [ProducesResponseType(typeof(ApiResponse<SubmitAnswerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SubmitAnswerV2([FromBody] SubmitAnswerRequestDto dto)
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

            var result = await _assessmentV2Service.SubmitAnswerAsync(userId, dto);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Failed to submit answer for v2 assessment. Assessment may have expired or question is invalid."));
            }

            return Ok(new ApiResponse<SubmitAnswerResponseDto>(result));
        }

        /// <summary>
        /// Complete v2 assessment and get skill-based results
        /// </summary>
        [HttpPost("v2/complete")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentResultV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CompleteAssessmentV2()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentV2Service.CompleteAssessmentAsync(userId);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Failed to complete v2 assessment. No v2 assessment in progress."));
            }

            _logger.LogInformation("V2 assessment completed for user {UserId}, score {Score}", userId, result.OverallScore);
            return Ok(new ApiResponse<AssessmentResultV2ResponseDto>(result, "Assessment v2 completed successfully"));
        }

        /// <summary>
        /// Abandon v2 assessment
        /// </summary>
        [HttpPost("v2/abandon")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AbandonAssessmentV2()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentV2Service.AbandonAssessmentAsync(userId);
            if (!result)
            {
                return BadRequest(new ApiErrorResponse("Failed to abandon v2 assessment. No v2 assessment in progress."));
            }

            _logger.LogInformation("V2 assessment abandoned for user {UserId}", userId);
            return Ok(new ApiResponse<bool>(true, "Assessment v2 abandoned successfully"));
        }

        /// <summary>
        /// Get v2 assessment history
        /// </summary>
        [HttpGet("v2/history")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentHistoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHistoryV2()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentV2Service.GetHistoryAsync(userId);
            return Ok(new ApiResponse<AssessmentHistoryResponseDto>(result));
        }

        /// <summary>
        /// Get detailed v2 result for a specific attempt
        /// </summary>
        [HttpGet("v2/result/{attemptId}")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentResultV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetResultV2(int attemptId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _assessmentV2Service.GetResultAsync(userId, attemptId);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("V2 assessment result not found or not completed yet"));
            }

            return Ok(new ApiResponse<AssessmentResultV2ResponseDto>(result));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
