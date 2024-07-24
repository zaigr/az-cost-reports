using AzCostTgBot.BotClients.Telegram;

namespace AzCostTgBot.Core.Commands.SendLastBillingPeriodRgBreakdown;

public class ResourceGroupBreakdownBotMessage : BotMessageBase
{
    public required string CurrencyCode { get; init; }

    public required IEnumerable<(string Name, decimal Cost)> Breakdown { get; init; }

    public required Stream Chart { get; init; }

    public override TelegramMessage ToTelegramMessage()
    {
        return new TelegramMediaMessage<TelegramMediaPhoto>
        {
            Media = [new TelegramMediaPhoto { Media = Chart, Name = "chart", }],
            Text = Breakdown
                .Select(x => $"{(x.Cost >= 0.01m ? x.Cost : 0.01m):F2} {CurrencyCode}: {x.Name}")
                .Aggregate((x, y) => $"{x}\n{y}"),
        };
    }
}
