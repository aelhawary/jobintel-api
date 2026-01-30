namespace RecruitmentPlatformAPI.DTOs.Common
{
/// <summary>
    /// Standard API response wrapper for successful operations with data
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// The response data payload
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Optional message providing additional context
        /// </summary>
        /// <example>Data retrieved successfully</example>
        public string? Message { get; set; }

        public ApiResponse()
        {
            Success = true;
        }

        public ApiResponse(T data)
        {
            Success = true;
            Data = data;
        }

        public ApiResponse(T data, string message)
        {
            Success = true;
            Data = data;
            Message = message;
        }
    }

    /// <summary>
    /// Standard API response wrapper for error responses
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Always false for error responses
        /// </summary>
        /// <example>false</example>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Error message describing what went wrong
        /// </summary>
        /// <example>User not found</example>
        public string Message { get; set; } = string.Empty;

        public ApiErrorResponse(string message)
        {
            Message = message;
        }
    }
}
