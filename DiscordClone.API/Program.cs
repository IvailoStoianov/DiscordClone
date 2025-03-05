using DiscordClone.Data.Context;
using DiscordClone.Data.Models;
using DiscordClone.Data.Repository.Interfaces;
using DiscordClone.Data.Repository;
using Microsoft.EntityFrameworkCore;
using DiscordClone.Services.Data;
using DiscordClone.Services.Data.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using DiscordClone.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add MSSQL database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add repositories
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ChatRoomRepository>();
builder.Services.AddScoped<MessageRepository>();

//Add servies
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add controllers
builder.Services.AddControllers();

// Add CORS configuration before app.UseHttpsRedirection()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:5173") 
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Update Cookie Authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Cookies";
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    
    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = async context =>
        {
            Console.WriteLine("Validating Principal");
            Console.WriteLine($"Cookie present: {context.Request.Cookies[".AspNetCore.Cookies"] != null}");
            if (context.Principal?.Identity?.IsAuthenticated == true)
            {
                foreach (var claim in context.Principal.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }
            }
            else
            {
                Console.WriteLine("Principal is not authenticated");
                if (context.Principal == null)
                    Console.WriteLine("Principal is null");
                else if (context.Principal.Identity == null)
                    Console.WriteLine("Identity is null");
            }
        },
        OnRedirectToLogin = context =>
        {
            Console.WriteLine("Redirecting to login");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnSigningIn = context =>
        {
            Console.WriteLine("Signing in user");
            foreach (var claim in context.Principal.Claims)
            {
                Console.WriteLine($"Setting claim: {claim.Type} = {claim.Value}");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDistributedMemoryCache();

// Configure SignalR with proper authentication and options
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100 KB
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add CORS middleware before routing/endpoints
app.UseCors("AllowReactApp");

// Add these in this order
app.UseRouting();
app.UseAuthentication();

// Add this middleware after UseAuthentication
app.Use(async (context, next) =>
{
    Console.WriteLine("\n=== Request Details ===");
    Console.WriteLine($"Request Path: {context.Request.Path}");
    Console.WriteLine($"Request Method: {context.Request.Method}");
    Console.WriteLine("=== Cookies ===");
    foreach (var cookie in context.Request.Cookies)
    {
        Console.WriteLine($"Cookie: {cookie.Key} = {cookie.Value.Substring(0, Math.Min(cookie.Value.Length, 20))}...");
    }
    Console.WriteLine("=== Authentication Status ===");
    Console.WriteLine($"Is Authenticated: {context.User?.Identity?.IsAuthenticated}");
    Console.WriteLine($"Authentication Type: {context.User?.Identity?.AuthenticationType}");
    
    await next();
});

app.UseAuthorization();

// Map controllers and SignalR hub
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();