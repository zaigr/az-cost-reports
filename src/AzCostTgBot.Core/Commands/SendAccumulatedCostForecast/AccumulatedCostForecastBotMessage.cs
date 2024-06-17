using AzCostTgBot.BotClients.Telegram;

namespace AzCostTgBot.Core.Commands.SendAccumulatedCostForecast;

public class AccumulatedCostForecastBotMessage : BotMessageBase
{
    public required decimal ActualCost { get; init; }

    public required decimal MonthForecastCost { get; init; }

    public required string CurrencyCode { get; init; }

    public required DateTimeOffset Date { get; set; }

    public override TelegramMessage ToTelegramMessage()
    {
        var text = $"""
            Cost for {Date:d/M/yy}:
                Actual: {ActualCost:F} {CurrencyCode}
                Forecast: {MonthForecastCost:F} {CurrencyCode}
        """;

        return new TelegramMessage
        {
            Text = text,
        };
    }
}

