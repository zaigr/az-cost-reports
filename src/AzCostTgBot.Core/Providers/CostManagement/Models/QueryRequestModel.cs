namespace AzCostTgBot.Core.Providers.CostManagement.Models;

public class QueryRequestModel
{
    public required ExportType Type { get; init; }

    public required Timeframe Timeframe { get; init; }

    public TimePeriodModel? TimePeriod { get; init; }

    public required DatasetModel Dataset { get; init; }
}
