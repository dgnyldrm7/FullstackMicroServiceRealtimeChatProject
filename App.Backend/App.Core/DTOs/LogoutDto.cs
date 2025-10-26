namespace App.Core.DTOs
{
    public class LogoutDto
    {
        public string RefreshToken { get; set; } = default!;
        public string UserNumber { get; set; } = default!;
    }
}