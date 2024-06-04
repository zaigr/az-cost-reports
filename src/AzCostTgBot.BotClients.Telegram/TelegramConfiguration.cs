namespace AzCostTgBot.BotClients.Telegram;

public class TelegramConfiguration
{
    public string ChatId { get; init; } = string.Empty;

    public string Token { get; init; } = string.Empty;

    public string? UatProxy { get; init; }
}
