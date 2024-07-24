using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AzCostTgBot.Core.Providers.Billing.Models;
using AzCostTgBot.Core.Providers.Credential;
using Azure.Core;
using Microsoft.Extensions.Options;

namespace AzCostTgBot.Core.Providers.Billing;

public class BillingProvider : IBillingProvider
{
    private readonly string _baseUrl = "https://management.azure.com/";
    private readonly string _apiVersion = "2018-03-01-preview";
    private readonly HttpClient _httpClient;
    private readonly string _subscriptionId;
    private readonly IAzureCredentialProvider _credentialProvider;

    public BillingProvider(HttpClient httpClient, IAzureCredentialProvider credentialProvider, IOptions<AzureConfiguration> options)
    {
        _httpClient = httpClient;
        _credentialProvider = credentialProvider;
        _subscriptionId = options.Value.SubscriptionId;
    }

    public async Task<BillingPeriod> GetBillingPeriod(int year, int month, CancellationToken cancellation = default)
    {
        var billingPeriodName = GetBillingPeriodName(year, month);

        var uri = GetRequestUri($"billingPeriods/{billingPeriodName}");
        var token = await GetAccessToken(cancellation);

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellation);
        _ = response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BillingPeriodResponse>(cancellationToken: cancellation);
        EnsureValidResult(result);

        var billingPeriod = new BillingPeriod(
            result!.Name,
            DateOnly.Parse(result.Properties[BillingPeriodResponse.StartDateProperty], CultureInfo.InvariantCulture),
            DateOnly.Parse(result.Properties[BillingPeriodResponse.EndDateProperty], CultureInfo.InvariantCulture));

        return billingPeriod;
    }

    private Uri GetRequestUri(string endpoint)
    {
        return new Uri($"{_baseUrl}/subscriptions/{_subscriptionId}/providers/Microsoft.Billing/{endpoint}?api-version={_apiVersion}");
    }

    private async Task<string> GetAccessToken(CancellationToken cancellation)
    {
        var tokenRequestContext = new TokenRequestContext(["https://management.azure.com/.default"]);

        var token = await _credentialProvider.GetToken(tokenRequestContext, cancellation);

        return token.Token;
    }

    private static void EnsureValidResult(BillingPeriodResponse? result)
    {
        var isValid = result != null
                      && result.Properties.ContainsKey(BillingPeriodResponse.StartDateProperty)
                      && result.Properties.ContainsKey(BillingPeriodResponse.EndDateProperty);

        if (!isValid)
        {
            throw new InvalidOperationException("Invalid billing period response.");
        }
    }

    private static string GetBillingPeriodName(int year, int month)
    {
        return $"{year}{month:D2}-1";
    }
}
