using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UserService.Models;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;

        public UserController(UserDbContext context)
        {
            _context = context;
        }

        // Helper method to check if the current user is authorized (either the user or an Admin)
        private bool IsUserAuthorized(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return false;
            return userIdClaim == id.ToString() || User.IsInRole("Admin");
        }

        // Helper method to get a user by ID
        private async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        // Get all users (Admin only)
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            if (!User.IsInRole("Admin"))
                return StatusCode(403, new { message = "You are not authorized to access this resource." });

            var users = await _context.Users.ToListAsync();
            if (users == null || !users.Any())
                return NotFound(new { message = "No users found." });

            return Ok(users);
        }

        // Get user by ID (Own profile or Admin)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            if (!IsUserAuthorized(id))
                return StatusCode(403, new { message = "You are not authorized to access this resource." });

            var user = await GetUserByIdAsync(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found." });

            return Ok(user);
        }

        // Update user details (Admin only, or user updating themselves)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (!IsUserAuthorized(id))
                return StatusCode(403, new { message = "You are not authorized to access this resource." });

            var user = await GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found." });

            if (user.Email != updatedUser.Email)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == updatedUser.Email);
                if (existingUser != null)
                    return BadRequest(new { message = "Email already in use, please choose a different one." });
            }

            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;
            user.IsActive = updatedUser.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "User updated successfully.", user });
        }

        // Delete user (Admin only)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!User.IsInRole("Admin"))
                return StatusCode(403, new { message = "You are not authorized to access this resource." });

            var user = await GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }
    }
}
