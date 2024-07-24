using AzCostTgBot.BotClients.Telegram;
using AzCostTgBot.Core;
using AzCostTgBot.Drawing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddUserSecrets<Program>();
    })
    .ConfigureServices((host, services) =>
    {
        services.AddLogging();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services
            .AddTelegramBotClients()
            .AddDrawing()
            .AddCore(azure =>
            {
                azure.SubscriptionId = GetSubscriptionIdEnvironmentValue()
                                       ?? host.Configuration.GetValue("AzureSubscriptionId", string.Empty)!;
            });
    })
    .Build();

host.Run();

static string? GetSubscriptionIdEnvironmentValue()
{
    return Environment.GetEnvironmentVariable("WEBSITE_OWNER_NAME")?.Split('+').First();
}
