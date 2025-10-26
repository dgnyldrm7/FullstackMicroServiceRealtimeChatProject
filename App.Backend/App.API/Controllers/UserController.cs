using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Result;
using App.Logic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace App.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;
        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        [Authorize]
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(typeof(Result<UserDto>), 400)]
        public async Task<IActionResult> GetUserById(string userId)
        {
            Result<UserDto> result = await userService.GetUserByIdAsync(userId);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(Result<UserDto>), 200)]
        [ProducesResponseType(typeof(Result<UserDto>), 404)]
        public async Task<IActionResult> GetMe()
        {
            Result<UserDto> result = await userService.GetMeAsync();

            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpGet("by-number/{userNumber}")]
        [ProducesResponseType(typeof(Result<UserDto>), 200)]
        [ProducesResponseType(typeof(Result<UserDto>), 404)]
        public async Task<IActionResult> GetUserByNumber(string userNumber)
        {
            Result<UserDto> result = await userService.GetUserByNumber(userNumber);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpGet("by-mail/{userMail}")]
        [ProducesResponseType(typeof(Result<UserDto>), 200)]
        [ProducesResponseType(typeof(Result<UserDto>), 404)]
        public async Task<IActionResult> GetUserByMail(string userMail)
        {
            Result<AppUser> result = await userService.GetUserByEmail(userMail);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(Result<List<UserDto>>), 200)]
        [ProducesResponseType(typeof(Result<List<UserDto>>), 404)]
        public async Task<IActionResult> GetAllUser()
        {
            var result = await userService.GetAllUsersAsync();

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Result<IdentityResult>), 201)]
        [ProducesResponseType(typeof(Result<IdentityResult>), 409)]
        [ProducesResponseType(typeof(Result<IdentityResult>), 400)]
        public async Task<IActionResult> RegisterUser(CreateUserDto createUserDto)
        {
            Result<IdentityResult> newUser = await userService.RegisterUserAsync(createUserDto);

            return StatusCode(newUser.StatusCode, newUser);
        }
    }
}
