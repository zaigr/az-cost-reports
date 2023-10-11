namespace AzCostTgBot.Core.Providers.CostManagement.Models;

public class DatasetModel
{
    public Granularity Granularity { get; init; }

    public required IDictionary<string, Aggregation> Aggregation { get; init; }
    
    public required IEnumerable<Grouping> Grouping { get; init; }
}
