using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Services; // Add this using
using MongoDB.Driver;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly EmailService _emailService;

    public ExpensesController(MongoDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    }

    [HttpGet]
    public async Task<IActionResult> GetExpenses()
    {
        var userId = GetUserId();
        var expenses = await _context.Expenses.Find(e => e.UserId == userId).ToListAsync();
        return Ok(expenses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetExpense(string id)
    {
        var userId = GetUserId();
        var expense = await _context.Expenses.Find(e => e.Id == id && e.UserId == userId).FirstOrDefaultAsync();
        
        if (expense == null)
            return NotFound();
        
        return Ok(expense);
    }

    [HttpPost]
    public async Task<IActionResult> AddExpense(Expense expense)
    {
        expense.UserId = GetUserId();
        expense.Date = DateTime.UtcNow;
        
        await _context.Expenses.InsertOneAsync(expense);

        // Check budget and send alert
        await CheckBudgetAndAlert(expense.UserId);

        // Send email notification if enabled
        await SendExpenseNotification(expense.UserId, expense, "added");

        return Ok(expense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(string id, Expense updatedExpense)
    {
        var userId = GetUserId();
        var expense = await _context.Expenses.Find(e => e.Id == id && e.UserId == userId).FirstOrDefaultAsync();
        
        if (expense == null)
            return NotFound();

        expense.Title = updatedExpense.Title;
        expense.Amount = updatedExpense.Amount;
        expense.Category = updatedExpense.Category;
        expense.Note = updatedExpense.Note;

        await _context.Expenses.ReplaceOneAsync(e => e.Id == id, expense);

        // Send email notification
        await SendExpenseNotification(userId, expense, "updated");

        return Ok(expense);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(string id)
    {
        var userId = GetUserId();
        var expense = await _context.Expenses.Find(e => e.Id == id && e.UserId == userId).FirstOrDefaultAsync();
        
        if (expense == null)
            return NotFound();

        await _context.Expenses.DeleteOneAsync(e => e.Id == id);

        // Send email notification
        await SendExpenseNotification(userId, expense, "deleted");

        return Ok();
    }

    private async Task CheckBudgetAndAlert(string userId)
    {
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null || !user.EmailNotifications) return;

        var expenses = await _context.Expenses.Find(e => e.UserId == userId).ToListAsync();
        var total = expenses.Sum(e => e.Amount);
        var percentage = (total / user.MonthlyBudget) * 100;

        Alert? alert = null;

        if (percentage >= 100)
        {
            alert = new Alert
            {
                UserId = userId,
                Type = "danger",
                Message = $"⚠️ CRITICAL: You've exceeded your monthly budget! Total: ₹{total:F2}, Budget: ₹{user.MonthlyBudget:F2}",
                Severity = "danger",
                Timestamp = DateTime.UtcNow
            };
        }
        else if (percentage >= 90)
        {
            alert = new Alert
            {
                UserId = userId,
                Type = "warning",
                Message = $"⚠️ Warning: You've used {percentage:F1}% of your monthly budget!",
                Severity = "warning",
                Timestamp = DateTime.UtcNow
            };
        }
        else if (percentage >= 75)
        {
            alert = new Alert
            {
                UserId = userId,
                Type = "info",
                Message = $"📊 You've used {percentage:F1}% of your monthly budget",
                Severity = "info",
                Timestamp = DateTime.UtcNow
            };
        }

        if (alert != null)
        {
            await _context.Alerts.InsertOneAsync(alert);
            
            // Only mark email as sent if it actually sends
            try
            {
                await _emailService.SendBudgetAlertEmail(user.Email, user.FullName, alert);
                
                // Update alert to mark email as sent
                var filter = Builders<Alert>.Filter.Eq(a => a.Id, alert.Id);
                var update = Builders<Alert>.Update.Set(a => a.EmailSent, true);
                await _context.Alerts.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email failed to send but alert saved: {ex.Message}");
                // Alert remains in database with EmailSent = false
            }
        }
    }

    private async Task SendExpenseNotification(string userId, Expense expense, string action)
    {
        var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null || !user.EmailNotifications) return;

        await _emailService.SendExpenseNotificationEmail(user.Email, user.FullName, expense, action);
    }
}