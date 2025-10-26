using App.Core.DTOs;
using App.Core.Jwt;
using App.Core.Result;
using App.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthServerController : ControllerBase
    {
        private readonly AuthService tokenService;

        public AuthServerController(AuthService tokenService)
        {
            this.tokenService = tokenService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 200)]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 401)]       
        public async Task<IActionResult> Login(LoginDto model)
        {
            var result = await tokenService.LoginAsync(model.UserNumber, model.Password);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 200)]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 401)]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto model)
        {
            var result = await tokenService.RefreshToken(model);

            return StatusCode(result.StatusCode, result);
        }

        
        [HttpPost("logout")]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 200)]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 401)]
        public async Task<IActionResult> Logout(LogoutDto model)
        {
            var result = await tokenService.Logout(model);

            return StatusCode(result.StatusCode, result);
        }        
    }
}
