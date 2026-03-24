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
    /// Service for managing job seeker skill assessments
    /// </summary>
    public class AssessmentService : IAssessmentService
    {
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

                // Get user and validate role
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.AccountType != AccountType.JobSeeker)
                {
                    result.Reason = "Only job seekers can take assessments";
                    return result;
                }

                // Get job seeker profile
                var jobSeeker = await _context.JobSeekers
                    .Include(js => js.JobTitle)
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    result.Reason = "Job seeker profile not found";
                    return result;
                }

                // Check profile completion
                result.HasCompletedProfile = user.ProfileCompletionStep >= 4;
                if (!result.HasCompletedProfile)
                {
                    result.Reason = "Please complete your profile before taking an assessment";
                    return result;
                }

                // Check job title
                result.HasJobTitle = jobSeeker.JobTitleId.HasValue;
                if (!result.HasJobTitle)
                {
                    result.Reason = "Please set your job title before taking an assessment";
                    return result;
                }

                // Check for in-progress assessment
                var inProgressAttempt = await _context.AssessmentAttempts
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress);

                result.HasInProgressAssessment = inProgressAttempt != null;
                if (result.HasInProgressAssessment)
                {
                    result.Reason = "You have an assessment in progress. Please complete or abandon it first.";
                    return result;
                }

                // Check cooldown period
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

                // Get previous attempts count
                result.PreviousAttempts = await _context.AssessmentAttempts
                    .CountAsync(a => a.JobSeekerId == jobSeeker.Id);

                // Get current active score
                var activeAttempt = await _context.AssessmentAttempts
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.IsActive
                                           && a.Status == AssessmentStatus.Completed);

                if (activeAttempt != null)
                {
                    result.CurrentScore = activeAttempt.OverallScore;
                    result.ScoreExpiresAt = activeAttempt.ScoreExpiresAt;
                }

                // All checks passed
                result.IsEligible = true;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking eligibility for user {UserId}", userId);
                return new EligibilityResponseDto { Reason = "An error occurred while checking eligibility" };
            }
        }

        #endregion

        #region Start Assessment

        public async Task<StartAssessmentResponseDto?> StartAssessmentAsync(int userId)
        {
            try
            {
                // Check eligibility first
                var eligibility = await CheckEligibilityAsync(userId);
                if (!eligibility.IsEligible)
                {
                    _logger.LogWarning("User {UserId} not eligible to start assessment: {Reason}", userId, eligibility.Reason);
                    return null;
                }

                // Get job seeker with job title
                var jobSeeker = await _context.JobSeekers
                    .Include(js => js.JobTitle)
                    .FirstOrDefaultAsync(js => js.User.Id == userId);

                if (jobSeeker?.JobTitle == null)
                {
                    return null;
                }

                // Calculate seniority level
                var seniorityLevel = CalculateSeniorityLevel(jobSeeker.YearsOfExperience);
                var roleFamily = jobSeeker.JobTitle.RoleFamily;

                // Select questions for the assessment
                var questionIds = await SelectQuestionsForAssessmentAsync(roleFamily, seniorityLevel);
                if (questionIds.Count < AssessmentSettings.TotalQuestionsPerAssessment)
                {
                    _logger.LogWarning("Insufficient questions available for RoleFamily {RoleFamily}, Seniority {Seniority}. Found: {Count}",
                        roleFamily, seniorityLevel, questionIds.Count);
                    // Continue with available questions
                }

                // Determine retake number
                var previousAttempts = await _context.AssessmentAttempts
                    .CountAsync(a => a.JobSeekerId == jobSeeker.Id);

                // Create the assessment attempt
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
                    QuestionIdsJson = JsonSerializer.Serialize(questionIds)
                };

                _context.AssessmentAttempts.Add(attempt);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Assessment started for user {UserId}, attempt {AttemptId} with {QuestionCount} questions",
                    userId, attempt.Id, questionIds.Count);

                // Count technical vs soft skill questions
                var technicalCount = await _context.AssessmentQuestions
                    .CountAsync(q => questionIds.Contains(q.Id) && q.Category == QuestionCategory.Technical);

                return new StartAssessmentResponseDto
                {
                    AttemptId = attempt.Id,
                    TotalQuestions = attempt.TotalQuestions,
                    TechnicalQuestions = technicalCount,
                    SoftSkillQuestions = attempt.TotalQuestions - technicalCount,
                    TimeLimitMinutes = attempt.TimeLimitMinutes,
                    StartedAt = attempt.StartedAt,
                    ExpiresAt = attempt.ExpiresAt,
                    JobTitle = jobSeeker.JobTitle.Title,
                    RoleFamily = roleFamily.ToString(),
                    SeniorityLevel = seniorityLevel.ToString(),
                    RetakeNumber = attempt.RetakeNumber
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

        public async Task<AssessmentStatusResponseDto?> GetCurrentStatusAsync(int userId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerByUserIdAsync(userId);
                if (jobSeeker == null) return null;

                var attempt = await _context.AssessmentAttempts
                    .FirstOrDefaultAsync(a => a.JobSeekerId == jobSeeker.Id
                                           && a.Status == AssessmentStatus.InProgress);

                if (attempt == null) return null;

                // Check if expired
                var now = DateTime.UtcNow;
                var isExpired = now > attempt.ExpiresAt;
                var timeRemaining = isExpired ? 0 : (int)(attempt.ExpiresAt - now).TotalSeconds;

                if (isExpired && attempt.Status == AssessmentStatus.InProgress)
                {
                    attempt.Status = AssessmentStatus.Expired;
                    await _context.SaveChangesAsync();
                }

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
                _logger.LogError(ex, "Error getting current status for user {UserId}", userId);
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
                                           && a.Status == AssessmentStatus.InProgress);

                if (attempt == null) return null;

                // Check if expired
                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    attempt.Status = AssessmentStatus.Expired;
                    await _context.SaveChangesAsync();
                    return null;
                }

                // Get question IDs for this attempt
                var questionIds = JsonSerializer.Deserialize<List<int>>(attempt.QuestionIdsJson ?? "[]") ?? new List<int>();

                // Find already answered question IDs
                var answeredQuestionIds = attempt.Answers.Select(a => a.QuestionId).ToHashSet();

                // Find next unanswered question (in order)
                int? nextQuestionId = null;
                int questionNumber = 0;
                for (int i = 0; i < questionIds.Count; i++)
                {
                    if (!answeredQuestionIds.Contains(questionIds[i]))
                    {
                        nextQuestionId = questionIds[i];
                        questionNumber = i + 1;
                        break;
                    }
                }

                if (nextQuestionId == null)
                {
                    // All questions answered
                    return null;
                }

                // Get the question
                var question = await _context.AssessmentQuestions
                    .FirstOrDefaultAsync(q => q.Id == nextQuestionId);

                if (question == null) return null;

                // Parse options
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
                                           && a.Status == AssessmentStatus.InProgress);

                if (attempt == null) return null;

                // Check if expired
                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    attempt.Status = AssessmentStatus.Expired;
                    await _context.SaveChangesAsync();
                    return null;
                }

                // Verify question is part of this assessment
                var questionIds = JsonSerializer.Deserialize<List<int>>(attempt.QuestionIdsJson ?? "[]") ?? new List<int>();
                if (!questionIds.Contains(dto.QuestionId))
                {
                    _logger.LogWarning("Question {QuestionId} not part of attempt {AttemptId}", dto.QuestionId, attempt.Id);
                    return null;
                }

                // Check if already answered
                if (attempt.Answers.Any(a => a.QuestionId == dto.QuestionId))
                {
                    _logger.LogWarning("Question {QuestionId} already answered in attempt {AttemptId}", dto.QuestionId, attempt.Id);
                    return null;
                }

                // Get the question to verify answer
                var question = await _context.AssessmentQuestions.FindAsync(dto.QuestionId);
                if (question == null) return null;

                // Create the answer
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
                attempt.QuestionsAnswered++;
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
                                           && a.Status == AssessmentStatus.InProgress);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;

                // Check if expired
                if (now > attempt.ExpiresAt)
                {
                    attempt.Status = AssessmentStatus.Expired;
                    await _context.SaveChangesAsync();
                    // Still calculate and return results for expired assessments
                }

                // Get question details for scoring
                var questionIds = attempt.Answers.Select(a => a.QuestionId).ToList();
                var questions = await _context.AssessmentQuestions
                    .Where(q => questionIds.Contains(q.Id))
                    .ToDictionaryAsync(q => q.Id);

                // Calculate scores
                var (overall, technical, softSkill, stats) = CalculateScores(attempt.Answers.ToList(), questions);

                // Update attempt
                attempt.OverallScore = overall;
                attempt.TechnicalScore = technical;
                attempt.SoftSkillsScore = softSkill;
                attempt.Status = now > attempt.ExpiresAt ? AssessmentStatus.Expired : AssessmentStatus.Completed;
                attempt.CompletedAt = now;
                attempt.ScoreExpiresAt = now.AddMonths(AssessmentSettings.ScoreValidityMonths);

                // Deactivate previous active attempts
                var previousActive = await _context.AssessmentAttempts
                    .Where(a => a.JobSeekerId == jobSeeker.Id && a.IsActive && a.Id != attempt.Id)
                    .ToListAsync();
                foreach (var prev in previousActive)
                {
                    prev.IsActive = false;
                }

                // Mark this attempt as active
                attempt.IsActive = true;

                // Update JobSeeker denormalized fields
                jobSeeker.CurrentAssessmentScore = overall;
                jobSeeker.LastAssessmentDate = now;
                jobSeeker.AssessmentJobTitleId = attempt.JobTitleId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Assessment completed for user {UserId}, attempt {AttemptId}, score {Score}",
                    userId, attempt.Id, overall);

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
                    JobTitle = attempt.JobTitle?.Title ?? "Unknown",
                    PerformanceLevel = GetPerformanceLevel(overall),
                    IsPassing = overall >= AssessmentSettings.MinimumPassingScore
                };
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
                                           && a.Status == AssessmentStatus.InProgress);

                if (attempt == null) return false;

                attempt.Status = AssessmentStatus.Abandoned;
                attempt.CompletedAt = DateTime.UtcNow;

                // Update cooldown even for abandoned assessments
                jobSeeker.LastAssessmentDate = DateTime.UtcNow;

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
                    .Where(a => a.JobSeekerId == jobSeeker.Id)
                    .OrderByDescending(a => a.StartedAt)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var items = attempts.Select(a => new AssessmentHistoryItemDto
                {
                    AttemptId = a.Id,
                    Status = a.Status.ToString(),
                    OverallScore = a.OverallScore,
                    JobTitle = a.JobTitle?.Title ?? "Unknown",
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
                _logger.LogError(ex, "Error getting history for user {UserId}", userId);
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
                    .FirstOrDefaultAsync(a => a.Id == attemptId && a.JobSeekerId == jobSeeker.Id);

                if (attempt == null) return null;

                // Only show results for completed/expired/abandoned attempts
                if (attempt.Status == AssessmentStatus.InProgress)
                {
                    return null;
                }

                // Get question details
                var questionIds = attempt.Answers.Select(a => a.QuestionId).ToList();
                var questions = await _context.AssessmentQuestions
                    .Where(q => questionIds.Contains(q.Id))
                    .ToDictionaryAsync(q => q.Id);

                // Build detailed question results
                var questionResults = attempt.Answers.Select(answer =>
                {
                    var question = questions.GetValueOrDefault(answer.QuestionId);
                    var options = question != null
                        ? JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>()
                        : new List<string>();

                    return new QuestionResultDto
                    {
                        QuestionId = answer.QuestionId,
                        QuestionText = question?.QuestionText ?? "Unknown",
                        Category = question?.Category.ToString() ?? "Unknown",
                        Difficulty = question?.Difficulty.ToString() ?? "Unknown",
                        Options = options,
                        SelectedAnswerIndex = answer.SelectedAnswerIndex,
                        CorrectAnswerIndex = question?.CorrectAnswerIndex ?? 0,
                        IsCorrect = answer.IsCorrect,
                        Explanation = question?.Explanation,
                        TimeSpentSeconds = answer.TimeSpentSeconds
                    };
                }).ToList();

                var (_, _, _, stats) = CalculateScores(attempt.Answers.ToList(), questions);
                var timeTaken = attempt.CompletedAt.HasValue
                    ? (int)(attempt.CompletedAt.Value - attempt.StartedAt).TotalMinutes
                    : (int)(DateTime.UtcNow - attempt.StartedAt).TotalMinutes;

                return new AssessmentResultResponseDto
                {
                    AttemptId = attempt.Id,
                    Status = attempt.Status.ToString(),
                    OverallScore = attempt.OverallScore ?? 0,
                    TechnicalScore = attempt.TechnicalScore ?? 0,
                    SoftSkillsScore = attempt.SoftSkillsScore ?? 0,
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
                    JobTitle = attempt.JobTitle?.Title ?? "Unknown",
                    PerformanceLevel = GetPerformanceLevel(attempt.OverallScore ?? 0),
                    IsPassing = (attempt.OverallScore ?? 0) >= AssessmentSettings.MinimumPassingScore,
                    QuestionResults = questionResults
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting result for user {UserId}, attempt {AttemptId}", userId, attemptId);
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

        private static ExperienceSeniorityLevel CalculateSeniorityLevel(int? yearsOfExperience)
        {
            return yearsOfExperience switch
            {
                null or <= 2 => ExperienceSeniorityLevel.Junior,
                >= 3 and <= 5 => ExperienceSeniorityLevel.Mid,
                _ => ExperienceSeniorityLevel.Senior
            };
        }

        private async Task<List<int>> SelectQuestionsForAssessmentAsync(
            JobTitleRoleFamily roleFamily,
            ExperienceSeniorityLevel seniorityLevel)
        {
            var selectedIds = new List<int>();

            // Define difficulty distribution based on seniority
            var (easyTech, mediumTech, hardTech) = seniorityLevel switch
            {
                ExperienceSeniorityLevel.Junior => (10, 8, 3),
                ExperienceSeniorityLevel.Mid => (5, 11, 5),
                _ => (3, 8, 10) // Senior
            };

            var (easySoft, mediumSoft, hardSoft) = seniorityLevel switch
            {
                ExperienceSeniorityLevel.Junior => (4, 4, 1),
                ExperienceSeniorityLevel.Mid => (2, 5, 2),
                _ => (1, 4, 4) // Senior
            };

            // Select technical questions
            foreach (var (difficulty, count) in new[]
            {
                (QuestionDifficulty.Easy, easyTech),
                (QuestionDifficulty.Medium, mediumTech),
                (QuestionDifficulty.Hard, hardTech)
            })
            {
                var questions = await _context.AssessmentQuestions
                    .Where(q => q.Category == QuestionCategory.Technical
                             && q.RoleFamily == roleFamily
                             && q.SeniorityLevel == seniorityLevel
                             && q.Difficulty == difficulty
                             && q.IsActive)
                    .Select(q => q.Id)
                    .ToListAsync();

                // Shuffle and take required count
                var shuffled = questions.OrderBy(_ => Guid.NewGuid()).Take(count);
                selectedIds.AddRange(shuffled);
            }

            // If not enough technical questions, try adjacent seniority levels
            if (selectedIds.Count < AssessmentSettings.TechnicalQuestionsCount)
            {
                var fallbackQuestions = await _context.AssessmentQuestions
                    .Where(q => q.Category == QuestionCategory.Technical
                             && q.RoleFamily == roleFamily
                             && q.IsActive
                             && !selectedIds.Contains(q.Id))
                    .Select(q => q.Id)
                    .ToListAsync();

                var needed = AssessmentSettings.TechnicalQuestionsCount - selectedIds.Count;
                selectedIds.AddRange(fallbackQuestions.OrderBy(_ => Guid.NewGuid()).Take(needed));
            }

            // Select soft skill questions (not role-specific)
            foreach (var (difficulty, count) in new[]
            {
                (QuestionDifficulty.Easy, easySoft),
                (QuestionDifficulty.Medium, mediumSoft),
                (QuestionDifficulty.Hard, hardSoft)
            })
            {
                var questions = await _context.AssessmentQuestions
                    .Where(q => q.Category == QuestionCategory.SoftSkill
                             && q.SeniorityLevel == seniorityLevel
                             && q.Difficulty == difficulty
                             && q.IsActive)
                    .Select(q => q.Id)
                    .ToListAsync();

                var shuffled = questions.OrderBy(_ => Guid.NewGuid()).Take(count);
                selectedIds.AddRange(shuffled);
            }

            // Shuffle the final list
            return selectedIds.OrderBy(_ => Guid.NewGuid()).ToList();
        }

        private record ScoreStats(int TotalCorrect, int TechnicalCorrect, int TechnicalTotal, int SoftSkillCorrect, int SoftSkillTotal);

        private static (decimal Overall, decimal Technical, decimal SoftSkill, ScoreStats Stats) CalculateScores(
            List<AssessmentAnswer> answers,
            Dictionary<int, AssessmentQuestion> questions)
        {
            var technicalAnswers = answers.Where(a =>
                questions.TryGetValue(a.QuestionId, out var q) && q.Category == QuestionCategory.Technical).ToList();
            var softSkillAnswers = answers.Where(a =>
                questions.TryGetValue(a.QuestionId, out var q) && q.Category == QuestionCategory.SoftSkill).ToList();

            var technicalCorrect = technicalAnswers.Count(a => a.IsCorrect);
            var technicalTotal = technicalAnswers.Count;
            var softSkillCorrect = softSkillAnswers.Count(a => a.IsCorrect);
            var softSkillTotal = softSkillAnswers.Count;

            var technicalScore = technicalTotal > 0
                ? (decimal)technicalCorrect / technicalTotal * 100
                : 0;
            var softSkillScore = softSkillTotal > 0
                ? (decimal)softSkillCorrect / softSkillTotal * 100
                : 0;

            var overallScore = (technicalScore * AssessmentSettings.TechnicalWeight)
                             + (softSkillScore * AssessmentSettings.SoftSkillWeight);

            var stats = new ScoreStats(
                technicalCorrect + softSkillCorrect,
                technicalCorrect,
                technicalTotal,
                softSkillCorrect,
                softSkillTotal
            );

            return (
                Math.Round(overallScore, 2),
                Math.Round(technicalScore, 2),
                Math.Round(softSkillScore, 2),
                stats
            );
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

        #endregion
    }
}
