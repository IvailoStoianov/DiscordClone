using DiscordClone.Data.Context;
using DiscordClone.Data.Models;
using DiscordClone.Data.Repository.Interfaces;
using DiscordClone.Data.Repository;
using Microsoft.EntityFrameworkCore;
using DiscordClone.Services.Data;
using DiscordClone.Services.Data.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add MSSQL database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ChatRoomRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();