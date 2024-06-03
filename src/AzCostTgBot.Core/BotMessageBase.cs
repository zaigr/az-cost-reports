using AzCostTgBot.Infra.BotClients;

namespace AzCostTgBot.Core;

public abstract class BotMessageBase
{
    public abstract TelegramMessage ToTelegramMessage();
}
