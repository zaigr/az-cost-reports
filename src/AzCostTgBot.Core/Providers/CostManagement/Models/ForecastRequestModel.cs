namespace AzCostTgBot.Core.Providers.CostManagement.Models;

public class ForecastRequestModel
{
    public required string Type { get; init; }

    public required TimePeriodModel TimePeriod { get; init; }

    public bool IncludeActualCost { get; init; }

    public bool IncludeFreshPartialCost { get; init; }

    public required string Timeframe { get; init; }

    public required DatasetModel Dataset { get; init; }
}
