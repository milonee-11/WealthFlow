using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(MongoDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        try
        {
            Console.WriteLine($"\n=== SIGNUP ATTEMPT ===");
            Console.WriteLine($"FullName: {request.FullName}");
            Console.WriteLine($"Email: {request.Email}");
            Console.WriteLine($"Time: {DateTime.UtcNow}");

            // Validate input
            if (string.IsNullOrEmpty(request.FullName) || 
                string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters" });
            }

            // Check if user exists
            var existingUser = await _context.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            // Create new user
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                MonthlyBudget = 2000,
                EmailNotifications = true
            };

            await _context.Users.InsertOneAsync(user);
            Console.WriteLine($"✅ User created with ID: {user.Id}");

            // Generate token
            var token = GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.MonthlyBudget,
                    user.EmailNotifications
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Signup error: {ex.Message}");
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            Console.WriteLine($"\n=== LOGIN ATTEMPT ===");
            Console.WriteLine($"Email: {request.Email}");
            Console.WriteLine($"Time: {DateTime.UtcNow}");

            var user = await _context.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            
            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            Console.WriteLine($"✅ Login successful for: {user.Email}");

            var token = GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.MonthlyBudget,
                    user.EmailNotifications
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Login error: {ex.Message}");
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { 
            message = "Auth controller is working!", 
            time = DateTime.UtcNow,
            endpoints = new[] {
                "POST /api/auth/signup",
                "POST /api/auth/login",
                "GET /api/auth/test"
            }
        });
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = GetUserId();
            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            return Ok(new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                MonthlyBudget = user.MonthlyBudget,
                EmailNotifications = user.EmailNotifications,
                CreatedAt = user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("update-email-notifications")]
    public async Task<IActionResult> UpdateEmailNotifications([FromBody] EmailNotificationsRequest request)
    {
        try
        {
            var userId = GetUserId();
            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            
            if (user == null)
                return NotFound("User not found");

            user.EmailNotifications = request.Enabled;
            await _context.Users.ReplaceOneAsync(u => u.Id == userId, user);

            return Ok(new { 
                message = "Email notifications updated successfully!",
                emailNotifications = user.EmailNotifications
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string inputPassword, string storedHash)
    {
        var hashOfInput = HashPassword(inputPassword);
        return hashOfInput == storedHash;
    }

    private string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"] ?? "your-secret-key-min-32-characters-long!");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class SignupRequest
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class UserResponse
{
    public string? Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public double MonthlyBudget { get; set; }
    public bool EmailNotifications { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmailNotificationsRequest
{
    public bool Enabled { get; set; }
}