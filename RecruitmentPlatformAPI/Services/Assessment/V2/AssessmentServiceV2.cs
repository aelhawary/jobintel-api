using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Assessment.V2;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Assessment.V2;
using RecruitmentPlatformAPI.Services.Assessment.LlmGeneration;
using JobSeekerModel = RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker;

namespace RecruitmentPlatformAPI.Services.Assessment.V2
{
    public class AssessmentServiceV2 : IAssessmentServiceV2
    {
        private readonly AppDbContext _context;
        private readonly QuestionGenerationOrchestrator _orchestrator;
        private readonly ILogger<AssessmentServiceV2> _logger;

        public AssessmentServiceV2(
            AppDbContext context,
            QuestionGenerationOrchestrator orchestrator,
            ILogger<AssessmentServiceV2> logger)
        {
            _context = context;
            _orchestrator = orchestrator;
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

                var inProgressAttempt = await _context.AssessmentAttemptsV2
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == (int)AssessmentStatus.InProgress);

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

                result.PreviousAttempts = await _context.AssessmentAttemptsV2
                    .CountAsync(a => a.JobSeekerId == jobSeeker.Id);

                var activeAttempt = await _context.AssessmentAttemptsV2
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.IsActive
                                           && a.Status == (int)AssessmentStatus.Completed);

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

                var allClaimedSkills = await GetClaimedSkillsAsync(jobSeeker.Id);
                var technicalSkills = allClaimedSkills;
                if (request?.SkillIds != null && request.SkillIds.Count > 0)
                {
                    var requested = request.SkillIds.Distinct().ToHashSet();
                    technicalSkills = allClaimedSkills.Where(s => requested.Contains(s.SkillId)).ToList();
                }

                if (technicalSkills.Count == 0)
                {
                    _logger.LogWarning("User {UserId} attempted to start assessment with no valid claimed skills", userId);
                    return null;
                }

                var claimedSkillIds = technicalSkills.Select(s => s.SkillId).ToList();
                var seniorityLevel = CalculateSeniorityLevel(jobSeeker.YearsOfExperience);
                var roleFamily = jobSeeker.JobTitle.RoleFamily;

                List<LlmGeneratedQuestionDto> generatedQuestions;
                try
                {
                    generatedQuestions = await _orchestrator.GenerateForCandidateAsync(
                        jobSeeker.JobTitle.TitleEn,
                        roleFamily.ToString(),
                        seniorityLevel,
                        jobSeeker.YearsOfExperience ?? 0,
                        technicalSkills.Select(s => (s.SkillId, s.SkillName)).ToList());
                }
                catch (LlmGenerationException ex)
                {
                    _logger.LogError(ex, "LLM Generation failed for user {UserId}", userId);
                    return null;
                }

                if (generatedQuestions.Count == 0)
                {
                    _logger.LogWarning("LLM returned 0 questions for user {UserId}", userId);
                    return null;
                }

                // Get a lookup of all available skills to map LLM names to IDs
                var allSkillsLookup = await _context.Skills
                    .ToDictionaryAsync(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase);

