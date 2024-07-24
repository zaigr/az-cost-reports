using AzCostTgBot.BotClients.Telegram;
using AzCostTgBot.Core;
using AzCostTgBot.Drawing;
using AzCostTgBot.Terminal;
using Cocona;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = CoconaApp.CreateBuilder();

builder.Host
    .ConfigureAppConfiguration((_, cfg) =>
    {
        cfg
            .AddUserSecrets<Program>()
            .AddJsonFile("appsettings.json");
    });

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(dispose: true);
});

builder.Services
    .AddTelegramBotClients()
    .AddDrawing()
    .AddCore(az =>
    {
        az.SubscriptionId = builder.Configuration.GetValue("AzureSubscriptionId", string.Empty)!;
    });

var app = builder.Build();

app.AddAppCommands();

app.Run();

