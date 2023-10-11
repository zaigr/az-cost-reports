using AzCostTgBot.Core.Extensions;
using AzCostTgBot.Infra.BotClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AzCostTgBot.Core.Notifications.AccumulatedCostForecastCreated;

public class AccumulatedCostForecastCreatedNotificationTelegramHandler : INotificationHandler<AccumulatedCostForecastCreatedNotification>
{
    private readonly ITelegramSender _telegramSender;
    private readonly ILogger<AccumulatedCostForecastCreatedNotificationTelegramHandler> _logger;

    public AccumulatedCostForecastCreatedNotificationTelegramHandler(
        ITelegramSender telegramSender,
        ILogger<AccumulatedCostForecastCreatedNotificationTelegramHandler> logger)
    {
        _telegramSender = telegramSender;
        _logger = logger;
    }

    public async Task Handle(AccumulatedCostForecastCreatedNotification notification, CancellationToken cancellationToken)
    {
        var date = DateTimeOffset.UtcNow;
        var formatterMessage = $"""
        Cost for {date:d/M/yy}:
            Today: {notification.DailyCost:F} {notification.CurrencyCode}
            Forecast: {notification.AccumulatedMonthCost:F} {notification.CurrencyCode}
        """;

        var result = await _telegramSender.SendMessageAsync(formatterMessage, cancellationToken);

        if (result.IsFailed)
        {
            _logger.LogError("Telegram handler failed to execute: {Error}", result.GetErrorMessage());
            return;
        }
        
        _logger.LogInformation("Telegram handler successfully executed.");
    }
}
