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
        builder.Configuration.GetConnectionString("ToDoListDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoListDB"))
    ));

// --- 2. הגדרת CORS (פתרון סופי ל-Preflight) ---
// --- הגדרת ה-CORS המעודכנת ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins("https://authclient-ip48.onrender.com") // בלי לוכסן בסוף
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var secretKey = "my_super_secret_key_123456789012"; 
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
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero // מונע עיכובים באימות פקיעת תוקף
    };
});

builder.Services.AddAuthorization();

// הגדרת פורט עבור Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();
app.UseRouting();

// --- 4. סדר ה-Middleware (הסדר קריטי למניעת CORS Error) ---
// ה-CORS חייב לבוא מיד אחרי Routing ולפני Authentication
app.UseCors("ReactPolicy"); 




app.UseAuthentication(); 
app.UseAuthorization();

// --- 5. הגדרת ה-Endpoints ---
app.MapGet("/", () => Results.Content("<h1>Server is Online!</h1>", "text/html"));
// יצירת משתמש חדש
app.MapPost("/register", async (ToDoDbContext db, User newUser) =>
{
    if (db.Users.Any(u => u.name == newUser.name)) 
        return Results.BadRequest("User already exists");

    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim("id", newUser.id.ToString()) }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
    };
    
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { token = tokenHandler.WriteToken(token) });
}).AllowAnonymous();

// התחברות
app.MapPost("/login", (ToDoDbContext db, User userLogin) =>
{
    var user = db.Users.FirstOrDefault(u => u.name == userLogin.name && u.password == userLogin.password);
    if (user == null) return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim("id", user.id.ToString()) }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { token = tokenHandler.WriteToken(token) });
}).AllowAnonymous();

// קבלת משימות - מוגן
app.MapGet("/todos", async (ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst("id")?.Value;
    if (userIdClaim == null) return Results.Unauthorized();

    int userId = int.Parse(userIdClaim);
    var userTasks = await db.Items.Where(t => t.UserId == userId).ToListAsync();
    return Results.Ok(userTasks);
}).RequireAuthorization();

// הוספת משימה - מוגן
app.MapPost("/todos", async (Item item, ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst("id")?.Value;
    if (userIdClaim == null) return Results.Unauthorized();

    item.UserId = int.Parse(userIdClaim);
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/todos/{item.Id}", item);
}).RequireAuthorization();

// מחיקת משימה - מוגן
app.MapDelete("/todos/{id}", async (int id, ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst("id")?.Value;
    if (userIdClaim == null) return Results.Unauthorized();

    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();
    if (item.UserId != int.Parse(userIdClaim)) return Results.Forbid();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// ... כאן אפשר להוסיף את שאר ה-Put וה-Get לפי ה-id באותו פורמט ...
app.MapGet("/test", () => Results.Ok("Server is working!")).AllowAnonymous();
app.Run();