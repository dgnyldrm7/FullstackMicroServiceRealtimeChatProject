namespace App.Core.Options
{
    public class JwtSettings
    {
        public string Key { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int AccessTokenExpireMinutes { get; set; } = default!;
        public int RefreshTokenExpireDays { get; set; } = default!;
        public int RefreshTokenCleanupIntervalMinutes { get; set; } = default!;
    }
}