                var skillNameToId = technicalSkills.ToDictionary(s => s.SkillName, s => s.SkillId, StringComparer.OrdinalIgnoreCase);

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var persistedQuestions = new List<AssessmentQuestionV2>();
                    foreach (var q in generatedQuestions)
                    {
                        int skillId;
                        if (allSkillsLookup.TryGetValue(q.SkillName, out var matchedId))
                        {
                            skillId = matchedId;
                        }
                        else if (skillNameToId.TryGetValue(q.SkillName, out var technicalId))
                        {
                            skillId = technicalId;
                        }
                        else
                        {
                            skillId = claimedSkillIds.First();
                        }

                        var question = new AssessmentQuestionV2
                        {
                            // CRIT-8: Defensive truncation
                            QuestionText = q.QuestionText.Length > 500 ? q.QuestionText[..500] : q.QuestionText,
                            Category = q.Category,
                            RoleFamily = roleFamily.ToString(),
                            SkillId = skillId,
                            Difficulty = q.Difficulty,
                            SeniorityLevel = seniorityLevel.ToString(),
                            Options = JsonSerializer.Serialize(q.Options),
                            CorrectAnswerIndex = q.CorrectAnswerIndex,
                            TimePerQuestion = AssessmentSettings.DefaultTimePerQuestionSeconds,
                            Explanation = (q.Explanation?.Length > 1000 ? q.Explanation[..1000] : q.Explanation) ?? string.Empty,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        persistedQuestions.Add(question);
                    }

                    _context.AssessmentQuestionsV2.AddRange(persistedQuestions);
                    await _context.SaveChangesAsync();

                    var questionIds = persistedQuestions.Select(q => q.Id).OrderBy(_ => Guid.NewGuid()).ToList();
                    var previousAttempts = await _context.AssessmentAttemptsV2
                        .CountAsync(a => a.JobSeekerId == jobSeeker.Id);

                    var now = DateTime.UtcNow;
                    var attempt = new AssessmentAttemptV2
                    {
                        JobSeekerId = jobSeeker.Id,
                        JobTitleId = jobSeeker.JobTitleId!.Value,
                        Status = (int)AssessmentStatus.InProgress,
                        StartedAt = now,
                        TimeLimitMinutes = AssessmentSettings.DefaultTimeLimitMinutes,
                        TotalQuestions = questionIds.Count,
                        QuestionsAnswered = 0,
                        ExpiresAt = now.AddMinutes(AssessmentSettings.DefaultTimeLimitMinutes),
                        IsActive = true,
                        RetakeNumber = previousAttempts + 1,
                        QuestionIdsJson = JsonSerializer.Serialize(questionIds),
                        ClaimedSkillIdsJson = JsonSerializer.Serialize(claimedSkillIds)
                    };

                    _context.AssessmentAttemptsV2.Add(attempt);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var technicalCount = persistedQuestions.Count(q => q.Category == "Technical");
                    var allSkillIdsForLookup = persistedQuestions.Select(q => q.SkillId).Concat(claimedSkillIds).Distinct().ToList();
                    var skillNameLookup = await _context.Skills
                        .Where(s => allSkillIdsForLookup.Contains(s.Id))
                        .ToDictionaryAsync(s => s.Id, s => s.Name);

                    var skillAllocations = persistedQuestions
                        .GroupBy(q => q.SkillId)
                        .Select(group => new SkillAllocationDto
                        {
                            SkillId = group.Key,
                            SkillName = skillNameLookup.GetValueOrDefault(group.Key, $"Skill #{group.Key}"),
                            TechnicalQuestions = group.Count(q => q.Category == "Technical"),
                            SoftSkillQuestions = group.Count(q => q.Category != "Technical")
                        })
                        .OrderByDescending(a => a.TotalQuestions)
                        .ThenBy(a => a.SkillName)
                        .ToList();

                    return new StartAssessmentResponseDto
                    {
                        AttemptId = attempt.Id,
                        TotalQuestions = attempt.TotalQuestions,
                        TechnicalQuestions = technicalCount,
                        SoftSkillQuestions = attempt.TotalQuestions - technicalCount,
                        TimeLimitMinutes = attempt.TimeLimitMinutes,
                        StartedAt = attempt.StartedAt,
                        ExpiresAt = attempt.ExpiresAt,
                        JobTitle = jobSeeker.JobTitle?.TitleEn ?? "Unknown",
                        RoleFamily = roleFamily.ToString(),
                        SeniorityLevel = seniorityLevel.ToString(),
                        RetakeNumber = attempt.RetakeNumber,
                        ClaimedSkillsCount = claimedSkillIds.Count,
                        SkillAllocations = skillAllocations
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting V2 assessment for user {UserId}", userId);
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

                var attempt = await GetInProgressAttemptWithAnswersAsync(jobSeeker.Id);
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
                    
                    return new AssessmentStatusResponseDto
                    {
                        AttemptId = attempt.Id,
                        Status = AssessmentStatus.Completed.ToString(),
                        TotalQuestions = attempt.TotalQuestions,
                        QuestionsAnswered = attempt.QuestionsAnswered,
                        QuestionsRemaining = attempt.TotalQuestions - attempt.QuestionsAnswered,
                        StartedAt = attempt.StartedAt,
                        ExpiresAt = attempt.ExpiresAt,
                        TimeRemainingSeconds = 0,
                        ProgressPercentage = attempt.TotalQuestions > 0 ? Math.Round((decimal)attempt.QuestionsAnswered / attempt.TotalQuestions * 100, 1) : 0,
                        IsExpired = false,
                        IsAutoSubmitted = true
                    };
                }

                return await GetCurrentStatusAsync(userId);
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

                var attempt = await GetInProgressAttemptWithAnswersAsync(jobSeeker.Id);

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
                    Status = ((AssessmentStatus)attempt.Status).ToString(),
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

                var attempt = await GetInProgressAttemptWithAnswersAsync(jobSeeker.Id);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                    return null;
                }

