using DiscordClone.Data.Context;
using DiscordClone.Data.Models;
using DiscordClone.Data.Repository.Interfaces;
using DiscordClone.Data.Repository;
using Microsoft.EntityFrameworkCore;
using DiscordClone.Services.Data;
using DiscordClone.Services.Data.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add MSSQL database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:5173") // Default Vite dev server port
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add Authentication with custom handling
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "CustomScheme";
    options.DefaultAuthenticateScheme = "CustomScheme";
    options.DefaultChallengeScheme = "CustomScheme";
})
.AddCookie("CustomScheme", options =>
{
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
    options.Cookie.Name = "DiscordClone.Auth";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowReactApp");

// Add these in this order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();