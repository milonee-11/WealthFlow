using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.API.Services;
using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetAlertController : ControllerBase
    {
        private readonly EmailService _emailService;

        public BudgetAlertController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-budget-alert")]
        public async Task<IActionResult> SendBudgetAlert([FromBody] BudgetAlertRequest request)
        {
            try
            {
                var alert = new ExpenseTracker.API.Models.Alert
                {
                    UserId = request.UserId,
                    Type = request.Type,
                    Message = request.Message,
                    Severity = request.Severity,
                    Timestamp = DateTime.UtcNow
                };

                await _emailService.SendBudgetAlertEmail(request.Email, request.UserName, alert);
                
                return Ok(new { 
                    message = "Budget alert email sent successfully!",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }

    public class BudgetAlertRequest
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
        public string Severity { get; set; } = "";
    }
}