                var questionIds = ParseIdsJson(attempt.QuestionIdsJson);
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

                var attempt = await GetInProgressAttemptWithAnswersAsync(jobSeeker.Id);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                    return null;
                }

                var questionIds = ParseIdsJson(attempt.QuestionIdsJson);
                if (questionNumber > questionIds.Count) return null;

                var questionId = questionIds[questionNumber - 1];
                var question = await _context.AssessmentQuestionsV2
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
                    Category = question.Category,
                    Difficulty = question.Difficulty,
                    Options = options,
                    SelectedAnswerIndex = existingAnswer?.SelectedAnswerIndex,
                    TimeAllowedSeconds = question.TimePerQuestion,
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

                var attempt = await GetInProgressAttemptWithAnswersAsync(jobSeeker.Id);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                    return null;
                }

                var questionIds = ParseIdsJson(attempt.QuestionIdsJson);
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

                var question = await _context.AssessmentQuestionsV2
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
                    Category = question.Category,
                    Difficulty = question.Difficulty,
                    Options = options,
                    SelectedAnswerIndex = null,
                    TimeAllowedSeconds = question.TimePerQuestion,
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

                var attempt = await GetInProgressAttemptWithAnswersAsync(jobSeeker.Id);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    await AutoSubmitIfExpiredAsync(jobSeeker, attempt, now);
                    return null;
                }

                var questionIds = ParseIdsJson(attempt.QuestionIdsJson);
                if (!questionIds.Contains(dto.QuestionId))
                {
                    _logger.LogWarning("Question {QuestionId} is not part of attempt {AttemptId}", dto.QuestionId, attempt.Id);
                    return null;
                }

                var question = await _context.AssessmentQuestionsV2.FindAsync(dto.QuestionId);
                if (question == null) return null;

                var options = JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>();
                if (dto.SelectedAnswerIndex < 0 || dto.SelectedAnswerIndex >= options.Count)
                {
                    _logger.LogWarning("Invalid SelectedAnswerIndex {Index} for question {QuestionId}", dto.SelectedAnswerIndex, dto.QuestionId);
                    return null;
                }

                var existingAnswer = attempt.Answers.FirstOrDefault(a => a.QuestionId == dto.QuestionId);
                if (existingAnswer != null)
                {
                    existingAnswer.SelectedAnswerIndex = dto.SelectedAnswerIndex;
                    existingAnswer.IsCorrect = dto.SelectedAnswerIndex == question.CorrectAnswerIndex;
                    existingAnswer.TimeSpentSeconds = dto.TimeSpentSeconds;
                    existingAnswer.AnsweredAt = now;
                }
                else
                {
                    var answer = new AssessmentAnswerV2
                    {
                        AssessmentAttemptId = attempt.Id,
                        QuestionId = dto.QuestionId,
                        SelectedAnswerIndex = dto.SelectedAnswerIndex,
                        IsCorrect = dto.SelectedAnswerIndex == question.CorrectAnswerIndex,
                        TimeSpentSeconds = dto.TimeSpentSeconds,
                        AnsweredAt = now
                    };

                    _context.AssessmentAnswersV2.Add(answer);

                    // CRIT-7: Atomic increment to prevent race conditions (SQL Server only)
                    if (_context.Database.IsSqlServer())
                    {
                        await _context.AssessmentAttemptsV2
                            .Where(a => a.Id == attempt.Id)
                            .ExecuteUpdateAsync(s => s.SetProperty(a => a.QuestionsAnswered, a => a.QuestionsAnswered + 1));
                            
                        attempt.QuestionsAnswered++; // Update local state
                        // Prevent change tracker from overwriting the atomic increment
                        _context.Entry(attempt).Property(a => a.QuestionsAnswered).IsModified = false;
                    }
                    else
                    {
                        attempt.QuestionsAnswered++; // Update local state
                    }
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

                var attempt = await GetInProgressAttemptWithAnswersAsync(jobSeeker.Id);

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

                var attempt = await GetInProgressAttemptWithAnswersAsync(jobSeeker.Id);

                if (attempt == null) return false;

                var now = DateTime.UtcNow;
                attempt.Status = (int)AssessmentStatus.Abandoned;
                attempt.CompletedAt = now;
                // CRIT-5: Don't set LastAssessmentDate on abandon to avoid unfair 60-day cooldown

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
                if (jobSeeker == null) return new AssessmentHistoryResponseDto();

                var attempts = await _context.AssessmentAttemptsV2
                    .Where(a => a.JobSeekerId == jobSeeker.Id)
                    .OrderByDescending(a => a.StartedAt)
                    .ToListAsync();

                var jobTitleIds = attempts.Where(a => a.JobTitleId > 0).Select(a => a.JobTitleId).Distinct().ToList();
                var jobTitles = await _context.JobTitles
                    .Where(jt => jobTitleIds.Contains(jt.Id))
                    .ToDictionaryAsync(jt => jt.Id, jt => jt.TitleEn);

                var now = DateTime.UtcNow;
                var items = attempts.Select(a => new AssessmentHistoryItemDto
                {
                    AttemptId = a.Id,
                    Status = ((AssessmentStatus)a.Status).ToString(),
                    OverallScore = a.OverallScore,
                    JobTitle = a.JobTitleId.HasValue ? jobTitles.GetValueOrDefault(a.JobTitleId.Value, "Unknown") : "Unknown",
                    StartedAt = a.StartedAt,
                    CompletedAt = a.CompletedAt,
                    RetakeNumber = a.RetakeNumber,
                    IsActive = a.IsActive,
                    IsScoreExpired = a.ScoreExpiresAt.HasValue && a.ScoreExpiresAt.Value < now,
                    PerformanceLevel = a.OverallScore > 0 ? GetPerformanceLevel(a.OverallScore) : null
                }).ToList();

                var completedAttempts = attempts.Where(a => a.Status == (int)AssessmentStatus.Completed).ToList();
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

                var attempt = await _context.AssessmentAttemptsV2
                    .Include(a => a.Answers)
                    .FirstOrDefaultAsync(a => a.Id == attemptId && a.JobSeekerId == jobSeeker.Id);

                // CRIT-2 Fix: Use read-only BuildResultDtoAsync instead of FinalizeAttemptAsync
                if (attempt == null || attempt.Status == (int)AssessmentStatus.InProgress) return null;

                return await BuildResultDtoAsync(jobSeeker, attempt, includeQuestionResults: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting result for attempt {AttemptId}", attemptId);
                return null;
            }
        }

        #endregion

        #region Private Helpers

        private async Task<JobSeekerModel?> GetJobSeekerByUserIdAsync(int userId)
        {
            return await _context.JobSeekers.FirstOrDefaultAsync(js => js.UserId == userId);
        }

        private async Task<AssessmentAttemptV2?> GetInProgressAttemptWithAnswersAsync(int jobSeekerId)
        {
            return await _context.AssessmentAttemptsV2
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeekerId
                                       && a.Status == (int)AssessmentStatus.InProgress);
        }

        private async Task<List<AssessmentSkillLiteDto>> GetClaimedSkillsAsync(int jobSeekerId)
        {
            return await _context.JobSeekerSkills
                .Where(js => js.JobSeekerId == jobSeekerId)
                .Select(js => new AssessmentSkillLiteDto
                {
                    SkillId = js.SkillId,
                    SkillName = js.Skill.Name
                })
                .ToListAsync();
        }

        private ExperienceSeniorityLevel CalculateSeniorityLevel(int? years)
        {
            // CRIT-6: Fix seniority boundaries (Junior is 0-2 years inclusive)
            if (!years.HasValue || years <= 2) return ExperienceSeniorityLevel.Junior;
            if (years <= 5) return ExperienceSeniorityLevel.Mid;
            return ExperienceSeniorityLevel.Senior;
        }

        private List<int> ParseIdsJson(string json)
        {
            try { return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>(); }
            catch { return new List<int>(); }
        }

        private async Task AutoSubmitIfExpiredAsync(JobSeekerModel jobSeeker, AssessmentAttemptV2 attempt, DateTime now)
        {
            attempt.Status = (int)AssessmentStatus.Expired;
            attempt.CompletedAt = attempt.ExpiresAt;
            await FinalizeAttemptAsync(jobSeeker, attempt, attempt.ExpiresAt, includeQuestionResults: false);
        }

        private async Task<AssessmentResultResponseDto> FinalizeAttemptAsync(
            JobSeekerModel jobSeeker, AssessmentAttemptV2 attempt, DateTime completionTime, bool includeQuestionResults)
        {
            var questionIds = ParseIdsJson(attempt.QuestionIdsJson);
            var questionMap = await _context.AssessmentQuestionsV2
                .Where(q => questionIds.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id);

            var orderedQuestions = questionIds
                .Select(id => questionMap.GetValueOrDefault(id))
                .OfType<AssessmentQuestionV2>()
                .ToList();

            var claimedSkillIds = ParseIdsJson(attempt.ClaimedSkillIdsJson);
            var skillIds = orderedQuestions.Select(q => q.SkillId).Concat(claimedSkillIds).Distinct().ToList();
            var skillNames = await _context.Skills
                .Where(s => skillIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name);

            var (overall, technical, softSkill, stats, skillScores, questionResults) =
                BuildSkillScores(attempt.Answers.ToList(), orderedQuestions, skillNames, claimedSkillIds, includeQuestionResults);

            attempt.OverallScore = overall;
            attempt.TechnicalScore = technical;
            attempt.SoftSkillsScore = softSkill;
            
            if (attempt.Status == (int)AssessmentStatus.InProgress || attempt.Status == (int)AssessmentStatus.Expired)
            {
                attempt.Status = (int)AssessmentStatus.Completed;
            }
            
            attempt.CompletedAt = completionTime;
            attempt.IsActive = true;
            attempt.ScoreExpiresAt = completionTime.AddMonths(AssessmentSettings.ScoreValidityMonths);

            var otherAttempts = await _context.AssessmentAttemptsV2
                .Where(a => a.JobSeekerId == jobSeeker.Id && a.Id != attempt.Id && a.IsActive)
                .ToListAsync();
            foreach (var other in otherAttempts) other.IsActive = false;

            jobSeeker.LastAssessmentDate = completionTime;
            jobSeeker.CurrentAssessmentScore = overall;

            // ── AI State Invalidation ──────────────────
            // A new assessment score fundamentally changes a candidate's ranking.
            // Invalidating existing AI recommendations to trigger a re-evaluation.
            _logger.LogInformation("Assessment V2 score updated for JobSeeker {JobSeekerId}. Invalidating existing AI recommendations.", jobSeeker.Id);
            var existingRecommendations = await _context.Recommendations
                .Where(r => r.JobSeekerId == jobSeeker.Id)
                .ToListAsync();
            if (existingRecommendations.Any())
            {
                _context.Recommendations.RemoveRange(existingRecommendations);
            }

            await _context.SaveChangesAsync();

            return await BuildResultDtoAsync(jobSeeker, attempt, completionTime, includeQuestionResults);
        }

        private async Task<AssessmentResultResponseDto> BuildResultDtoAsync(
            JobSeekerModel jobSeeker, AssessmentAttemptV2 attempt, DateTime? completionTime = null, bool includeQuestionResults = true)
        {
            var finishTime = completionTime ?? attempt.CompletedAt ?? DateTime.UtcNow;
            var questionIds = ParseIdsJson(attempt.QuestionIdsJson);
            var questionMap = await _context.AssessmentQuestionsV2
                .Where(q => questionIds.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id);

            var orderedQuestions = questionIds
                .Select(id => questionMap.GetValueOrDefault(id))
                .OfType<AssessmentQuestionV2>()
                .ToList();

            var claimedSkillIds = ParseIdsJson(attempt.ClaimedSkillIdsJson);
            var skillIds = orderedQuestions.Select(q => q.SkillId).Concat(claimedSkillIds).Distinct().ToList();
            var skillNames = await _context.Skills
                .Where(s => skillIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name);

            var (overall, technical, softSkill, stats, skillScores, questionResults) =
                BuildSkillScores(attempt.Answers.ToList(), orderedQuestions, skillNames, claimedSkillIds, includeQuestionResults);

            var jobTitleName = (await _context.JobTitles.FindAsync(attempt.JobTitleId))?.TitleEn ?? "Unknown";

            return new AssessmentResultResponseDto
            {
                AttemptId = attempt.Id,
                Status = ((AssessmentStatus)attempt.Status).ToString(),
                OverallScore = overall,
                TechnicalScore = technical,
                SoftSkillsScore = softSkill,
                TotalQuestions = attempt.TotalQuestions,
                CorrectAnswers = stats.CorrectAnswers,
                TechnicalCorrect = orderedQuestions.Count(q => q.Category == "Technical" && (attempt.Answers.FirstOrDefault(a => a.QuestionId == q.Id)?.IsCorrect ?? false)),
                TechnicalTotal = orderedQuestions.Count(q => q.Category == "Technical"),
                SoftSkillCorrect = orderedQuestions.Count(q => q.Category != "Technical" && (attempt.Answers.FirstOrDefault(a => a.QuestionId == q.Id)?.IsCorrect ?? false)),
                SoftSkillTotal = orderedQuestions.Count(q => q.Category != "Technical"),
                StartedAt = attempt.StartedAt,
                CompletedAt = finishTime,
                TimeTakenMinutes = (int)(finishTime - attempt.StartedAt).TotalMinutes,
                JobTitle = jobTitleName,
                PerformanceLevel = GetPerformanceLevel(overall),
                IsPassing = overall >= AssessmentSettings.MinimumPassingScore,
                Stats = stats,
                SkillScores = skillScores,
                QuestionResults = questionResults
            };
        }

        private (decimal overall, decimal technical, decimal softSkill, AssessmentStatsDto stats, List<SkillScoreDto> skillScores, List<QuestionResultDto>? questionResults) 
            BuildSkillScores(List<AssessmentAnswerV2> answers, List<AssessmentQuestionV2> orderedQuestions, Dictionary<int, string> skillNames, List<int> claimedSkillIds, bool includeQuestionResults)
        {
            var answerMap = answers.ToDictionary(a => a.QuestionId);
            var results = new List<QuestionResultDto>();
            
            int totalTech = 0, correctTech = 0;
            int totalSoft = 0, correctSoft = 0;

            var skillCounts = new Dictionary<int, (int Total, int Correct)>();
            var skillIdToCategory = orderedQuestions
                .GroupBy(q => q.SkillId)
                .ToDictionary(g => g.Key, g => g.First().Category);

            foreach (var q in orderedQuestions)
            {
                var isTech = q.Category == "Technical";
                var ans = answerMap.GetValueOrDefault(q.Id);
                var isCorrect = ans?.IsCorrect ?? false;

                if (isTech) { totalTech++; if (isCorrect) correctTech++; }
                else { totalSoft++; if (isCorrect) correctSoft++; }

                var current = skillCounts.GetValueOrDefault(q.SkillId, (0, 0));
                skillCounts[q.SkillId] = (current.Item1 + 1, isCorrect ? current.Item2 + 1 : current.Item2);

                if (includeQuestionResults)
                {
                    results.Add(new QuestionResultDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.QuestionText,
                        Category = q.Category,
                        Difficulty = q.Difficulty,
                        Options = JsonSerializer.Deserialize<List<string>>(q.Options) ?? new List<string>(),
                        CorrectAnswerIndex = q.CorrectAnswerIndex,
                        SelectedAnswerIndex = ans?.SelectedAnswerIndex,
                        IsCorrect = isCorrect,
                        Explanation = q.Explanation,
                        SkillId = q.SkillId,
                        SkillName = skillNames.GetValueOrDefault(q.SkillId, "Unknown"),
                        TimeSpentSeconds = ans?.TimeSpentSeconds ?? 0
                    });
                }
            }

            decimal techScore = totalTech > 0 ? (decimal)correctTech / totalTech * 100 : 0;
            decimal softScore = totalSoft > 0 ? (decimal)correctSoft / totalSoft * 100 : 0;
            
            decimal overall = orderedQuestions.Count > 0 
                ? (decimal)(correctTech + correctSoft) / orderedQuestions.Count * 100 
                : 0;

            var scores = skillCounts.Select(kvp => new SkillScoreDto
            {
                SkillId = kvp.Key,
                SkillName = skillNames.GetValueOrDefault(kvp.Key, "Unknown"),
                Category = skillIdToCategory.GetValueOrDefault(kvp.Key, "Unknown"),
                Score = kvp.Value.Item1 > 0 ? (decimal)kvp.Value.Item2 / kvp.Value.Item1 * 100 : 0,
                TotalQuestions = kvp.Value.Item1,
                CorrectAnswers = kvp.Value.Item2,
                IsClaimedSkill = claimedSkillIds.Contains(kvp.Key)
            }).ToList();

            var stats = new AssessmentStatsDto
            {
                TotalQuestions = orderedQuestions.Count,
                AnsweredQuestions = answers.Count,
                CorrectAnswers = answers.Count(a => a.IsCorrect),
                WrongAnswers = answers.Count(a => !a.IsCorrect),
                SkippedQuestions = orderedQuestions.Count - answers.Count
            };

            return (Math.Round(overall, 2), Math.Round(techScore, 2), Math.Round(softScore, 2), stats, scores, includeQuestionResults ? results : null);
        }

        private string GetPerformanceLevel(decimal score)
        {
            if (score >= 90) return "Expert";
            if (score >= 75) return "Advanced";
            if (score >= 60) return "Intermediate";
            if (score >= 40) return "Beginner";
            return "Novice";
        }

        #endregion
    }
}
