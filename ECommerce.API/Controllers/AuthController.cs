using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost("login")]
        public IActionResult Login(LoginDto login)
        {
            //dummy validation
            if (login.Username != "admin" || login.Password != "123")
                return Unauthorized();
            var token = GenerateToken(login.Username);
               return Ok(new{ token });
        }
        private string GenerateToken(string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
