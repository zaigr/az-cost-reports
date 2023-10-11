using FluentResults;

namespace AzCostTgBot.Infra.BotClients;

public interface ITelegramSender
{
    Task<Result> SendMessageAsync(string message, CancellationToken cancellation = default);
}
