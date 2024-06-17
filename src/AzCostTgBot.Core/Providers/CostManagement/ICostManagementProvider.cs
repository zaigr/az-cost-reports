namespace AzCostTgBot.Core.Providers.CostManagement;

public interface ICostManagementProvider
{
    public Task<TotalForecast> GetTotalForecast(DateOnly from, DateOnly until, CancellationToken cancellation = default);

    public Task<IReadOnlyCollection<ResourceCost>> GetRgLastBillingPeriodCost(CancellationToken cancellation = default);
}
