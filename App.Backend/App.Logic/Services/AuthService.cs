using App.Core.Entities;
using App.Core.Interface;
using App.Core.Interface.Redis;
using App.Core.Jwt;
using App.Core.Options;
using App.Core.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace App.Logic.Services
{
    public class AuthService
    {
        private readonly IJwtTokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly JwtSettings jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;
        private readonly IRedisService _redisService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService currentUserService;

        public AuthService(IJwtTokenService tokenService, IRefreshTokenRepository refreshTokenRepository, UserManager<AppUser> userManager, IOptions<JwtSettings> jwtOptions, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger, IRedisService redisService, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            jwtSettings = jwtOptions.Value;
            _redisService = redisService;
            this.unitOfWork = unitOfWork;
            this.currentUserService = currentUserService;
        }

        //Login
        public async Task<Result<ResultTokenMessage>> LoginAsync(string userNumber, string password)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == userNumber);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {

                _logger.LogInformation("Failed login attempt for phone number {userNumber}, from IP {ipAddress} at {Time}",
                    user?.PhoneNumber, 
                    _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    DateTime.UtcNow.AddHours(3));

                return Result<ResultTokenMessage>.Failure("Invalid username or password.", 401);
            }

            var refreshToken = await _userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshToken");
            var refreshTokenExpiry = await _userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshTokenExpiry");

            var isValidRefresh = !string.IsNullOrEmpty(refreshToken)
                && DateTime.TryParse(refreshTokenExpiry, out var expiry)
                && expiry > DateTime.UtcNow;

            if (!isValidRefresh)
            {
                refreshToken = _tokenService.GenerateRefreshToken();
                string newExpiry = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpireDays).ToString("O");

                await _userManager.SetAuthenticationTokenAsync(user, "MyApp", "RefreshToken", refreshToken);
                await _userManager.SetAuthenticationTokenAsync(user, "MyApp", "RefreshTokenExpiry", newExpiry);
            }

            var (accessToken, jti) = _tokenService.GenerateAccessToken(user.Id, user.PhoneNumber!, user.UserName!, await _userManager.GetRolesAsync(user));

            //JTI Managment with Redis
            var oldJti = await _redisService.GetAsync($"user-jti:{user.Id}");
            if (oldJti != null)
            {
                await _redisService.DeleteAsync($"jti:{oldJti}");
            }

            await _redisService.SetAsync($"user-jti:{user.Id}", jti, TimeSpan.FromMinutes(jwtSettings.AccessTokenExpireMinutes));

            await _redisService.SetAsync($"jti:{jti}", user.Id, TimeSpan.FromMinutes(jwtSettings.AccessTokenExpireMinutes));

            var accessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(3).AddMinutes(jwtSettings.AccessTokenExpireMinutes)
            };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append("AccessToken", accessToken, accessTokenCookieOptions);

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(3).AddDays(jwtSettings.RefreshTokenExpireDays)
            };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append("RefreshToken", refreshToken!, refreshTokenCookieOptions);

            var data = new ResultTokenMessage
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken!,
                Message = "Token generated successfully",
            };

            _logger.LogInformation("Login success: {@user}", user);

            return Result<ResultTokenMessage>.Success(data, 200);
        }

        //Refresh Token
        public async Task<Result<ResultTokenMessage>> RefreshToken(RefreshTokenDto model)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.UserNumber);            

            if (user == null)
            {
                // Kullanıcı bulunamadıysa 401 Unauthorized.
                return Result<ResultTokenMessage>.Failure("User number is not found.", 401);
            }

            //Redisdeki access tokeni silelim
            string? jti = await _redisService.GetAsync($"user-jti:{user.Id}");

            if (!string.IsNullOrEmpty(jti))
            {
                await _redisService.DeleteAsync($"user-jti:{user.Id}");
                await _redisService.DeleteAsync($"jti:{jti}");
            }

            RefreshToken? refreshToken = await _refreshTokenRepository.GetByTokenAsync(model.RefreshToken!, user.PhoneNumber!);

            if (refreshToken == null  || refreshToken.IsExpired)
            { 
                return Result<ResultTokenMessage>.Failure("Refresh token is faulty.", 401);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var (accessToken, newJti) = _tokenService.GenerateAccessToken(user.Id, user.PhoneNumber!, user.UserName!, roles);

            //Redise yeni access tokeni yerleştirelim
            await _redisService.SetAsync($"user-jti:{user.Id}", newJti, TimeSpan.FromMinutes(jwtSettings.AccessTokenExpireMinutes));

            await _redisService.SetAsync($"jti:{newJti}", user.Id, TimeSpan.FromMinutes(jwtSettings.AccessTokenExpireMinutes));

            // Yeni access token'ı HttpOnly cookie olarak ayarlayalım.
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "AccessToken",
                accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(3).AddMinutes(jwtSettings.AccessTokenExpireMinutes)
                });

            ResultTokenMessage data = new ResultTokenMessage
            {
                AccessToken = accessToken,
                Message = "New access token generated.",
            };

            return Result<ResultTokenMessage>.Success(data, 200);
        }

        // Web Logout (userNumber yok)
        public async Task<Result<ResultTokenMessage>> LogoutInternal(bool isWeb, string? refreshToken = null)
        {
            AppUser? user = await currentUserService.GetLoggedInUserAsync();
            if (user == null)
                return Result<ResultTokenMessage>.Failure("User not found.", 401);

            return await LogoutInternalForUser(user, isWeb, refreshToken);
        }

        // Mobil Logout (userNumber ile geliyor)
        public async Task<Result<ResultTokenMessage>> LogoutInternal(string userNumber, bool isWeb, string? refreshToken = null)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == userNumber);
            if (user == null)
                return Result<ResultTokenMessage>.Failure("User not found.", 401);

            return await LogoutInternalForUser(user, isWeb, refreshToken);
        }

        //Logout işlemi
        private async Task<Result<ResultTokenMessage>> LogoutInternalForUser(AppUser user, bool isWeb, string? refreshToken)
        {
            // Web için cookie'den token al
            if (isWeb && string.IsNullOrEmpty(refreshToken))
            {
                var cookieToken = _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
                if (!string.IsNullOrEmpty(cookieToken))
                    refreshToken = cookieToken;
            }

            if (string.IsNullOrEmpty(refreshToken))
                return Result<ResultTokenMessage>.Failure("Refresh token missing.", 400);

            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, user.PhoneNumber!);
            if (token == null)
                return Result<ResultTokenMessage>.Failure("Refresh token is invalid.", 401);

            await _refreshTokenRepository.DeleteAsync(refreshToken);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);

            if (isWeb)
            {
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("AccessToken");
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");
            }

            var jti = await _redisService.GetAsync($"user-jti:{user.Id}");
            if (!string.IsNullOrEmpty(jti))
            {
                await _redisService.DeleteAsync($"user-jti:{user.Id}");
                await _redisService.DeleteAsync($"jti:{jti}");
            }


            _logger.LogInformation("User, logout in system: {@user}", user);

            return Result<ResultTokenMessage>.Success(new ResultTokenMessage
            {
                Message = "Logout successful."
            }, 200);
        }
    }
}
