using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Assessment;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Assessment;
using JobSeekerModel = RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker;

namespace RecruitmentPlatformAPI.Services.Assessment
{
    /// <summary>
    /// Claimed-skills assessment service.
    /// Questions are selected from the job seeker's claimed skill set and
    /// filtered by role family and seniority level.
    /// </summary>
    public class AssessmentService : IAssessmentService
    {
        private const int AlgorithmVersion = 2;

        private readonly AppDbContext _context;
        private readonly ILogger<AssessmentService> _logger;

        public AssessmentService(AppDbContext context, ILogger<AssessmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Eligibility

        public async Task<EligibilityResponseDto> CheckEligibilityAsync(int userId)
        {
            try
            {
                var result = new EligibilityResponseDto();

                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.AccountType != AccountType.JobSeeker)
                {
                    result.Reason = "Only job seekers can take assessments";
                    return result;
                }

                var jobSeeker = await _context.JobSeekers
                    .Include(js => js.JobTitle)
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    result.Reason = "Job seeker profile not found";
                    return result;
                }

                result.HasCompletedProfile = user.ProfileCompletionStep >= 4;
                if (!result.HasCompletedProfile)
                {
                    result.Reason = "Please complete your profile before taking an assessment";
                    return result;
                }

                result.HasJobTitle = jobSeeker.JobTitleId.HasValue;
                if (!result.HasJobTitle)
                {
                    result.Reason = "Please set your job title before taking an assessment";
                    return result;
                }

                var claimedSkills = await GetClaimedSkillsAsync(jobSeeker.Id);
                result.ClaimedSkillsCount = claimedSkills.Count;
                result.HasClaimedSkills = claimedSkills.Count > 0;
                result.ClaimedSkills = claimedSkills
                    .Select(s => new AssessmentSkillLiteDto { SkillId = s.SkillId, SkillName = s.SkillName })
                    .ToList();

                if (!result.HasClaimedSkills)
                {
                    result.Reason = "Please select at least one skill before taking an assessment";
                    return result;
                }

                // Shared lock: a user may only have one active in-progress assessment at a time.
                var inProgressAttempt = await _context.AssessmentAttempts
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress);

                result.HasInProgressAssessment = inProgressAttempt != null;
                if (result.HasInProgressAssessment)
                {
                    result.Reason = "You have an assessment in progress. Please complete or abandon it first.";
                    return result;
                }

                if (jobSeeker.LastAssessmentDate.HasValue)
                {
                    var cooldownEnds = jobSeeker.LastAssessmentDate.Value.AddDays(AssessmentSettings.CooldownDays);
                    if (DateTime.UtcNow < cooldownEnds)
                    {
                        result.IsInCooldownPeriod = true;
                        result.CooldownEndsAt = cooldownEnds;
                        result.DaysUntilEligible = (int)Math.Ceiling((cooldownEnds - DateTime.UtcNow).TotalDays);
                        result.Reason = $"Please wait {result.DaysUntilEligible} days before taking another assessment";
                        return result;
                    }
                }

                result.PreviousAttempts = await _context.AssessmentAttempts
                    .CountAsync(a => a.JobSeekerId == jobSeeker.Id && a.AlgorithmVersion == AlgorithmVersion);

                var activeAttempt = await _context.AssessmentAttempts
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.IsActive
                                           && a.Status == AssessmentStatus.Completed);

                if (activeAttempt != null)
                {
                    result.CurrentScore = activeAttempt.OverallScore;
                    result.ScoreExpiresAt = activeAttempt.ScoreExpiresAt;
                }

