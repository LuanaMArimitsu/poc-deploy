namespace WebsupplyConnect.API.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? Error { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Sucesso") =>
            new()
            {
                Success = true,
                Message = message,
                Data = data
            };

        public static ApiResponse<T> ErrorResponse(string message, string? error = null) =>
            new()
            {
                Success = false,
                Message = message,
                Error = error
            };
    }
}
