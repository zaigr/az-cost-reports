namespace AzCostTgBot.Core.Providers.Billing.Models;

public class BillingPeriodResponse
{
    public const string StartDateProperty = "billingPeriodStartDate";
    public const string EndDateProperty = "billingPeriodEndDate";

    public string Name { get; init; } = string.Empty;

    public IDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}
