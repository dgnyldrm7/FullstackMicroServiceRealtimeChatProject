using App.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace App.API.Controllers
{
    [ApiController]
    [Route("apiworker/[controller]")]
    public class WorkerAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WorkerAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] WorkerLoginRequest request)
        {
            var serviceUser = _configuration["WorkerAuth:ServiceUser"];
            var serviceSecret = _configuration["WorkerAuth:ServiceSecret"];

            if (request.Username != serviceUser || request.Password != serviceSecret)
            {
                return Unauthorized("Invalid worker credentials");
            }                

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, "WorkerService"),
                    new Claim(ClaimTypes.Role, "SystemWorker")
                },
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }

    
}
