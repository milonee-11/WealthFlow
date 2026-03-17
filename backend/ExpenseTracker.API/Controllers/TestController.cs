using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            message = "Test controller is working!",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { 
            status = "alive", 
            timestamp = DateTime.UtcNow,
            database = "MongoDB",
            connection = "localhost:27017"
        });
    }
}