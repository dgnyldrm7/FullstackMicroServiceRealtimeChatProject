namespace App.Core.DTOs
{
    public class LogoutDto
    {
        public string? RefreshToken { get; set; }
        public string UserNumber { get; set; } = default!;
    }
}