                result.IsEligible = true;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking assessment eligibility for user {UserId}", userId);
                return new EligibilityResponseDto { Reason = "An error occurred while checking eligibility" };
            }
        }

        #endregion

        #region Start Assessment

        public async Task<StartAssessmentResponseDto?> StartAssessmentAsync(int userId, StartAssessmentRequestDto? request = null)
        {
            try
            {
                var eligibility = await CheckEligibilityAsync(userId);
                if (!eligibility.IsEligible)
                {
                    _logger.LogWarning("User {UserId} not eligible to start assessment: {Reason}", userId, eligibility.Reason);
                    return null;
                }

                var jobSeeker = await _context.JobSeekers
                    .Include(js => js.JobTitle)
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker?.JobTitle == null)
                {
                    return null;
                }

                var claimedSkills = await GetClaimedSkillsAsync(jobSeeker.Id);
                if (request?.SkillIds != null && request.SkillIds.Count > 0)
                {
                    var requested = request.SkillIds.Distinct().ToHashSet();
                    claimedSkills = claimedSkills.Where(s => requested.Contains(s.SkillId)).ToList();
                }

                if (claimedSkills.Count == 0)
                {
                    _logger.LogWarning("User {UserId} attempted to start assessment with no valid claimed skills", userId);
                    return null;
                }

                var claimedSkillIds = claimedSkills.Select(s => s.SkillId).ToList();
                var seniorityLevel = CalculateSeniorityLevel(jobSeeker.YearsOfExperience);
                var roleFamily = jobSeeker.JobTitle.RoleFamily;

                var questionIds = await SelectQuestionsForAssessmentAsync(roleFamily, seniorityLevel, claimedSkillIds);
                if (questionIds.Count == 0)
                {
                    _logger.LogWarning("No questions could be selected for user {UserId}", userId);
                    return null;
                }

                if (questionIds.Count != AssessmentSettings.TotalQuestionsPerAssessment)
                {
                    _logger.LogWarning(
                        "Insufficient questions for user {UserId}. RoleFamily {RoleFamily}, Seniority {Seniority}, Selected {Count} (Expected {Expected})",
                        userId, roleFamily, seniorityLevel, questionIds.Count, AssessmentSettings.TotalQuestionsPerAssessment);
                    return null;
                }

                var previousAttempts = await _context.AssessmentAttempts
                    .CountAsync(a => a.JobSeekerId == jobSeeker.Id && a.AlgorithmVersion == AlgorithmVersion);

                var now = DateTime.UtcNow;
                var attempt = new AssessmentAttempt
                {
                    JobSeekerId = jobSeeker.Id,
                    JobTitleId = jobSeeker.JobTitleId!.Value,
                    Status = AssessmentStatus.InProgress,
                    StartedAt = now,
                    TimeLimitMinutes = AssessmentSettings.DefaultTimeLimitMinutes,
                    TotalQuestions = questionIds.Count,
                    QuestionsAnswered = 0,
                    ExpiresAt = now.AddMinutes(AssessmentSettings.DefaultTimeLimitMinutes),
                    IsActive = false,
                    RetakeNumber = previousAttempts + 1,
                    QuestionIdsJson = JsonSerializer.Serialize(questionIds),
                    ClaimedSkillIdsJson = JsonSerializer.Serialize(claimedSkillIds),
                    AlgorithmVersion = AlgorithmVersion
                };

                _context.AssessmentAttempts.Add(attempt);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx) when (IsInProgressConstraintViolation(dbEx))
                {
                    _logger.LogWarning(
                        dbEx,
                        "Concurrent start prevented for user {UserId}: another in-progress assessment already exists",
                        userId);
                    return null;
                }

                var selectedQuestions = await _context.AssessmentQuestions
                    .Where(q => questionIds.Contains(q.Id))
                    .ToListAsync();

                var technicalCount = selectedQuestions.Count(q => q.Category == QuestionCategory.Technical);

                var allSkillIds = selectedQuestions
                    .Select(q => q.SkillId)
                    .Concat(claimedSkillIds)
                    .Distinct()
                    .ToList();

                var skillNameLookup = await _context.Skills
                    .Where(s => allSkillIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.Name);

                var skillAllocations = selectedQuestions
                    .GroupBy(q => q.SkillId)
                    .Select(group => new SkillAllocationDto
                    {
                        SkillId = group.Key,
                        SkillName = skillNameLookup.GetValueOrDefault(group.Key, $"Skill #{group.Key}"),
                        TechnicalQuestions = group.Count(q => q.Category == QuestionCategory.Technical),
                        SoftSkillQuestions = group.Count(q => q.Category == QuestionCategory.SoftSkill)
                    })
                    .OrderByDescending(a => a.TotalQuestions)
                    .ThenBy(a => a.SkillName)
                    .ToList();

                _logger.LogInformation(
                    "Assessment started for user {UserId}, attempt {AttemptId}, questions {QuestionCount}, claimed skills {SkillCount}",
                    userId, attempt.Id, questionIds.Count, claimedSkillIds.Count);

                return new StartAssessmentResponseDto
                {
                    AttemptId = attempt.Id,
                    TotalQuestions = attempt.TotalQuestions,
                    TechnicalQuestions = technicalCount,
                    SoftSkillQuestions = attempt.TotalQuestions - technicalCount,
                    TimeLimitMinutes = attempt.TimeLimitMinutes,
                    StartedAt = attempt.StartedAt,
                    ExpiresAt = attempt.ExpiresAt,
                    JobTitle = jobSeeker.JobTitle.TitleEn,
                    RoleFamily = roleFamily.ToString(),
                    SeniorityLevel = seniorityLevel.ToString(),
                    RetakeNumber = attempt.RetakeNumber,
                    ClaimedSkillsCount = claimedSkillIds.Count,
                    SkillAllocations = skillAllocations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting assessment for user {UserId}", userId);
                return null;
            }
        }

        #endregion

        #region Current Status
        
        public async Task<AssessmentStatusResponseDto?> ResumeAssessmentAsync(int userId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return null;

                var attempt = await _context.AssessmentAttempts
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress
                                           && a.AlgorithmVersion == AlgorithmVersion);
                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                    return null;
                }

                attempt.ResumeCount++;
                await _context.SaveChangesAsync();

                if (attempt.ResumeCount > 3)
                {
                    _logger.LogWarning("User {UserId} exceeded maximum resume attempts ({Count}) for attempt {AttemptId}. Auto-submitting.", userId, attempt.ResumeCount, attempt.Id);
                    await CompleteAssessmentAsync(userId);
                    throw new InvalidOperationException("Maximum resume limit exceeded. Your assessment has been automatically submitted.");
                }

                return await GetCurrentStatusAsync(userId);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording resume for user {UserId}", userId);
                return null;
            }
        }

        public async Task<AssessmentStatusResponseDto?> GetCurrentStatusAsync(int userId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return null;

                var attempt = await _context.AssessmentAttempts
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress
                                           && a.AlgorithmVersion == AlgorithmVersion);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                var isExpired = now > attempt.ExpiresAt;
                if (isExpired)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                }

                var timeRemaining = isExpired ? 0 : (int)(attempt.ExpiresAt - now).TotalSeconds;

                return new AssessmentStatusResponseDto
                {
                    AttemptId = attempt.Id,
                    Status = attempt.Status.ToString(),
                    TotalQuestions = attempt.TotalQuestions,
                    QuestionsAnswered = attempt.QuestionsAnswered,
                    QuestionsRemaining = attempt.TotalQuestions - attempt.QuestionsAnswered,
                    StartedAt = attempt.StartedAt,
                    ExpiresAt = attempt.ExpiresAt,
                    TimeRemainingSeconds = timeRemaining,
                    ProgressPercentage = attempt.TotalQuestions > 0
                        ? Math.Round((decimal)attempt.QuestionsAnswered / attempt.TotalQuestions * 100, 1)
                        : 0,
                    IsExpired = isExpired
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment status for user {UserId}", userId);
                return null;
            }
        }

        #endregion

        #region Question Overview

        public async Task<List<AssessmentQuestionStatusDto>?> GetQuestionStatusesAsync(int userId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return null;

                var attempt = await _context.AssessmentAttempts
                    .Include(a => a.Answers)
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress
                                           && a.AlgorithmVersion == AlgorithmVersion);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                    return null;
                }

                var questionIds = ParseQuestionIdsJson(attempt.QuestionIdsJson);
                var answeredQuestionIds = attempt.Answers.Select(a => a.QuestionId).ToHashSet();

                var statuses = new List<AssessmentQuestionStatusDto>(questionIds.Count);
                for (var i = 0; i < questionIds.Count; i++)
                {
                    statuses.Add(new AssessmentQuestionStatusDto
                    {
                        QuestionNumber = i + 1,
                        IsAnswered = answeredQuestionIds.Contains(questionIds[i])
                    });
                }

                return statuses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question statuses for user {UserId}", userId);
                return null;
            }
        }

        public async Task<QuestionResponseDto?> GetQuestionByNumberAsync(int userId, int questionNumber)
        {
            try
            {
                if (questionNumber <= 0) return null;

                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return null;

                var attempt = await _context.AssessmentAttempts
                    .Include(a => a.Answers)
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress
                                           && a.AlgorithmVersion == AlgorithmVersion);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                    return null;
                }

                var questionIds = ParseQuestionIdsJson(attempt.QuestionIdsJson);
                if (questionNumber > questionIds.Count) return null;

                var questionId = questionIds[questionNumber - 1];
                var question = await _context.AssessmentQuestions
                    .FirstOrDefaultAsync(q => q.Id == questionId);

                if (question == null) return null;

                var options = JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>();
                var timeRemaining = (int)(attempt.ExpiresAt - now).TotalSeconds;
                var existingAnswer = attempt.Answers.FirstOrDefault(a => a.QuestionId == questionId);

                return new QuestionResponseDto
                {
                    QuestionId = question.Id,
                    QuestionNumber = questionNumber,
                    TotalQuestions = attempt.TotalQuestions,
                    QuestionText = question.QuestionText,
                    Category = question.Category.ToString(),
                    Difficulty = question.Difficulty.ToString(),
                    Options = options,
                    SelectedAnswerIndex = existingAnswer?.SelectedAnswerIndex,
                    TimeAllowedSeconds = question.TimePerQuestion ?? AssessmentSettings.DefaultTimePerQuestionSeconds,
                    TimeRemainingInAssessmentSeconds = Math.Max(0, timeRemaining)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question {QuestionNumber} for user {UserId}", questionNumber, userId);
                return null;
            }
        }

        #endregion

        #region Question Flow

        public async Task<QuestionResponseDto?> GetNextQuestionAsync(int userId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return null;

                var attempt = await _context.AssessmentAttempts
                    .Include(a => a.Answers)
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress
                                           && a.AlgorithmVersion == AlgorithmVersion);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                    return null;
                }

                var questionIds = ParseQuestionIdsJson(attempt.QuestionIdsJson);
                var answeredQuestionIds = attempt.Answers.Select(a => a.QuestionId).ToHashSet();

                int? nextQuestionId = null;
                int questionNumber = 0;
                for (var i = 0; i < questionIds.Count; i++)
                {
                    if (!answeredQuestionIds.Contains(questionIds[i]))
                    {
                        nextQuestionId = questionIds[i];
                        questionNumber = i + 1;
                        break;
                    }
                }

                if (nextQuestionId == null) return null;

                var question = await _context.AssessmentQuestions
                    .FirstOrDefaultAsync(q => q.Id == nextQuestionId);

                if (question == null) return null;

                var options = JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>();
                var timeRemaining = (int)(attempt.ExpiresAt - now).TotalSeconds;

                return new QuestionResponseDto
                {
                    QuestionId = question.Id,
                    QuestionNumber = questionNumber,
                    TotalQuestions = attempt.TotalQuestions,
                    QuestionText = question.QuestionText,
                    Category = question.Category.ToString(),
                    Difficulty = question.Difficulty.ToString(),
                    Options = options,
                    SelectedAnswerIndex = null,
                    TimeAllowedSeconds = question.TimePerQuestion ?? AssessmentSettings.DefaultTimePerQuestionSeconds,
                    TimeRemainingInAssessmentSeconds = Math.Max(0, timeRemaining)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next question for user {UserId}", userId);
                return null;
            }
        }

        public async Task<SubmitAnswerResponseDto?> SubmitAnswerAsync(int userId, SubmitAnswerRequestDto dto)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return null;

                var attempt = await _context.AssessmentAttempts
                    .Include(a => a.Answers)
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress
                                           && a.AlgorithmVersion == AlgorithmVersion);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                    return null;
                }

                var questionIds = ParseQuestionIdsJson(attempt.QuestionIdsJson);
                if (!questionIds.Contains(dto.QuestionId))
                {
                    _logger.LogWarning("Question {QuestionId} is not part of attempt {AttemptId}", dto.QuestionId, attempt.Id);
                    return null;
                }

                var question = await _context.AssessmentQuestions.FindAsync(dto.QuestionId);
                if (question == null) return null;

                var existingAnswer = attempt.Answers.FirstOrDefault(a => a.QuestionId == dto.QuestionId);
                if (existingAnswer != null)
                {
                    // Overwrite — answered count is unchanged.
                    existingAnswer.SelectedAnswerIndex = dto.SelectedAnswerIndex;
                    existingAnswer.IsCorrect = dto.SelectedAnswerIndex == question.CorrectAnswerIndex;
                    existingAnswer.TimeSpentSeconds = dto.TimeSpentSeconds;
                    existingAnswer.AnsweredAt = now;
                }
                else
                {
                    var answer = new AssessmentAnswer
                    {
                        AssessmentAttemptId = attempt.Id,
                        QuestionId = dto.QuestionId,
                        SelectedAnswerIndex = dto.SelectedAnswerIndex,
                        IsCorrect = dto.SelectedAnswerIndex == question.CorrectAnswerIndex,
                        TimeSpentSeconds = dto.TimeSpentSeconds,
                        AnsweredAt = now
                    };

                    _context.AssessmentAnswers.Add(answer);
                    attempt.Answers.Add(answer);
                    attempt.QuestionsAnswered++;
                }

                await _context.SaveChangesAsync();

                var timeRemaining = (int)(attempt.ExpiresAt - now).TotalSeconds;
                var questionsRemaining = attempt.TotalQuestions - attempt.QuestionsAnswered;

                return new SubmitAnswerResponseDto
                {
                    Success = true,
                    QuestionsAnswered = attempt.QuestionsAnswered,
                    QuestionsRemaining = questionsRemaining,
                    IsAssessmentComplete = questionsRemaining == 0,
                    TimeRemainingSeconds = Math.Max(0, timeRemaining),
                    ProgressPercentage = Math.Round((decimal)attempt.QuestionsAnswered / attempt.TotalQuestions * 100, 1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting answer for user {UserId}", userId);
                return null;
            }
        }

        #endregion

        #region Completion

        public async Task<AssessmentResultResponseDto?> CompleteAssessmentAsync(int userId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return null;

                var attempt = await _context.AssessmentAttempts
                    .Include(a => a.Answers)
                    .Include(a => a.JobTitle)
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress
                                           && a.AlgorithmVersion == AlgorithmVersion);

                if (attempt == null) return null;

                return await FinalizeAttemptAsync(jobSeeker, attempt, DateTime.UtcNow, includeQuestionResults: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing assessment for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> AbandonAssessmentAsync(int userId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return false;

                var attempt = await _context.AssessmentAttempts
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress
                                           && a.AlgorithmVersion == AlgorithmVersion);

                if (attempt == null) return false;

                var now = DateTime.UtcNow;
                attempt.Status = AssessmentStatus.Abandoned;
                attempt.CompletedAt = now;
                jobSeeker.LastAssessmentDate = now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Assessment abandoned for user {UserId}, attempt {AttemptId}", userId, attempt.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error abandoning assessment for user {UserId}", userId);
                return false;
            }
        }

        #endregion

        #region History & Results

        public async Task<AssessmentHistoryResponseDto> GetHistoryAsync(int userId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null)
                {
                    return new AssessmentHistoryResponseDto();
                }

                var attempts = await _context.AssessmentAttempts
                    .Include(a => a.JobTitle)
                    .Where(a => a.JobSeekerId == jobSeeker.Id && a.AlgorithmVersion == AlgorithmVersion)
                    .OrderByDescending(a => a.StartedAt)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var items = attempts.Select(a => new AssessmentHistoryItemDto
                {
                    AttemptId = a.Id,
                    Status = a.Status.ToString(),
                    OverallScore = a.OverallScore,
                    JobTitle = a.JobTitle?.TitleEn ?? "Unknown",
                    StartedAt = a.StartedAt,
                    CompletedAt = a.CompletedAt,
                    RetakeNumber = a.RetakeNumber,
                    IsActive = a.IsActive,
                    IsScoreExpired = a.ScoreExpiresAt.HasValue && a.ScoreExpiresAt.Value < now,
                    PerformanceLevel = a.OverallScore.HasValue ? GetPerformanceLevel(a.OverallScore.Value) : null
                }).ToList();

                var completedAttempts = attempts.Where(a => a.Status == AssessmentStatus.Completed).ToList();
                var activeAttempt = attempts.FirstOrDefault(a => a.IsActive && a.ScoreExpiresAt > now);

                return new AssessmentHistoryResponseDto
                {
                    Attempts = items,
                    TotalAttempts = attempts.Count,
                    BestScore = completedAttempts.Any() ? completedAttempts.Max(a => a.OverallScore) : null,
                    CurrentActiveScore = activeAttempt?.OverallScore
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment history for user {UserId}", userId);
                return new AssessmentHistoryResponseDto();
            }
        }

        public async Task<AssessmentResultResponseDto?> GetResultAsync(int userId, int attemptId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return null;

                var attempt = await _context.AssessmentAttempts
                    .Include(a => a.Answers)
                    .Include(a => a.JobTitle)
                    .FirstOrDefaultAsync(a => a.Id == attemptId
                                           && a.JobSeekerId == jobSeeker.Id
                                           && a.AlgorithmVersion == AlgorithmVersion);

                if (attempt == null) return null;
                if (attempt.Status == AssessmentStatus.InProgress) return null;

                var questionIds = ParseQuestionIdsJson(attempt.QuestionIdsJson);
                var questionMap = await _context.AssessmentQuestions
                    .Where(q => questionIds.Contains(q.Id))
                    .ToDictionaryAsync(q => q.Id);

                var orderedQuestions = questionIds
                    .Select(id => questionMap.GetValueOrDefault(id))
                    .OfType<AssessmentQuestion>()
                    .ToList();

                var claimedSkillIds = ParseIdsJson(attempt.ClaimedSkillIdsJson);
                var usedSkillIds = orderedQuestions.Select(q => q.SkillId).Distinct().ToList();
                var allSkillIds = usedSkillIds.Concat(claimedSkillIds).Distinct().ToList();

                var skillNames = await _context.Skills
                    .Where(s => allSkillIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.Name);

                var (overall, technical, softSkill, stats, skillScores, questionResults) =
                    BuildSkillScores(attempt.Answers.ToList(), orderedQuestions, skillNames, claimedSkillIds, includeQuestionResults: true);

                var timeTaken = attempt.CompletedAt.HasValue
                    ? (int)(attempt.CompletedAt.Value - attempt.StartedAt).TotalMinutes
                    : (int)(DateTime.UtcNow - attempt.StartedAt).TotalMinutes;

                return new AssessmentResultResponseDto
                {
                    AttemptId = attempt.Id,
                    Status = attempt.Status.ToString(),
                    OverallScore = attempt.OverallScore ?? overall,
                    TechnicalScore = attempt.TechnicalScore ?? technical,
                    SoftSkillsScore = attempt.SoftSkillsScore ?? softSkill,
                    TotalQuestions = attempt.TotalQuestions,
                    CorrectAnswers = stats.TotalCorrect,
                    TechnicalCorrect = stats.TechnicalCorrect,
                    TechnicalTotal = stats.TechnicalTotal,
                    SoftSkillCorrect = stats.SoftSkillCorrect,
                    SoftSkillTotal = stats.SoftSkillTotal,
                    StartedAt = attempt.StartedAt,
                    CompletedAt = attempt.CompletedAt,
                    TimeTakenMinutes = timeTaken,
                    ScoreExpiresAt = attempt.ScoreExpiresAt,
                    JobTitle = attempt.JobTitle?.TitleEn ?? "Unknown",
                    PerformanceLevel = GetPerformanceLevel(attempt.OverallScore ?? overall),
                    IsPassing = (attempt.OverallScore ?? overall) >= AssessmentSettings.MinimumPassingScore,
                    SkillScores = skillScores,
                    QuestionResults = questionResults
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment result for user {UserId}, attempt {AttemptId}", userId, attemptId);
                return null;
            }
        }

        #endregion

        #region Private Helpers

        private async Task<JobSeekerModel?> GetJobSeekerByUserIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.AccountType != AccountType.JobSeeker)
            {
                return null;
            }

            return await _context.JobSeekers.FirstOrDefaultAsync(js => js.UserId == userId);
        }

        private async Task<bool> AutoSubmitIfExpiredAsync(JobSeekerModel jobSeeker, AssessmentAttempt attempt, DateTime now)
        {
            if (attempt.Status != AssessmentStatus.InProgress || now <= attempt.ExpiresAt)
            {
                return false;
            }

            await FinalizeAttemptAsync(jobSeeker, attempt, now, includeQuestionResults: false);
            return true;
        }

        private async Task<AssessmentResultResponseDto> FinalizeAttemptAsync(
            JobSeekerModel jobSeeker,
            AssessmentAttempt attempt,
            DateTime now,
            bool includeQuestionResults)
        {
            await _context.Entry(attempt).Collection(a => a.Answers).LoadAsync();
            await _context.Entry(attempt).Reference(a => a.JobTitle).LoadAsync();

            var questionIds = ParseQuestionIdsJson(attempt.QuestionIdsJson);
            var questionMap = await _context.AssessmentQuestions
                .Where(q => questionIds.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id);

            var orderedQuestions = questionIds
                .Select(id => questionMap.GetValueOrDefault(id))
                .OfType<AssessmentQuestion>()
                .ToList();

            var claimedSkillIds = ParseIdsJson(attempt.ClaimedSkillIdsJson);
            var usedSkillIds = orderedQuestions.Select(q => q.SkillId).Distinct().ToList();
            var allSkillIds = usedSkillIds.Concat(claimedSkillIds).Distinct().ToList();

            var skillNames = await _context.Skills
                .Where(s => allSkillIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name);

            var (overall, technical, softSkill, stats, skillScores, questionResults) =
                BuildSkillScores(attempt.Answers.ToList(), orderedQuestions, skillNames, claimedSkillIds, includeQuestionResults);

            attempt.OverallScore = overall;
            attempt.TechnicalScore = technical;
            attempt.SoftSkillsScore = softSkill;
            attempt.Status = AssessmentStatus.Completed;
            attempt.CompletedAt = now;
            attempt.ScoreExpiresAt = now.AddMonths(AssessmentSettings.ScoreValidityMonths);

            var previousActiveAttempts = await _context.AssessmentAttempts
                .Where(a => a.JobSeekerId == jobSeeker.Id && a.IsActive && a.Id != attempt.Id)
                .ToListAsync();

            foreach (var prev in previousActiveAttempts)
            {
                prev.IsActive = false;
            }

            attempt.IsActive = true;
            jobSeeker.CurrentAssessmentScore = overall;
            jobSeeker.AssessmentJobTitleId = attempt.JobTitleId;
            jobSeeker.LastAssessmentDate = now;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Assessment completed for user {UserId}, attempt {AttemptId}, score {OverallScore}",
                jobSeeker.UserId, attempt.Id, overall);

            var timeTaken = (int)(now - attempt.StartedAt).TotalMinutes;

            return new AssessmentResultResponseDto
            {
                AttemptId = attempt.Id,
                Status = attempt.Status.ToString(),
                OverallScore = overall,
                TechnicalScore = technical,
                SoftSkillsScore = softSkill,
                TotalQuestions = attempt.TotalQuestions,
                CorrectAnswers = stats.TotalCorrect,
                TechnicalCorrect = stats.TechnicalCorrect,
                TechnicalTotal = stats.TechnicalTotal,
                SoftSkillCorrect = stats.SoftSkillCorrect,
                SoftSkillTotal = stats.SoftSkillTotal,
                StartedAt = attempt.StartedAt,
                CompletedAt = attempt.CompletedAt,
                TimeTakenMinutes = timeTaken,
                ScoreExpiresAt = attempt.ScoreExpiresAt,
                JobTitle = attempt.JobTitle?.TitleEn ?? "Unknown",
                PerformanceLevel = GetPerformanceLevel(overall),
                IsPassing = overall >= AssessmentSettings.MinimumPassingScore,
                SkillScores = skillScores,
                QuestionResults = questionResults
            };
        }

        private async Task<List<(int SkillId, string SkillName)>> GetClaimedSkillsAsync(int jobSeekerId)
        {
            return await _context.JobSeekerSkills
                .Where(js => js.JobSeekerId == jobSeekerId)
                .Include(js => js.Skill)
                .OrderBy(js => js.Skill.Name)
                .Select(js => new ValueTuple<int, string>(js.SkillId, js.Skill.Name))
                .ToListAsync();
        }

        private static ExperienceSeniorityLevel CalculateSeniorityLevel(int? yearsOfExperience)
        {
            return yearsOfExperience switch
            {
                null or <= 2 => ExperienceSeniorityLevel.Junior,
                >= 3 and <= 5 => ExperienceSeniorityLevel.Mid,
                _ => ExperienceSeniorityLevel.Senior
            };
        }

        /// <summary>
        /// Returns the target difficulty distribution for a given seniority level.
        /// Juniors get mostly easy questions, seniors get mostly hard questions.
        /// </summary>
        private static Dictionary<QuestionDifficulty, double> GetDifficultyDistribution(ExperienceSeniorityLevel level)
        {
            return level switch
            {
                ExperienceSeniorityLevel.Junior => new Dictionary<QuestionDifficulty, double>
                {
                    { QuestionDifficulty.Easy, 0.50 },
                    { QuestionDifficulty.Medium, 0.35 },
                    { QuestionDifficulty.Hard, 0.15 }
                },
                ExperienceSeniorityLevel.Mid => new Dictionary<QuestionDifficulty, double>
                {
                    { QuestionDifficulty.Easy, 0.20 },
                    { QuestionDifficulty.Medium, 0.50 },
                    { QuestionDifficulty.Hard, 0.30 }
                },
                ExperienceSeniorityLevel.Senior => new Dictionary<QuestionDifficulty, double>
                {
                    { QuestionDifficulty.Easy, 0.10 },
                    { QuestionDifficulty.Medium, 0.30 },
                    { QuestionDifficulty.Hard, 0.60 }
                },
                _ => new Dictionary<QuestionDifficulty, double>
                {
                    { QuestionDifficulty.Easy, 0.33 },
                    { QuestionDifficulty.Medium, 0.34 },
                    { QuestionDifficulty.Hard, 0.33 }
                }
            };
        }

        /// <summary>
        /// Computes how many questions of each difficulty level are needed for a given
        /// target count, based on the seniority-appropriate distribution.
        /// </summary>
        private static Dictionary<QuestionDifficulty, int> ComputeDifficultyTargets(int targetCount, ExperienceSeniorityLevel seniority)
        {
            var distribution = GetDifficultyDistribution(seniority);
            var targets = new Dictionary<QuestionDifficulty, int>();
            var allocated = 0;

            var difficulties = new[] { QuestionDifficulty.Easy, QuestionDifficulty.Medium, QuestionDifficulty.Hard };
            foreach (var diff in difficulties)
            {
                var count = (int)Math.Round(targetCount * distribution.GetValueOrDefault(diff, 0));
                targets[diff] = count;
                allocated += count;
            }

            // Adjust rounding: add/remove from Medium bucket
            if (allocated != targetCount)
            {
                targets[QuestionDifficulty.Medium] += targetCount - allocated;
            }

            return targets;
        }

        private async Task<List<int>> SelectQuestionsForAssessmentAsync(
            JobTitleRoleFamily roleFamily,
            ExperienceSeniorityLevel seniorityLevel,
            List<int> claimedSkillIds)
        {
            var questions = await _context.AssessmentQuestions
                .AsNoTracking()
                .Where(q => q.IsActive)
                .ToListAsync();

            var technicalPool = questions
                .Where(q => q.Category == QuestionCategory.Technical && IsRoleCompatible(q.RoleFamily, roleFamily))
                .Select(q => new QuestionPoolItem(q))
                .ToList();

            var softPool = questions
                .Where(q => q.Category == QuestionCategory.SoftSkill)
                .Select(q => new QuestionPoolItem(q))
                .ToList();

            var claimedSkillSet = claimedSkillIds.Distinct().ToHashSet();

            var claimedTechnicalSkills = technicalPool
                .Where(q => claimedSkillSet.Contains(q.SkillId))
                .Select(q => q.SkillId)
                .Distinct()
                .ToList();

            var claimedSoftSkills = softPool
                .Where(q => claimedSkillSet.Contains(q.SkillId))
                .Select(q => q.SkillId)
                .Distinct()
                .ToList();

            var technicalDifficultyTargets = ComputeDifficultyTargets(AssessmentSettings.TechnicalQuestionsCount, seniorityLevel);
            var softDifficultyTargets = ComputeDifficultyTargets(AssessmentSettings.SoftSkillQuestionsCount, seniorityLevel);

            var selectedTechnical = SelectQuestionsBySkillCoverage(
                technicalPool, claimedTechnicalSkills, AssessmentSettings.TechnicalQuestionsCount, seniorityLevel, technicalDifficultyTargets);

            if (selectedTechnical.Count < AssessmentSettings.TechnicalQuestionsCount)
            {
                FillFromPool(selectedTechnical, technicalPool, AssessmentSettings.TechnicalQuestionsCount, seniorityLevel, _ => true);
            }

            var selectedSoft = SelectQuestionsBySkillCoverage(
                softPool,
                claimedSoftSkills.Count > 0 ? claimedSoftSkills : new List<int>(),
                AssessmentSettings.SoftSkillQuestionsCount,
                seniorityLevel,
                softDifficultyTargets);

            if (selectedSoft.Count < AssessmentSettings.SoftSkillQuestionsCount)
            {
                FillFromPool(selectedSoft, softPool, AssessmentSettings.SoftSkillQuestionsCount, seniorityLevel, _ => true);
            }

            return selectedTechnical
                .Concat(selectedSoft)
                .Distinct()
                .OrderBy(_ => Guid.NewGuid())
                .ToList();
        }

        /// <summary>
        /// Selects questions from the pool, distributing evenly across claimed skills
        /// and respecting the target difficulty distribution per seniority.
        /// </summary>
        private static List<int> SelectQuestionsBySkillCoverage(
            List<QuestionPoolItem> pool,
            List<int> skillIds,
            int targetCount,
            ExperienceSeniorityLevel preferredSeniority,
            Dictionary<QuestionDifficulty, int> difficultyTargets)
        {
            if (targetCount <= 0 || pool.Count == 0)
            {
                return new List<int>();
            }

            var selected = new List<int>();
            var uniquePool = pool
                .GroupBy(p => p.QuestionId)
                .Select(g => g.First())
                .ToList();

            if (skillIds.Count == 0)
            {
                FillFromPoolByDifficulty(selected, uniquePool, difficultyTargets, targetCount);
                return selected;
            }

            var distinctSkills = skillIds.Distinct().ToList();
            var basePerSkill = targetCount / distinctSkills.Count;
            var remainder = targetCount % distinctSkills.Count;

            for (var i = 0; i < distinctSkills.Count; i++)
            {
                var skillId = distinctSkills[i];
                var requiredForSkill = basePerSkill + (i < remainder ? 1 : 0);
                var skillSubTargets = ComputeDifficultyTargets(requiredForSkill, preferredSeniority);
                var countBefore = selected.Count;

                // Select from this skill following difficulty distribution
                foreach (var (diff, count) in skillSubTargets)
                {
                    if (count <= 0) continue;
                    var batch = uniquePool
                        .Where(q => q.SkillId == skillId && q.Difficulty == diff && !selected.Contains(q.QuestionId))
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(count)
                        .Select(q => q.QuestionId)
                        .ToList();
                    selected.AddRange(batch);
                }

                // If difficulty-targeted selection didn't fill the skill quota, fill from any difficulty
                var selectedForThisSkill = selected.Count - countBefore;
                if (selectedForThisSkill < requiredForSkill)
                {
                    var gap = requiredForSkill - selectedForThisSkill;
                    var fallback = uniquePool
                        .Where(q => q.SkillId == skillId && !selected.Contains(q.QuestionId))
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(gap)
                        .Select(q => q.QuestionId)
                        .ToList();
                    selected.AddRange(fallback);
                }
            }

            if (selected.Count < targetCount)
            {
                FillFromPool(selected, uniquePool, targetCount, preferredSeniority, q => distinctSkills.Contains(q.SkillId));
            }

            return selected;
        }

        /// <summary>
        /// Fills selected IDs from the pool, preferring the target difficulty distribution first,
        /// then falling back to preferred seniority, then any remaining questions.
        /// </summary>
        private static void FillFromPoolByDifficulty(
            List<int> selectedIds,
            List<QuestionPoolItem> pool,
            Dictionary<QuestionDifficulty, int> difficultyTargets,
            int totalTarget)
        {
            foreach (var (diff, count) in difficultyTargets)
            {
                if (selectedIds.Count >= totalTarget || count <= 0) break;
                var batch = pool
                    .Where(q => q.Difficulty == diff && !selectedIds.Contains(q.QuestionId))
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(count)
                    .Select(q => q.QuestionId)
                    .ToList();
                selectedIds.AddRange(batch);
            }

            // Fill any remaining gap from the full pool
            if (selectedIds.Count < totalTarget)
            {
                var remaining = pool
                    .Where(q => !selectedIds.Contains(q.QuestionId))
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(totalTarget - selectedIds.Count)
                    .Select(q => q.QuestionId)
                    .ToList();
                selectedIds.AddRange(remaining);
            }
        }

        private static void FillFromPool(
            List<int> selectedIds,
            List<QuestionPoolItem> pool,
            int targetCount,
            ExperienceSeniorityLevel preferredSeniority,
            Func<QuestionPoolItem, bool> predicate)
        {
            if (selectedIds.Count >= targetCount) return;

            var preferred = pool
                .Where(q => predicate(q) && q.SeniorityLevel == preferredSeniority && !selectedIds.Contains(q.QuestionId))
                .OrderBy(_ => Guid.NewGuid())
                .Take(targetCount - selectedIds.Count)
                .Select(q => q.QuestionId)
                .ToList();

            selectedIds.AddRange(preferred);

            if (selectedIds.Count >= targetCount) return;

            var anySeniority = pool
                .Where(q => predicate(q) && !selectedIds.Contains(q.QuestionId))
                .OrderBy(_ => Guid.NewGuid())
                .Take(targetCount - selectedIds.Count)
                .Select(q => q.QuestionId)
                .ToList();

            selectedIds.AddRange(anySeniority);
        }

        private (decimal Overall, decimal Technical, decimal SoftSkill, ScoreStats Stats, List<SkillScoreDto> SkillScores, List<QuestionResultDto>? QuestionResults)
            BuildSkillScores(
                List<AssessmentAnswer> answers,
                List<AssessmentQuestion> questions,
                Dictionary<int, string> skillNames,
                List<int> claimedSkillIds,
                bool includeQuestionResults)
        {
            var claimedSkillSet = claimedSkillIds.ToHashSet();
            var buckets = new Dictionary<int, SkillBucket>();

            var answersByQuestionId = answers
                .GroupBy(a => a.QuestionId)
                .Select(g => g.First())
                .ToDictionary(a => a.QuestionId);

            var technicalCorrect = 0;
            var technicalTotal = 0;
            var softSkillCorrect = 0;
            var softSkillTotal = 0;

            List<QuestionResultDto>? questionResults = includeQuestionResults ? new List<QuestionResultDto>() : null;

            foreach (var question in questions)
            {
                var skillId = question.SkillId;
                if (!buckets.TryGetValue(skillId, out var bucket))
                {
                    bucket = new SkillBucket { SkillId = skillId, Category = question.Category };
                    buckets[skillId] = bucket;
                }

                bucket.TotalQuestions++;

                var hasAnswer = answersByQuestionId.TryGetValue(question.Id, out var answer);
                var isCorrect = hasAnswer && answer!.IsCorrect;
                if (isCorrect) bucket.CorrectAnswers++;

                if (question.Category == QuestionCategory.Technical)
                {
                    technicalTotal++;
                    if (isCorrect) technicalCorrect++;
                }
                else
                {
                    softSkillTotal++;
                    if (isCorrect) softSkillCorrect++;
                }

                if (questionResults != null)
                {
                    var options = JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>();
                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.QuestionText,
                        Category = question.Category.ToString(),
                        Difficulty = question.Difficulty.ToString(),
                        Options = options,
                        SelectedAnswerIndex = hasAnswer ? answer!.SelectedAnswerIndex : null,
                        CorrectAnswerIndex = question.CorrectAnswerIndex,
                        IsCorrect = isCorrect,
                        Explanation = question.Explanation,
                        TimeSpentSeconds = hasAnswer ? answer!.TimeSpentSeconds : 0,
                        SkillId = skillId,
                        SkillName = skillNames.GetValueOrDefault(skillId, $"Skill #{skillId}")
                    });
                }
            }

            // Ensure every claimed skill has a bucket (even if no questions were sampled for it).
            foreach (var claimedSkillId in claimedSkillSet)
            {
                if (!buckets.ContainsKey(claimedSkillId))
                {
                    buckets[claimedSkillId] = new SkillBucket
                    {
                        SkillId = claimedSkillId,
                        Category = QuestionCategory.Technical
                    };
                }
            }

            var totalCorrect = technicalCorrect + softSkillCorrect;
            var totalQuestions = technicalTotal + softSkillTotal;

            var technicalScore = technicalTotal > 0 ? (decimal)technicalCorrect / technicalTotal * 100 : 0;
            var softSkillScore = softSkillTotal > 0 ? (decimal)softSkillCorrect / softSkillTotal * 100 : 0;
            var overallScore = totalQuestions > 0 ? (decimal)totalCorrect / totalQuestions * 100 : 0;

            var skillScores = buckets.Values
                .Select(b => new SkillScoreDto
                {
                    SkillId = b.SkillId,
                    SkillName = skillNames.GetValueOrDefault(b.SkillId, $"Skill #{b.SkillId}"),
                    Category = b.Category.ToString(),
                    CorrectAnswers = b.CorrectAnswers,
                    TotalQuestions = b.TotalQuestions,
                    Score = b.TotalQuestions > 0
                        ? Math.Round((decimal)b.CorrectAnswers / b.TotalQuestions * 100, 2)
                        : 0,
                    IsClaimedSkill = claimedSkillSet.Contains(b.SkillId)
                })
                .OrderByDescending(s => s.IsClaimedSkill)
                .ThenByDescending(s => s.TotalQuestions)
                .ThenBy(s => s.SkillName)
                .ToList();

            var statsResult = new ScoreStats(totalCorrect, technicalCorrect, technicalTotal, softSkillCorrect, softSkillTotal);

            return (
                Math.Round(overallScore, 2),
                Math.Round(technicalScore, 2),
                Math.Round(softSkillScore, 2),
                statsResult,
                skillScores,
                questionResults);
        }

        private static bool IsRoleCompatible(JobTitleRoleFamily questionRoleFamily, JobTitleRoleFamily userRoleFamily)
        {
            return questionRoleFamily == userRoleFamily
                || questionRoleFamily == JobTitleRoleFamily.FullStack
                || userRoleFamily == JobTitleRoleFamily.FullStack;
        }

        private static List<int> ParseIdsJson(string? json)
        {
            return JsonSerializer.Deserialize<List<int>>(json ?? "[]")?.Distinct().ToList() ?? new List<int>();
        }

        private static List<int> ParseQuestionIdsJson(string? json)
        {
            return JsonSerializer.Deserialize<List<int>>(json ?? "[]") ?? new List<int>();
        }

        private static string GetPerformanceLevel(decimal score)
        {
            return score switch
            {
                >= 90 => "Excellent",
                >= 75 => "Good",
                >= 50 => "Average",
                _ => "Needs Improvement"
            };
        }

        private static bool IsInProgressConstraintViolation(DbUpdateException exception)
        {
            return exception.Message.Contains("UX_AssessmentAttempt_JobSeeker_InProgress", StringComparison.OrdinalIgnoreCase)
                || (exception.InnerException?.Message.Contains("UX_AssessmentAttempt_JobSeeker_InProgress", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private sealed class QuestionPoolItem
        {
            public QuestionPoolItem(AssessmentQuestion question)
            {
                QuestionId = question.Id;
                SkillId = question.SkillId;
                SeniorityLevel = question.SeniorityLevel;
                Difficulty = question.Difficulty;
            }

            public int QuestionId { get; }
            public int SkillId { get; }
            public ExperienceSeniorityLevel SeniorityLevel { get; }
            public QuestionDifficulty Difficulty { get; }
        }

        private sealed class SkillBucket
        {
            public int SkillId { get; set; }
            public QuestionCategory Category { get; set; }
            public int CorrectAnswers { get; set; }
            public int TotalQuestions { get; set; }
        }

        private record ScoreStats(int TotalCorrect, int TechnicalCorrect, int TechnicalTotal, int SoftSkillCorrect, int SoftSkillTotal);

        #endregion
    }
}
