using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
using RecruitmentPlatformAPI.DTOs.Assessment.V2;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.Services.Assessment.V2;

namespace RecruitmentPlatformAPI.Controllers.Assessment
{
    /// <summary>
    /// LLM-powered dynamic assessments for job seekers (Assessment V2).
    /// </summary>
    [ApiController]
    [Route("api/assessment/v2")]
    [Authorize]
    [Produces("application/json")]
    public class AssessmentV2Controller : BaseApiController
    {
        private readonly IAssessmentServiceV2 _assessmentService;
        private readonly ILogger<AssessmentV2Controller> _logger;

        public AssessmentV2Controller(IAssessmentServiceV2 assessmentService, ILogger<AssessmentV2Controller> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        /// <summary>Check whether the current user is eligible to start a V2 assessment.</summary>
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

        /// <summary>Start a new V2 assessment attempt using LLM-generated questions.</summary>
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
                return BadRequest(new ApiErrorResponse("Cannot start V2 assessment. Check eligibility, claimed skills, or LLM availability."));
            }

            _logger.LogInformation("V2 Assessment started for user {UserId}, attempt {AttemptId}", userId, result.AttemptId);
            return Ok(new ApiResponse<StartAssessmentResponseDto>(result, "V2 Assessment started successfully"));
        }

        /// <summary>
        /// Explicitly resume an in-progress V2 assessment.
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
                if (result == null) return NotFound(new ApiErrorResponse("No V2 assessment in progress"));

                _logger.LogInformation("V2 Assessment resumed for user {UserId}", userId);
                return Ok(new ApiResponse<AssessmentStatusResponseDto>(result, "V2 Assessment resumed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(ex.Message));
            }
        }

        /// <summary>Get the current in-progress V2 assessment status.</summary>
        [HttpGet("current")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentStatusResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentStatus()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetCurrentStatusAsync(userId);
            if (result == null) return NotFound(new ApiErrorResponse("No V2 assessment in progress"));

            return Ok(new ApiResponse<AssessmentStatusResponseDto>(result));
        }

        /// <summary>Return the answered / unanswered status for every question in the active V2 attempt.</summary>
        [HttpGet("questions")]
        [ProducesResponseType(typeof(ApiResponse<List<AssessmentQuestionStatusDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetQuestionStatuses()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetQuestionStatusesAsync(userId);
            if (result == null) return NotFound(new ApiErrorResponse("No V2 assessment in progress"));

            return Ok(new ApiResponse<List<AssessmentQuestionStatusDto>>(result));
        }

        /// <summary>Return the next unanswered question for V2 assessment.</summary>
        [HttpGet("question")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNextQuestion()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetNextQuestionAsync(userId);
            if (result == null) return NotFound(new ApiErrorResponse("No more questions or V2 assessment not found"));

            return Ok(new ApiResponse<QuestionResponseDto>(result));
        }

        /// <summary>Return the question at the given 1-based position for V2 assessment.</summary>
        [HttpGet("question/{questionNumber:int}")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetQuestionByNumber(int questionNumber)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetQuestionByNumberAsync(userId, questionNumber);
            if (result == null) return NotFound(new ApiErrorResponse("Question not found or no V2 assessment in progress"));

            return Ok(new ApiResponse<QuestionResponseDto>(result));
        }

        /// <summary>Submit or overwrite an answer for a V2 question.</summary>
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
                return BadRequest(new ApiErrorResponse("Failed to submit answer. V2 Assessment may have expired or the question is invalid."));
            }

            return Ok(new ApiResponse<SubmitAnswerResponseDto>(result));
        }

        /// <summary>Finalise the V2 assessment and compute scores.</summary>
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
                return BadRequest(new ApiErrorResponse("Failed to complete V2 assessment. No assessment in progress."));
            }

            _logger.LogInformation("V2 Assessment completed for user {UserId}, score {Score}", userId, result.OverallScore);
            return Ok(new ApiResponse<AssessmentResultResponseDto>(result, "V2 Assessment completed successfully"));
        }

        /// <summary>Abandon the current in-progress V2 assessment.</summary>
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
                return BadRequest(new ApiErrorResponse("Failed to abandon V2 assessment. No assessment in progress."));
            }

            _logger.LogInformation("V2 Assessment abandoned for user {UserId}", userId);
            return Ok(new ApiResponse<bool>(true, "V2 Assessment abandoned successfully"));
        }

        /// <summary>Return the V2 assessment history for the current user.</summary>
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

        /// <summary>Return the full review result for a completed V2 attempt.</summary>
        [HttpGet("result/{attemptId:int}")]
        [ProducesResponseType(typeof(ApiResponse<AssessmentResultResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetResult(int attemptId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _assessmentService.GetResultAsync(userId, attemptId);
            if (result == null) return NotFound(new ApiErrorResponse("V2 Assessment result not found or not yet completed"));

            return Ok(new ApiResponse<AssessmentResultResponseDto>(result));
        }
    }
}
