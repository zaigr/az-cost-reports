using AzCostTgBot.Core.Extensions;
using AzCostTgBot.Infra.BotClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AzCostTgBot.Core.Notifications.BotMessageCreated;

public class BotMessageCreatedTelegramHandler : INotificationHandler<BotMessageCreatedNotification>
{
    private readonly ITelegramSender _telegramSender;
    private readonly ILogger<BotMessageCreatedTelegramHandler> _logger;

    public BotMessageCreatedTelegramHandler(
        ITelegramSender telegramSender,
        ILogger<BotMessageCreatedTelegramHandler> logger)
    {
        _telegramSender = telegramSender;
        _logger = logger;
    }

    public async Task Handle(BotMessageCreatedNotification notification, CancellationToken cancellationToken)
    {
        var result = await _telegramSender.SendMessageAsync(notification.Message.ToTelegramMessage(), cancellationToken);

        if (result.IsFailed)
        {
            _logger.LogError("Telegram handler failed to execute: {Error}", result.GetErrorMessage());
            return;
        }

        _logger.LogInformation("Telegram handler successfully executed.");
    }
}
