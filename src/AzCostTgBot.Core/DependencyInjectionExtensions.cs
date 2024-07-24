using System.Diagnostics.CodeAnalysis;
using AzCostTgBot.Core.Commands.SendAccumulatedCostForecast;
using AzCostTgBot.Core.Providers;
using AzCostTgBot.Core.Providers.Billing;
using AzCostTgBot.Core.Providers.CostManagement;
using AzCostTgBot.Core.Providers.Credential;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace AzCostTgBot.Core;

[SuppressMessage("Style", "IDE0058:Expression value is never used")]
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, Action<AzureConfiguration> azureCfg)
    {
        services.Configure(azureCfg);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<SendAccumulatedCostForecastCommand>();
        });

        services.AddScoped<IAzureCredentialProvider, AzureCredentialProvider>();

        services
            .AddHttpClient<IBillingProvider, BillingProvider>()
            .AddPolicyHandler(GetRetryPolicy());

        services
            .AddHttpClient<ICostManagementProvider, CostManagementProvider>()
            .AddPolicyHandler(GetRetryPolicy());

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
