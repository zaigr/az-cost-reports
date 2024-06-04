using FluentResults;

namespace AzCostTgBot.BotClients.Telegram;

public interface ITelegramSender
{
    Task<Result> SendMessageAsync(TelegramMessage message, CancellationToken cancellation = default);
}
