namespace RecruitmentPlatformAPI.DTOs.Recruiter
{
    public enum JobServiceErrorCode
    {
        None = 0,
        Forbidden = 1,
        NotFound = 2,
        Validation = 3,
        ProfileMissing = 4,
        ServerError = 5
    }

    public class JobServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public JobServiceErrorCode ErrorCode { get; set; } = JobServiceErrorCode.None;
        public T? Data { get; set; }

        public static JobServiceResult<T> Ok(T data, string? message = null)
        {
            return new JobServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message ?? string.Empty,
                ErrorCode = JobServiceErrorCode.None
            };
        }

        public static JobServiceResult<T> Fail(string message, JobServiceErrorCode errorCode)
        {
            return new JobServiceResult<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}
