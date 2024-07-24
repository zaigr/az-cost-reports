using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzCostTgBot.Core.Providers.CostManagement.Models;
using AzCostTgBot.Core.Providers.Credential;
using Azure.Core;
using Microsoft.Extensions.Options;

namespace AzCostTgBot.Core.Providers.CostManagement;

public class CostManagementProvider : ICostManagementProvider
{
    private const int ForecastRowCostIndex = 0;
    private const int ForecastRowTypeIndex = 1;
    private const int ForecastRowCurrencyIndex = 2;

    private const string QueryCostColumn = "Cost";
    private const string QueryResourceGroupColumn = "ResourceGroup";
    private const string QueryCurrencyColumn = "Currency";

    private readonly string _baseUrl = "https://management.azure.com/";
    private readonly string _apiVersion = "2022-10-01";
    private readonly HttpClient _httpClient;
    private readonly string _subscriptionId;
    private readonly IAzureCredentialProvider _credentialProvider;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    public CostManagementProvider(HttpClient httpClient, IAzureCredentialProvider credentialProvider, IOptions<AzureConfiguration> options)
    {
        _httpClient = httpClient;
        _credentialProvider = credentialProvider;
        _subscriptionId = options.Value.SubscriptionId;
    }

    public async Task<TotalForecast> GetTotalForecast(DateOnly from, DateOnly until, CancellationToken cancellation = default)
    {
        var forecastRequest = new ForecastRequestModel
        {
            Type = ExportType.Usage,
            Timeframe = Timeframe.Custom,
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

    public async Task<IReadOnlyCollection<ResourceCost>> GetRgLastBillingPeriodCost(CancellationToken cancellation = default)
    {
        var requestModel = new QueryRequestModel
        {
            Type = ExportType.ActualCost,
            Timeframe = Timeframe.TheLastBillingMonth,
            Dataset = new DatasetModel
            {
                Granularity = Granularity.None,
                Aggregation =
                    new Dictionary<string, Aggregation>
                    {
                        ["totalCost"] = new() { Name = "Cost", Function = "Sum", },
                    },
                Grouping = [new Grouping { Type = "Dimension", Name = "ResourceGroup" },],
            },
        };

        var requestUri = GetRequestUri("query");
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestModel, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json"),
        };

        var response = await GetResponse<QueryResponse>(request, cancellation);
        if (response is null)
        {
            throw new InvalidOperationException("Failed to get forecast response.");
        }

        return ToResourceGroupCost(response);
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

    private async Task<string> GetAccessToken(CancellationToken cancellation)
    {
        var tokenRequestContext = new TokenRequestContext(["https://management.azure.com/.default"]);

        var token = await _credentialProvider.GetToken(tokenRequestContext, cancellation);

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

    private static List<ResourceCost> ToResourceGroupCost(QueryResponse response)
    {
        var costIndex = Array.FindIndex(response.Properties.Columns, c => c.Name == QueryCostColumn);
        var rgIndex = Array.FindIndex(response.Properties.Columns, c => c.Name == QueryResourceGroupColumn);
        var currencyIndex = Array.FindIndex(response.Properties.Columns, c => c.Name == QueryCurrencyColumn);

        return response.Properties.Rows
            .Select(r => new ResourceCost(
                Type: QueryResourceGroupColumn,
                r[rgIndex].GetString() ?? string.Empty,
                r[costIndex].Deserialize<decimal>(),
                r[currencyIndex].GetString() ?? string.Empty))
            .ToList();
    }
}
