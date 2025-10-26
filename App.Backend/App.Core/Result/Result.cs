namespace App.Core.Result
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }

        public static Result<T> Success(T data, int statusCode = 200) => new Result<T> { IsSuccess = true, Data = data, StatusCode = statusCode };
        public static Result<T> Failure(string error, int statusCode = 400) => new Result<T> { IsSuccess = false, ErrorMessage = error, StatusCode = statusCode };
    }
}