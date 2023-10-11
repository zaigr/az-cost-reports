namespace AzCostTgBot.Infra.BotClients;

public class TelegramConfiguration
{
    public string ChatId { get; init; } = default!;

    public string Token { get; init; } = default!;
    
    public string? UatProxy { get; init; }
}
