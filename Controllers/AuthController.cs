using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserService.Models;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _jwtKey;

        public AuthController(UserDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key", "JWT Key is not set in configuration.");
        }

        // Login for authenticate user and return JWT token
        [HttpPost("login")]
        public IActionResult Login([FromBody] Auth auth)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == auth.Email && u.IsActive);
            if (user == null || !VerifyPassword(auth.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials." });

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token});
        }

        // Create a new user account
        [HttpPost("register")]
        public IActionResult Register([FromBody] Register register)
        {
            if (_context.Users.Any(u => u.Email == register.Email))
                return BadRequest(new { message = "Email is already in use." });

            var newUser = new User
            {
                FullName = register.FullName,
                Email = register.Email,
                PasswordHash = HashPassword(register.Password),
                Role = "User",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();
            return Ok(new { message = "User registered successfully.", user = newUser });
        }

        // users to reset their password
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPassword resetPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == resetPassword.Email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.PasswordHash = HashPassword(resetPassword.NewPassword);
            user.IsActive = true;
            _context.SaveChanges();

            return Ok(new { message = "Password reset successfully." });
        }

        // Helper method to hash the password
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        // Helper method to verify password
        private bool VerifyPassword(string password, string? storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                throw new ArgumentNullException(nameof(storedHash), "Stored hash cannot be null or empty.");

            return HashPassword(password) == storedHash;
        }

        // Generate JWT token
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? throw new ArgumentNullException("FullName")),
                new Claim(ClaimTypes.Email, user.Email ?? throw new ArgumentNullException("Email")),
                new Claim(ClaimTypes.Role, user.Role ?? throw new ArgumentNullException("Role"))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
