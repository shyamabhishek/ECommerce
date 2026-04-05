using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Models;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Using C# 12 Primary Constructor syntax here:
    public class AuthController(IConfiguration config, AppDbContext context) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // 1. Check if user already exists
            if (await context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists.");

            // 2. Hash the password using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Create and save the user
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash,
                Role = dto.Role
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            // 1. Find user by username
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == login.Username);

            // 2. Verify existence AND check the password hash
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            // 3. Generate token with roles
            var token = GenerateToken(user);
            return Ok(new { token });
        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Strict null check for the JWT Key (Best Practice)
            var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}