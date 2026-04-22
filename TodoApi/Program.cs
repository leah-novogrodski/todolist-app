using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// --- 1. הגדרת מסד נתונים ---
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))
    ));

// --- 2. הגדרת CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://authclient-ip48.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// --- 3. הגדרת אימות (Authentication) - זה מה שהיה חסר! ---
var secretKey = "my_super_secret_key_123456789012"; // ודא שזה תואם למפתח ב-Login
var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = securityKey,
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

// הגדרת פורט ל-Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// --- 4. סדר ה-Middleware (קריטי מאוד!) ---
app.UseRouting();

app.UseCors("ReactPolicy"); // חייב לבוא לפני הראוטים

app.UseAuthentication(); // 1. מי המשתמש? (זה היה חסר)
app.UseAuthorization();  // 2. מה מותר לו לעשות?

// --- 5. הראוטים שלך (נשארים אותו דבר) ---
app.MapGet("/todos", async (ToDoDbContext db, ClaimsPrincipal user) => { 
    /* הקוד שלך */ 
}).RequireAuthorization();

// ... שאר ה-Endpoints (MapPost, MapPut וכו') ...

app.MapPost("/register", async (ToDoDbContext db, User newUser) => {
    /* הקוד שלך */
}).AllowAnonymous();

app.MapPost("/login", (ToDoDbContext db, User userLogin) => {
    /* הקוד שלך */
}).AllowAnonymous();

app.Run();