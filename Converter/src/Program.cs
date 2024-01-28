using Common;
using Common.Database;
using Converter;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IMessageQueueClient, RabbitMqClient>();
builder.Services.AddHostedService<ConverterService>();

var parentDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.FullName;
var appSettingsPath = Path.Combine(parentDirectory, "appsettings.json");

builder.Configuration.AddJsonFile(appSettingsPath);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

const string connectionString = "Host=localhost;Username=postgres_user;Password=postgres_password;Database=sample_database";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString), optionsLifetime: ServiceLifetime.Singleton);
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


var app = builder.Build();

app.Run();