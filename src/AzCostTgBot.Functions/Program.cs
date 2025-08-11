using AzCostTgBot.BotClients.Telegram;
using AzCostTgBot.Core;
using AzCostTgBot.Core.Commands.SendAccumulatedCostForecast;
using AzCostTgBot.Core.Commands.SendLastBillingPeriodRgBreakdown;
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

        // Register and configure BotCommandDispatcher
        services.AddSingleton(_ =>
        {
            var dispatcher = new BotCommandDispatcher();
            dispatcher.Register("/forecast", "Show accumulated cost forecast for the current billing period.", args => new SendAccumulatedCostForecastCommand());
            dispatcher.Register("/breakdown", "Show last billing period resource group cost breakdown.", args => new SendLastBillingPeriodRgBreakdownCommand());
            return dispatcher;
        });
    })
    .Build();

host.Run();

static string? GetSubscriptionIdEnvironmentValue()
{
    return Environment.GetEnvironmentVariable("WEBSITE_OWNER_NAME")?.Split('+').First();
}
