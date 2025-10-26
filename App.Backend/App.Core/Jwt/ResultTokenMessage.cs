namespace App.Core.Jwt
{
    public class ResultTokenMessage
    {
        public string Message { get; set; } = default!;
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
