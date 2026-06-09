using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
using RecruitmentPlatformAPI.DTOs.Assessment;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.Services.Assessment;

namespace RecruitmentPlatformAPI.Controllers.Assessment
{
    /// <summary>
    /// Skill-based assessments for job seekers.
    /// Questions are selected from the candidate's claimed-skills profile and
    /// filtered by role family and seniority level.
    /// </summary>
    [ApiController]
    [Route("api/assessment")]
    [Authorize]
    [Produces("application/json")]
    public class AssessmentController : BaseApiController
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<AssessmentController> _logger;

        public AssessmentController(IAssessmentService assessmentService, ILogger<AssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // ELIGIBILITY
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Check whether the current user is eligible to start an assessment.</summary>
        [HttpGet("eligibility")]
        [ProducesResponseType(typeof(ApiResponse<EligibilityResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckEligibility()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.CheckEligibilityAsync(userId);
            return Ok(new ApiResponse<EligibilityResponseDto>(result));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // START
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Start a new assessment attempt.
        /// Optionally pass a body with explicit skill IDs; if omitted the server
        /// snapshots skills from the job-seeker profile.
        /// </summary>
        [HttpPost("start")]
        [ProducesResponseType(typeof(ApiResponse<StartAssessmentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> StartAssessment([FromBody] StartAssessmentRequestDto? request = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.StartAssessmentAsync(userId, request);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Cannot start assessment. Check eligibility and claimed skills."));
            }

            _logger.LogInformation("Assessment started for user {UserId}, attempt {AttemptId}", userId, result.AttemptId);
            return Ok(new ApiResponse<StartAssessmentResponseDto>(result, "Assessment started successfully"));
        }

        /// <summary>
        /// Explicitly resume an in-progress assessment.
        /// Fails if the maximum resume limit is exceeded to prevent cheating.
        /// </summary>
        [HttpPost("resume")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentStatusResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResumeAssessment()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            try
            {
                var result = await _assessmentService.ResumeAssessmentAsync(userId);
                if (result == null) return NotFound(new ApiErrorResponse("No assessment in progress"));

                _logger.LogInformation("Assessment resumed for user {UserId}", userId);
                return Ok(new ApiResponse<AssessmentStatusResponseDto>(result, "Assessment resumed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(ex.Message));
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // STATUS
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Get the current in-progress assessment status.
        /// If the attempt has expired it is auto-submitted and the completed status is returned.
        /// </summary>
        [HttpGet("current")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentStatusResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentStatus()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetCurrentStatusAsync(userId);
            if (result == null) return NotFound(new ApiErrorResponse("No assessment in progress"));

            return Ok(new ApiResponse<AssessmentStatusResponseDto>(result));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // QUESTION OVERVIEW
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Return the answered / unanswered status for every question in the active attempt.</summary>
        [HttpGet("questions")]
        [ProducesResponseType(typeof(ApiResponse<List<AssessmentQuestionStatusDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetQuestionStatuses()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetQuestionStatusesAsync(userId);
            if (result == null) return NotFound(new ApiErrorResponse("No assessment in progress"));

            return Ok(new ApiResponse<List<AssessmentQuestionStatusDto>>(result));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // QUESTION NAVIGATION
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Return the next unanswered question. Returns 404 when all questions are answered.</summary>
        [HttpGet("question")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNextQuestion()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetNextQuestionAsync(userId);
            if (result == null) return NotFound(new ApiErrorResponse("No more questions or assessment not found"));

            return Ok(new ApiResponse<QuestionResponseDto>(result));
        }

        /// <summary>
        /// Return the question at the given 1-based position.
        /// If the question has already been answered the previously selected index is included,
        /// enabling non-linear navigation and answer review during the exam.
        /// </summary>
        [HttpGet("question/{questionNumber:int}")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetQuestionByNumber(int questionNumber)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetQuestionByNumberAsync(userId, questionNumber);
            if (result == null) return NotFound(new ApiErrorResponse("Question not found or no assessment in progress"));

            return Ok(new ApiResponse<QuestionResponseDto>(result));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // ANSWER
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Submit or overwrite an answer for a question.
        /// Overwriting an existing answer does not change the answered count.
        /// No correctness information is returned while the exam is in progress.
        /// </summary>
        [HttpPost("answer")]
        [ProducesResponseType(typeof(ApiResponse<SubmitAnswerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _assessmentService.SubmitAnswerAsync(userId, dto);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Failed to submit answer. Assessment may have expired or the question is invalid."));
            }

            return Ok(new ApiResponse<SubmitAnswerResponseDto>(result));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // COMPLETE / ABANDON
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Finalise the assessment and compute scores.
        /// Unanswered questions count as incorrect (partial submission is supported).
        /// </summary>
        [HttpPost("complete")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentResultResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CompleteAssessment()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.CompleteAssessmentAsync(userId);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Failed to complete assessment. No assessment in progress."));
            }

            _logger.LogInformation("Assessment completed for user {UserId}, score {Score}", userId, result.OverallScore);
            return Ok(new ApiResponse<AssessmentResultResponseDto>(result, "Assessment completed successfully"));
        }

        /// <summary>Abandon the current in-progress assessment.</summary>
        [HttpPost("abandon")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AbandonAssessment()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.AbandonAssessmentAsync(userId);
            if (!result)
            {
                return BadRequest(new ApiErrorResponse("Failed to abandon assessment. No assessment in progress."));
            }

            _logger.LogInformation("Assessment abandoned for user {UserId}", userId);
            return Ok(new ApiResponse<bool>(true, "Assessment abandoned successfully"));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // HISTORY & RESULTS
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Return the assessment history for the current user.</summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentHistoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetHistoryAsync(userId);
            return Ok(new ApiResponse<AssessmentHistoryResponseDto>(result));
        }

        /// <summary>
        /// Return the full review result for a completed attempt.
        /// Includes per-question breakdown with correct answers, explanations, and skill attribution.
        /// </summary>
        [HttpGet("result/{attemptId:int}")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentResultResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetResult(int attemptId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetResultAsync(userId, attemptId);
            if (result == null) return NotFound(new ApiErrorResponse("Assessment result not found or not yet completed"));

            return Ok(new ApiResponse<AssessmentResultResponseDto>(result));
        }
    }
}
