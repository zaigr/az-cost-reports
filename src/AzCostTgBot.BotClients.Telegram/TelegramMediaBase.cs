namespace AzCostTgBot.BotClients.Telegram;

public abstract class TelegramMediaBase
{
    public abstract string Type { get; }

    public string? Caption { get; init; }

    public required string Name { get; init; }

    public required Stream Media { get; init; }
}
