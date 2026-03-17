using ExpenseTracker.API.Data;
using ExpenseTracker.API.Services; // Add this using
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MongoDB Context
builder.Services.AddSingleton<MongoDbContext>();

// Register Email Service
builder.Services.AddSingleton<EmailService>(); // Add this line

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                builder.Configuration["JWT:Secret"] ?? "your-secret-key-min-32-characters-long!")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3001", "http://localhost:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WealthFlow API V1");
    });
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Root endpoint with dynamic port info
app.MapGet("/", () => {
    var port = Environment.GetEnvironmentVariable("ASPNETCORE_PORT") ?? "5000";
    return Results.Ok(new { 
        message = "WealthFlow API is running!", 
        status = "healthy",
        timestamp = DateTime.UtcNow,
        port = port,
        endpoints = new[] {
            "GET  /",
            "GET  /swagger",
            "GET  /api/test",
            "GET  /api/test/ping",
            "GET  /api/auth/test",
            "POST /api/auth/signup",
            "POST /api/auth/login",
            "GET  /api/expenses",
            "POST /api/expenses"
        }
    });
});

app.Run();