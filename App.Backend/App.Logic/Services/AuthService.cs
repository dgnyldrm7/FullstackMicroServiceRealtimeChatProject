using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Interface;
using App.Core.Interface.Redis;
using App.Core.Jwt;
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

        public AuthService(IJwtTokenService tokenService, IRefreshTokenRepository refreshTokenRepository, UserManager<AppUser> userManager, IOptions<JwtSettings> jwtOptions, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger, IRedisService redisService, IUnitOfWork unitOfWork)
        {
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            jwtSettings = jwtOptions.Value;
            _redisService = redisService;
            this.unitOfWork = unitOfWork;
        }

        //Login
        public async Task<Result<ResultTokenMessage>> LoginAsync(string userNumber, string password)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == userNumber);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
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

                _logger.LogInformation($"New refresh token is generated., Time : {newExpiry}");
            }

            var (accessToken, jti) = _tokenService.GenerateAccessToken(user.Id, user.PhoneNumber!, user.UserName!, await _userManager.GetRolesAsync(user));

            // Redis ile JTI yönetimi
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
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpireMinutes)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("AccessToken", accessToken, accessTokenCookieOptions);


            var data = new ResultTokenMessage
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken!,
                Message = "Token generated successfully",
            };

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

            RefreshToken? refreshToken = await _refreshTokenRepository.GetByTokenAsync(model.RefreshToken, user.PhoneNumber!);

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
            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                "AccessToken",
                accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(jwtSettings.AccessTokenExpireMinutes)
                });

            ResultTokenMessage data = new ResultTokenMessage
            {
                AccessToken = accessToken,
                Message = "New access token generated.",
            };

            return Result<ResultTokenMessage>.Success(data, 200);
        }

        //Logout
        public async Task<Result<ResultTokenMessage>> Logout(LogoutDto model)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.UserNumber);

            if (user == null)
            {
                // Kullanıcı bulunamadıysa 401 Unauthorized
                return Result<ResultTokenMessage>.Failure("User is not found.", 401);
            }

            RefreshToken? refreshToken = await _refreshTokenRepository.GetByTokenAsync(model.RefreshToken, user.PhoneNumber!);

            if (refreshToken != null)
            {
                await _refreshTokenRepository.DeleteAsync(model.RefreshToken);

                await unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
            else
            {
                return Result<ResultTokenMessage>.Failure("Refresh token is faulty.", 401);
            }

            //Cookie’leri sil -> Access Token
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("AccessToken");

            var jti = await _redisService.GetAsync($"user-jti:{user.Id}");

            if (!string.IsNullOrEmpty(jti))
            {
                await _redisService.DeleteAsync($"user-jti:{user.Id}");
                await _redisService.DeleteAsync($"jti:{jti}");
            }

            ResultTokenMessage result = new ResultTokenMessage
            {
                Message = "Logout successful.",              
            };

            return Result<ResultTokenMessage>.Success(result, 200);
        }
    }
}
