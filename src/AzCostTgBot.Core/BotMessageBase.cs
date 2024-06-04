using AzCostTgBot.BotClients.Telegram;

namespace AzCostTgBot.Core;

public abstract class BotMessageBase
{
    public abstract TelegramMessage ToTelegramMessage();
}
