namespace AzCostTgBot.Core.Providers.Billing;

public interface IBillingProvider
{
    public Task<BillingPeriod?> GetBillingPeriod(int year, int month, CancellationToken cancellation = default);
}
