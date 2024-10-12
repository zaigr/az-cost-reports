using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace AzCostTgBot.BotClients.Telegram;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddTelegramBotClients(this IServiceCollection services, string configSectionPath = "Telegram")
    {
        services
            .AddOptions<TelegramConfiguration>()
            .BindConfiguration(configSectionPath);

        services
            .AddHttpClient("TelegramClient")
            .AddPolicyHandler(GetRetryPolicy())
            .AddTypedClient<ITelegramSender, TelegramSender>();

        return services;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
