using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzCostTgBot.Core.Providers.CostManagement.Models;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace AzCostTgBot.Core.Providers.CostManagement;

public class CostManagementProvider : ICostManagementProvider
{
    private const int ForecastRowCostIndex = 0;
    private const int ForecastRowTypeIndex = 1;
    private const int ForecastRowCurrencyIndex = 2;

    private readonly string _baseUrl = "https://management.azure.com/";
    private readonly string _apiVersion = "2022-10-01";
    private readonly HttpClient _httpClient;
    private readonly string _subscriptionId;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    public CostManagementProvider(HttpClient httpClient, IOptions<AzureConfiguration> options)
    {
        _httpClient = httpClient;
        _subscriptionId = options.Value.SubscriptionId;
    }

    public async Task<TotalForecast> GetTotalForecast(DateOnly from, DateOnly until, CancellationToken cancellation = default)
    {
        var forecastRequest = new ForecastRequestModel
        {
            Type = "Usage",
            Timeframe = "Custom",
            TimePeriod = new TimePeriodModel { From = from, To = until, },
            IncludeActualCost = true,
            IncludeFreshPartialCost = true,
            Dataset = new DatasetModel
            {
                Granularity = Granularity.None,
                Aggregation =
                    new Dictionary<string, Aggregation>
                    {
                        ["totalCost"] = new() { Name = "Cost", Function = "Sum", },
                    },
                Grouping = [new Grouping { Type = "Dimension", Name = "Subscription" },],
            },
        };

        var requestUri = GetRequestUri("forecast");
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(forecastRequest, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json"),
        };

        var response = await GetResponse<ForecastResponse>(request, cancellation);
        if (response is null)
        {
            throw new InvalidOperationException("Failed to get forecast response.");
        }

        return ToTotalForecast(response);
    }

    private async Task<T?> GetResponse<T>(HttpRequestMessage request, CancellationToken cancellation)
    {
        var token = await GetAccessToken(cancellation);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellation);
        _ = response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadAsStringAsync(cancellation);

        return JsonSerializer.Deserialize<T>(res, _jsonSerializerOptions);
    }

    private Uri GetRequestUri(string endpoint)
    {
        return new Uri($"{_baseUrl}/subscriptions/{_subscriptionId}/providers/Microsoft.CostManagement/{endpoint}?api-version={_apiVersion}");
    }

    private static async Task<string> GetAccessToken(CancellationToken cancellation)
    {
        var azCredential = new DefaultAzureCredential();
        var tokenRequestContext = new TokenRequestContext(["https://management.azure.com/.default"]);

        var token = await azCredential.GetTokenAsync(tokenRequestContext, cancellation);

        return token.Token;
    }

    private static TotalForecast ToTotalForecast(ForecastResponse response)
    {
        var actual = response.Properties.Rows.FirstOrDefault(r => r[ForecastRowTypeIndex].ValueEquals("Actual"));
        if (actual is null)
        {
            throw new InvalidOperationException("Failed to get actual cost.");
        }

        var forecasted = response.Properties.Rows.FirstOrDefault(r => r[ForecastRowTypeIndex].ValueEquals("Forecast"));
        var forecastedCost = forecasted?[ForecastRowCostIndex].Deserialize<decimal>();

        var currency = actual[ForecastRowCurrencyIndex].GetString() ?? string.Empty;
        var actualCost = actual[ForecastRowCostIndex].Deserialize<decimal>();

        return new TotalForecast(currency, actualCost, forecastedCost);
    }
}
