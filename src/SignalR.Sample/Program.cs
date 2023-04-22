using System.Runtime.CompilerServices;

using Microsoft.AspNetCore.Mvc;

using SignalR.Sample.Chat;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var hostName = Environment.GetEnvironmentVariable("HOST_NAME") ?? "localhost";
var portNumber = 6379;
var password = Environment.GetEnvironmentVariable("CACHE_PASSWORD") ?? "";

var connectionString = $"{hostName}:{portNumber}";

if (!string.IsNullOrEmpty(password))
{
    connectionString = $"{connectionString},password={password}";
}

builder.Services.AddSignalR()
    .AddStackExchangeRedis(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapGet(
    "/health",
    () => Results.Ok());

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();
