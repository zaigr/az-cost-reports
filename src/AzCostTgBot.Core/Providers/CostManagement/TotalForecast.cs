namespace AzCostTgBot.Core.Providers.CostManagement;

public record TotalForecast(
    string Currency,
    decimal Actual,
    decimal? Forecast);
