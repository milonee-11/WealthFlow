using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.API.Services;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestEmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public TestEmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-test")]
    public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            var testAlert = new ExpenseTracker.API.Models.Alert
            {
                UserId = "test-user",
                Type = "test",
                Message = request.Message,
                Severity = "info",
                Timestamp = DateTime.UtcNow
            };

            await _emailService.SendBudgetAlertEmail(request.Email, request.Name, testAlert);
            
            return Ok(new { 
                message = "Test email sent successfully!",
                email = request.Email,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                error = ex.Message,
                email = request.Email,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

public class TestEmailRequest
{
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string Message { get; set; } = "";
}
