using FluentResults;

namespace AzCostTgBot.Infra.BotClients;

public interface ITelegramSender
{
    Task<Result> SendMessageAsync(TelegramMessage message, CancellationToken cancellation = default);
}
