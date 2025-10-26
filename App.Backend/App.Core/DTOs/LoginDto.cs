namespace App.Core.DTOs
{
    public class LoginDto
    {
        public string UserNumber { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}