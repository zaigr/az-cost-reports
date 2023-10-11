using MediatR;

namespace AzCostTgBot.Core.Notifications.AccumulatedCostForecastCreated;

public record AccumulatedCostForecastCreatedNotification(
    decimal DailyCost,
    decimal AccumulatedMonthCost,
    string CurrencyCode) : INotification;
