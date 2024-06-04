using AzCostTgBot.BotClients.Telegram;
using AzCostTgBot.Core.Commands.SendAccumulatedCostForecast;
using AzCostTgBot.Core.Providers;
using AzCostTgBot.Core.Providers.Billing;
using AzCostTgBot.Core.Providers.CostManagement;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddUserSecrets<Program>();
    })
    .ConfigureServices((host, services) =>
    {
        services.AddLogging();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<SendAccumulatedCostForecastCommand>();
        });

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.Configure<AzureConfiguration>(cfg =>
        {
            cfg.SubscriptionId = GetSubscriptionIdEnvironmentValue()
                                 ?? host.Configuration.GetValue("AzureSubscriptionId", string.Empty)!;
        });

        services
            .AddHttpClient<IBillingProvider, BillingProvider>()
            .AddPolicyHandler(GetRetryPolicy());

        services
            .AddHttpClient<ICostManagementProvider, CostManagementProvider>()
            .AddPolicyHandler(GetRetryPolicy());

        services
            .AddOptions<TelegramConfiguration>()
            .BindConfiguration("Telegram");

        services
            .AddHttpClient("TelegramClient")
            .AddPolicyHandler(GetRetryPolicy())
            .AddTypedClient<ITelegramSender, TelegramSender>();
    })
    .Build();

host.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static string? GetSubscriptionIdEnvironmentValue()
{
    return Environment.GetEnvironmentVariable("WEBSITE_OWNER_NAME")?.Split('+').First();
}
