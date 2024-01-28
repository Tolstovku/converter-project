using Common;
using Common.Database;
using Converter;
using Microsoft.EntityFrameworkCore;

const string corsPolicy = "MyCorsPolicy";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Username=postgres_user;Password=postgres_password;Database=sample_database"));

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy,
        builder => builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddSingleton<IMessageQueueClient, RabbitMqClient>();
builder.Services.AddHostedService<StartupHostedService>();
builder.Services.AddHostedService<FinishedConvertingMessageHandler>();


var app = builder.Build();
app.UseSession();
app.UseMiddleware<SimpleAuthMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapHub<FileStatusHub>("/status");

app.MapControllers();
app.UseCors(corsPolicy);

app.Run();



