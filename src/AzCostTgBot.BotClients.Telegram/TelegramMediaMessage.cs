namespace AzCostTgBot.BotClients.Telegram;

public class TelegramMediaMessage<T> : TelegramMessage
    where T : TelegramMediaBase
{
    public required IEnumerable<T> Media { get; init; }
}
