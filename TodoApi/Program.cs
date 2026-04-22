using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))
    ));
    
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://authclient-ip48.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // אם אתה שולח Cookies או Headers של Auth
    });
});

var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("my_super_secret_key_123456789012"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("https://authclient-ip48.onrender.com") // הכתובת של הריאקט ב-Render
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // חשוב מאוד אם אתה משתמש בטוקנים/עוגיות
        });
});

builder.Services.AddAuthorization();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
var app = builder.Build();
app.UseRouting();
app.UseCors("ReactPolicy"); 


app.UseAuthorization();


app.MapGet("/todos", async (ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst("id")?.Value;
    if (userIdClaim == null) return Results.Unauthorized();

    int userId = int.Parse(userIdClaim);
    var userTasks = await db.Items.Where(t => t.UserId == userId).ToListAsync();
    return Results.Ok(userTasks);
}).RequireAuthorization();

app.MapPost("/todos", async (Item item, ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst("id")?.Value;
    if (userIdClaim == null) return Results.Unauthorized();

    item.UserId = int.Parse(userIdClaim);
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/todos/{item.Id}", item);
}).RequireAuthorization();

app.MapPut("/todos/{id}", async (int id, Item input, ToDoDbContext db, ClaimsPrincipal user) =>
{
    // ✅ הוספת אימות משתמש
    var userIdClaim = user.FindFirst("id")?.Value;
    if (userIdClaim == null) return Results.Unauthorized();

    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();

    // ✅ ודא שהמשימה שייכת למשתמש המחובר
    if (item.UserId != int.Parse(userIdClaim)) return Results.Forbid();

    item.Name = input.Name;
    item.IsComplete = input.IsComplete;

    await db.SaveChangesAsync();
    return Results.Ok(item);
}).RequireAuthorization(); // ✅ הוספת אימות

app.MapDelete("/todos/{id}", async (int id, ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst("id")?.Value;
    if (userIdClaim == null) return Results.Unauthorized();

    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();

    // ✅ ודא שהמשימה שייכת למשתמש המחובר
    if (item.UserId != int.Parse(userIdClaim)) return Results.Forbid();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // ✅ הוספת אימות

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
app.MapGet("/todos/{id}", async (int id, ToDoDbContext db, ClaimsPrincipal user) =>
{
    var userIdClaim = user.FindFirst("id")?.Value;
    if (userIdClaim == null) return Results.Unauthorized();

    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();
    if (item.UserId != int.Parse(userIdClaim)) return Results.Forbid();

    return Results.Ok(item);
}).RequireAuthorization();
app.Run();