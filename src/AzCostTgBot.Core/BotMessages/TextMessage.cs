using AzCostTgBot.BotClients.Telegram;

namespace AzCostTgBot.Core.BotMessages;

public class TextMessage : BotMessageBase
{
    private readonly string _message;

    public TextMessage(string message)
    {
        _message = message;
    }

    public override TelegramMessage ToTelegramMessage()
    {
        return new TelegramMessage
        {
            Text = _message,
        };
    }
}
