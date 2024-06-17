namespace AzCostTgBot.Core.Providers.CostManagement;

public record ResourceCost(
    string Type,
    string Name,
    decimal Cost,
    string Currency);
