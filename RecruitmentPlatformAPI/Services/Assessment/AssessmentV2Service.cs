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
    /// Claimed-skills validation assessment service (v2).
    /// </summary>
    public class AssessmentV2Service : IAssessmentV2Service
    {
        private const int V2AlgorithmVersion = 2;

        private readonly AppDbContext _context;
        private readonly ILogger<AssessmentV2Service> _logger;

        public AssessmentV2Service(AppDbContext context, ILogger<AssessmentV2Service> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Eligibility

        public async Task<EligibilityV2ResponseDto> CheckEligibilityAsync(int userId)
        {
            try
            {
                var result = new EligibilityV2ResponseDto();

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

                // Shared lock across versions: a user can only have one active in-progress assessment.
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
                    .CountAsync(a => a.JobSeekerId == jobSeeker.Id && a.AlgorithmVersion == V2AlgorithmVersion);

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
                _logger.LogError(ex, "Error checking v2 eligibility for user {UserId}", userId);
                return new EligibilityV2ResponseDto { Reason = "An error occurred while checking eligibility" };
            }
        }

        #endregion

        #region Start Assessment

        public async Task<StartAssessmentV2ResponseDto?> StartAssessmentAsync(int userId, StartAssessmentV2RequestDto? request = null)
        {
            try
            {
                var eligibility = await CheckEligibilityAsync(userId);
                if (!eligibility.IsEligible)
                {
                    _logger.LogWarning("User {UserId} not eligible to start v2 assessment: {Reason}", userId, eligibility.Reason);
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
                    _logger.LogWarning("User {UserId} attempted to start v2 assessment with no valid claimed skills", userId);
                    return null;
                }

                var claimedSkillIds = claimedSkills.Select(s => s.SkillId).ToList();
                var seniorityLevel = CalculateSeniorityLevel(jobSeeker.YearsOfExperience);
                var roleFamily = jobSeeker.JobTitle.RoleFamily;

                var questionIds = await SelectQuestionsForAssessmentAsync(roleFamily, seniorityLevel, claimedSkillIds);
                if (questionIds.Count == 0)
                {
                    _logger.LogWarning("No questions could be selected for user {UserId} in v2 mode", userId);
                    return null;
                }

                if (questionIds.Count < AssessmentSettings.TotalQuestionsPerAssessment)
                {
                    _logger.LogWarning(
                        "Insufficient v2 questions for user {UserId}. RoleFamily {RoleFamily}, Seniority {Seniority}, Selected {Count}",
                        userId,
                        roleFamily,
                        seniorityLevel,
                        questionIds.Count);
                }

                var previousAttempts = await _context.AssessmentAttempts
                    .CountAsync(a => a.JobSeekerId == jobSeeker.Id && a.AlgorithmVersion == V2AlgorithmVersion);

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
                    AlgorithmVersion = V2AlgorithmVersion
                };

                _context.AssessmentAttempts.Add(attempt);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx) when (IsInProgressAttemptConstraintViolation(dbEx))
                {
                    _logger.LogWarning(
                        dbEx,
                        "Concurrent v2 start prevented for user {UserId}: another in-progress assessment already exists",
                        userId);
                    return null;
                }

                var selectedQuestions = await _context.AssessmentQuestions
                    .Where(q => questionIds.Contains(q.Id))
                    .ToListAsync();

                var technicalCount = selectedQuestions.Count(q => q.Category == QuestionCategory.Technical);

                var allSkillIds = selectedQuestions
                    .Select(GetEffectiveSkillId)
                    .Concat(claimedSkillIds)
                    .Distinct()
                    .ToList();

                var skillNameLookup = await _context.Skills
                    .Where(s => allSkillIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.Name);

                var skillAllocations = selectedQuestions
                    .GroupBy(GetEffectiveSkillId)
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
                    "V2 assessment started for user {UserId}, attempt {AttemptId}, questions {QuestionCount}, claimed skills {SkillCount}",
                    userId,
                    attempt.Id,
                    questionIds.Count,
                    claimedSkillIds.Count);

                return new StartAssessmentV2ResponseDto
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
                    RetakeNumber = attempt.RetakeNumber,
                    ClaimedSkillsCount = claimedSkillIds.Count,
                    SkillAllocations = skillAllocations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting v2 assessment for user {UserId}", userId);
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
                                           && a.Status == AssessmentStatus.InProgress
                                           && a.AlgorithmVersion == V2AlgorithmVersion);

                if (attempt == null) return null;

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
                _logger.LogError(ex, "Error getting v2 current status for user {UserId}", userId);
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
                                           && a.AlgorithmVersion == V2AlgorithmVersion);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    attempt.Status = AssessmentStatus.Expired;
                    await _context.SaveChangesAsync();
                    return null;
                }

                var questionIds = JsonSerializer.Deserialize<List<int>>(attempt.QuestionIdsJson ?? "[]") ?? new List<int>();
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

                if (nextQuestionId == null)
                {
                    return null;
                }

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
                    TimeAllowedSeconds = question.TimePerQuestion ?? AssessmentSettings.DefaultTimePerQuestionSeconds,
                    TimeRemainingInAssessmentSeconds = Math.Max(0, timeRemaining)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting v2 next question for user {UserId}", userId);
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
                                           && a.AlgorithmVersion == V2AlgorithmVersion);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                if (now > attempt.ExpiresAt)
                {
                    attempt.Status = AssessmentStatus.Expired;
                    await _context.SaveChangesAsync();
                    return null;
                }

                var questionIds = JsonSerializer.Deserialize<List<int>>(attempt.QuestionIdsJson ?? "[]") ?? new List<int>();
                if (!questionIds.Contains(dto.QuestionId))
                {
                    _logger.LogWarning("Question {QuestionId} not part of v2 attempt {AttemptId}", dto.QuestionId, attempt.Id);
                    return null;
                }

                if (attempt.Answers.Any(a => a.QuestionId == dto.QuestionId))
                {
                    _logger.LogWarning("Question {QuestionId} already answered in v2 attempt {AttemptId}", dto.QuestionId, attempt.Id);
                    return null;
                }

                var question = await _context.AssessmentQuestions.FindAsync(dto.QuestionId);
                if (question == null) return null;

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
                _logger.LogError(ex, "Error submitting v2 answer for user {UserId}", userId);
                return null;
            }
        }

        #endregion

        #region Completion

        public async Task<AssessmentResultV2ResponseDto?> CompleteAssessmentAsync(int userId)
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
                                           && a.AlgorithmVersion == V2AlgorithmVersion);

                if (attempt == null) return null;

                var now = DateTime.UtcNow;
                var isExpired = now > attempt.ExpiresAt;
                if (isExpired)
                {
                    attempt.Status = AssessmentStatus.Expired;
                    await _context.SaveChangesAsync();
                }

                var answeredQuestionIds = attempt.Answers.Select(a => a.QuestionId).ToList();
                var questions = await _context.AssessmentQuestions
                    .Where(q => answeredQuestionIds.Contains(q.Id))
                    .ToDictionaryAsync(q => q.Id);

                var claimedSkillIds = ParseIdsJson(attempt.ClaimedSkillIdsJson);

                var usedSkillIds = questions.Values.Select(GetEffectiveSkillId).Distinct().ToList();
                var allSkillIds = usedSkillIds.Concat(claimedSkillIds).Distinct().ToList();

                var skillNames = await _context.Skills
                    .Where(s => allSkillIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.Name);

                var (overall, technical, softSkill, stats, skillScores, _) =
                    BuildSkillScores(attempt.Answers.ToList(), questions, skillNames, claimedSkillIds, includeQuestionResults: false);

                attempt.OverallScore = overall;
                attempt.TechnicalScore = technical;
                attempt.SoftSkillsScore = softSkill;
                attempt.Status = isExpired ? AssessmentStatus.Expired : AssessmentStatus.Completed;
                attempt.CompletedAt = now;
                attempt.ScoreExpiresAt = isExpired
                    ? null
                    : now.AddMonths(AssessmentSettings.ScoreValidityMonths);

                if (!isExpired)
                {
                    var previousActiveAttempts = await _context.AssessmentAttempts
                        .Where(a => a.JobSeekerId == jobSeeker.Id && a.IsActive && a.Id != attempt.Id)
                        .ToListAsync();

                    foreach (var previousActive in previousActiveAttempts)
                    {
                        previousActive.IsActive = false;
                    }

                    attempt.IsActive = true;

                    jobSeeker.CurrentAssessmentScore = overall;
                    jobSeeker.AssessmentJobTitleId = attempt.JobTitleId;
                }
                else
                {
                    attempt.IsActive = false;
                }

                jobSeeker.LastAssessmentDate = now;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "V2 assessment completed for user {UserId}, attempt {AttemptId}, overall {OverallScore}",
                    userId,
                    attempt.Id,
                    overall);

                var timeTaken = (int)(now - attempt.StartedAt).TotalMinutes;

                return new AssessmentResultV2ResponseDto
                {
                    AttemptId = attempt.Id,
                    Status = attempt.Status.ToString(),
                    OverallScore = overall,
                    TechnicalSkillsTotalScore = technical,
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
                    IsPassing = overall >= AssessmentSettings.MinimumPassingScore,
                    SkillScores = skillScores
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing v2 assessment for user {UserId}", userId);
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
                                           && a.AlgorithmVersion == V2AlgorithmVersion);

                if (attempt == null) return false;

                var now = DateTime.UtcNow;
                attempt.Status = AssessmentStatus.Abandoned;
                attempt.CompletedAt = now;

                // Cooldown is shared across all assessment versions.
                jobSeeker.LastAssessmentDate = now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("V2 assessment abandoned for user {UserId}, attempt {AttemptId}", userId, attempt.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error abandoning v2 assessment for user {UserId}", userId);
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
                    .Where(a => a.JobSeekerId == jobSeeker.Id && a.AlgorithmVersion == V2AlgorithmVersion)
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
                _logger.LogError(ex, "Error getting v2 history for user {UserId}", userId);
                return new AssessmentHistoryResponseDto();
            }
        }

        public async Task<AssessmentResultV2ResponseDto?> GetResultAsync(int userId, int attemptId)
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
                                           && a.AlgorithmVersion == V2AlgorithmVersion);

                if (attempt == null) return null;

                if (attempt.Status == AssessmentStatus.InProgress)
                {
                    return null;
                }

                var questionIds = attempt.Answers.Select(a => a.QuestionId).ToList();
                var questions = await _context.AssessmentQuestions
                    .Where(q => questionIds.Contains(q.Id))
                    .ToDictionaryAsync(q => q.Id);

                var claimedSkillIds = ParseIdsJson(attempt.ClaimedSkillIdsJson);
                var usedSkillIds = questions.Values.Select(GetEffectiveSkillId).Distinct().ToList();
                var allSkillIds = usedSkillIds.Concat(claimedSkillIds).Distinct().ToList();

                var skillNames = await _context.Skills
                    .Where(s => allSkillIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.Name);

                var (overall, technical, softSkill, stats, skillScores, questionResults) =
                    BuildSkillScores(attempt.Answers.ToList(), questions, skillNames, claimedSkillIds, includeQuestionResults: true);

                var timeTaken = attempt.CompletedAt.HasValue
                    ? (int)(attempt.CompletedAt.Value - attempt.StartedAt).TotalMinutes
                    : (int)(DateTime.UtcNow - attempt.StartedAt).TotalMinutes;

                return new AssessmentResultV2ResponseDto
                {
                    AttemptId = attempt.Id,
                    Status = attempt.Status.ToString(),
                    OverallScore = attempt.OverallScore ?? overall,
                    TechnicalSkillsTotalScore = attempt.TechnicalScore ?? technical,
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
                    JobTitle = attempt.JobTitle?.Title ?? "Unknown",
                    PerformanceLevel = GetPerformanceLevel(attempt.OverallScore ?? overall),
                    IsPassing = (attempt.OverallScore ?? overall) >= AssessmentSettings.MinimumPassingScore,
                    SkillScores = skillScores,
                    QuestionResults = questionResults
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting v2 result for user {UserId}, attempt {AttemptId}", userId, attemptId);
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
                .Select(q => new QuestionPoolItem(q, GetEffectiveSkillId(q)))
                .ToList();

            var softPool = questions
                .Where(q => q.Category == QuestionCategory.SoftSkill)
                .Select(q => new QuestionPoolItem(q, GetEffectiveSkillId(q)))
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

            var selectedTechnical = SelectQuestionsBySkillCoverage(
                technicalPool,
                claimedTechnicalSkills,
                AssessmentSettings.TechnicalQuestionsCount,
                seniorityLevel);

            if (selectedTechnical.Count < AssessmentSettings.TechnicalQuestionsCount)
            {
                FillFromPool(
                    selectedTechnical,
                    technicalPool,
                    AssessmentSettings.TechnicalQuestionsCount,
                    seniorityLevel,
                    q => true);
            }

            var selectedSoft = claimedSoftSkills.Count > 0
                ? SelectQuestionsBySkillCoverage(
                    softPool,
                    claimedSoftSkills,
                    AssessmentSettings.SoftSkillQuestionsCount,
                    seniorityLevel)
                : SelectQuestionsBySkillCoverage(
                    softPool,
                    new List<int>(),
                    AssessmentSettings.SoftSkillQuestionsCount,
                    seniorityLevel);

            if (selectedSoft.Count < AssessmentSettings.SoftSkillQuestionsCount)
            {
                FillFromPool(
                    selectedSoft,
                    softPool,
                    AssessmentSettings.SoftSkillQuestionsCount,
                    seniorityLevel,
                    q => true);
            }

            var selectedIds = selectedTechnical
                .Concat(selectedSoft)
                .Distinct()
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            return selectedIds;
        }

        private static List<int> SelectQuestionsBySkillCoverage(
            List<QuestionPoolItem> pool,
            List<int> skillIds,
            int targetCount,
            ExperienceSeniorityLevel preferredSeniority)
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
                FillFromPool(selected, uniquePool, targetCount, preferredSeniority, _ => true);
                return selected;
            }

            var distinctSkills = skillIds.Distinct().ToList();
            var basePerSkill = targetCount / distinctSkills.Count;
            var remainder = targetCount % distinctSkills.Count;

            for (var i = 0; i < distinctSkills.Count; i++)
            {
                var skillId = distinctSkills[i];
                var requiredForSkill = basePerSkill + (i < remainder ? 1 : 0);

                var preferred = uniquePool
                    .Where(q => q.SkillId == skillId
                             && q.SeniorityLevel == preferredSeniority
                             && !selected.Contains(q.QuestionId))
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(requiredForSkill)
                    .Select(q => q.QuestionId)
                    .ToList();

                selected.AddRange(preferred);

                if (preferred.Count < requiredForSkill)
                {
                    var fallback = uniquePool
                        .Where(q => q.SkillId == skillId
                                 && !selected.Contains(q.QuestionId))
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(requiredForSkill - preferred.Count)
                        .Select(q => q.QuestionId)
                        .ToList();

                    selected.AddRange(fallback);
                }
            }

            if (selected.Count < targetCount)
            {
                FillFromPool(
                    selected,
                    uniquePool,
                    targetCount,
                    preferredSeniority,
                    q => distinctSkills.Contains(q.SkillId));
            }

            return selected;
        }

        private static void FillFromPool(
            List<int> selectedIds,
            List<QuestionPoolItem> pool,
            int targetCount,
            ExperienceSeniorityLevel preferredSeniority,
            Func<QuestionPoolItem, bool> predicate)
        {
            if (selectedIds.Count >= targetCount)
            {
                return;
            }

            var preferred = pool
                .Where(q => predicate(q)
                         && q.SeniorityLevel == preferredSeniority
                         && !selectedIds.Contains(q.QuestionId))
                .OrderBy(_ => Guid.NewGuid())
                .Take(targetCount - selectedIds.Count)
                .Select(q => q.QuestionId)
                .ToList();

            selectedIds.AddRange(preferred);

            if (selectedIds.Count >= targetCount)
            {
                return;
            }

            var anySeniority = pool
                .Where(q => predicate(q) && !selectedIds.Contains(q.QuestionId))
                .OrderBy(_ => Guid.NewGuid())
                .Take(targetCount - selectedIds.Count)
                .Select(q => q.QuestionId)
                .ToList();

            selectedIds.AddRange(anySeniority);
        }

        private (decimal Overall, decimal Technical, decimal SoftSkill, ScoreStats Stats, List<SkillScoreDto> SkillScores, List<QuestionResultV2Dto>? QuestionResults) BuildSkillScores(
            List<AssessmentAnswer> answers,
            Dictionary<int, AssessmentQuestion> questions,
            Dictionary<int, string> skillNames,
            List<int> claimedSkillIds,
            bool includeQuestionResults)
        {
            var claimedSkillSet = claimedSkillIds.ToHashSet();
            var buckets = new Dictionary<int, SkillBucket>();

            var technicalCorrect = 0;
            var technicalTotal = 0;
            var softSkillCorrect = 0;
            var softSkillTotal = 0;

            List<QuestionResultV2Dto>? questionResults = includeQuestionResults ? new List<QuestionResultV2Dto>() : null;

            foreach (var answer in answers)
            {
                if (!questions.TryGetValue(answer.QuestionId, out var question))
                {
                    continue;
                }

                var effectiveSkillId = GetEffectiveSkillId(question);
                if (!buckets.TryGetValue(effectiveSkillId, out var bucket))
                {
                    bucket = new SkillBucket
                    {
                        SkillId = effectiveSkillId,
                        Category = question.Category
                    };
                    buckets[effectiveSkillId] = bucket;
                }

                bucket.TotalQuestions++;
                if (answer.IsCorrect)
                {
                    bucket.CorrectAnswers++;
                }

                if (question.Category == QuestionCategory.Technical)
                {
                    technicalTotal++;
                    if (answer.IsCorrect)
                    {
                        technicalCorrect++;
                    }
                }
                else
                {
                    softSkillTotal++;
                    if (answer.IsCorrect)
                    {
                        softSkillCorrect++;
                    }
                }

                if (questionResults != null)
                {
                    var options = JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>();
                    questionResults.Add(new QuestionResultV2Dto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.QuestionText,
                        Category = question.Category.ToString(),
                        Difficulty = question.Difficulty.ToString(),
                        Options = options,
                        SelectedAnswerIndex = answer.SelectedAnswerIndex,
                        CorrectAnswerIndex = question.CorrectAnswerIndex,
                        IsCorrect = answer.IsCorrect,
                        Explanation = question.Explanation,
                        TimeSpentSeconds = answer.TimeSpentSeconds,
                        SkillId = effectiveSkillId,
                        SkillName = skillNames.GetValueOrDefault(effectiveSkillId, $"Skill #{effectiveSkillId}")
                    });
                }
            }

            foreach (var claimedSkillId in claimedSkillSet)
            {
                if (!buckets.ContainsKey(claimedSkillId))
                {
                    buckets[claimedSkillId] = new SkillBucket
                    {
                        SkillId = claimedSkillId,
                        Category = QuestionCategory.Technical,
                        TotalQuestions = 0,
                        CorrectAnswers = 0
                    };
                }
            }

            var totalCorrect = technicalCorrect + softSkillCorrect;
            var totalAnswered = technicalTotal + softSkillTotal;

            var technicalScore = technicalTotal > 0
                ? (decimal)technicalCorrect / technicalTotal * 100
                : 0;

            var softSkillScore = softSkillTotal > 0
                ? (decimal)softSkillCorrect / softSkillTotal * 100
                : 0;

            var overallScore = totalAnswered > 0
                ? (decimal)totalCorrect / totalAnswered * 100
                : 0;

            var skillScores = buckets.Values
                .Select(bucket => new SkillScoreDto
                {
                    SkillId = bucket.SkillId,
                    SkillName = skillNames.GetValueOrDefault(bucket.SkillId, $"Skill #{bucket.SkillId}"),
                    Category = bucket.Category.ToString(),
                    CorrectAnswers = bucket.CorrectAnswers,
                    TotalQuestions = bucket.TotalQuestions,
                    Score = bucket.TotalQuestions > 0
                        ? Math.Round((decimal)bucket.CorrectAnswers / bucket.TotalQuestions * 100, 2)
                        : 0,
                    IsClaimedSkill = claimedSkillSet.Contains(bucket.SkillId)
                })
                .OrderByDescending(s => s.IsClaimedSkill)
                .ThenByDescending(s => s.TotalQuestions)
                .ThenBy(s => s.SkillName)
                .ToList();

            var stats = new ScoreStats(totalCorrect, technicalCorrect, technicalTotal, softSkillCorrect, softSkillTotal);

            return (
                Math.Round(overallScore, 2),
                Math.Round(technicalScore, 2),
                Math.Round(softSkillScore, 2),
                stats,
                skillScores,
                questionResults);
        }

        private int GetEffectiveSkillId(AssessmentQuestion question)
        {
            return question.SkillId;
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

        private static bool IsInProgressAttemptConstraintViolation(DbUpdateException exception)
        {
            return exception.Message.Contains("UX_AssessmentAttempt_JobSeeker_InProgress", StringComparison.OrdinalIgnoreCase)
                || (exception.InnerException?.Message.Contains("UX_AssessmentAttempt_JobSeeker_InProgress", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private sealed class QuestionPoolItem
        {
            public QuestionPoolItem(AssessmentQuestion question, int skillId)
            {
                QuestionId = question.Id;
                SkillId = skillId;
                SeniorityLevel = question.SeniorityLevel;
            }

            public int QuestionId { get; }

            public int SkillId { get; }

            public ExperienceSeniorityLevel SeniorityLevel { get; }
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
