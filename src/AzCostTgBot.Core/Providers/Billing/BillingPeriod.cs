namespace AzCostTgBot.Core.Providers.Billing;

public record BillingPeriod(
    string Name,
    DateOnly Start,
    DateOnly End);  
