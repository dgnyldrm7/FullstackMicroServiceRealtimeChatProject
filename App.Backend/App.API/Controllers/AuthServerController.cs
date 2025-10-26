using System.Collections.Generic;
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

        [HttpPost("mobile/refresh-token")]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 200)]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 401)]
        public async Task<IActionResult> RefreshTokenMobile(RefreshTokenDto model)
        {
            var result = await tokenService.RefreshToken(model);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("web/refresh-token")]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 200)]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 401)]
        public async Task<IActionResult> RefreshTokenWeb(RefreshTokenDto model)
        {
            var result = await tokenService.RefreshToken(model);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("mobile/logout")]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 200)]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 401)]
        public async Task<IActionResult> LogoutMobile(LogoutDto model, bool isWeb = false)
        {
            var result = await tokenService.LogoutInternal(model.UserNumber, isWeb, model.RefreshToken);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("web/logout")]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 200)]
        [ProducesResponseType(typeof(Result<ResultTokenMessage>), 401)]
        public async Task<IActionResult> LogoutWeb()
        {
            var result = await tokenService.LogoutInternal(isWeb: true);

            return StatusCode(result.StatusCode, result);
        }
    }
}