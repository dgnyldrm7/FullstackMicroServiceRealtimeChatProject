namespace App.Core.Jwt
{
    public class RefreshTokenDto
    {
        public string? RefreshToken { get; set; }
        public string UserNumber { get; set; } = default!;
    }
}