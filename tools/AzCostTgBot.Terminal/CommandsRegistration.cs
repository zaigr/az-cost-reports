using AzCostTgBot.Core.Commands.SendAccumulatedCostForecast;
using AzCostTgBot.Core.Commands.SendLastBillingPeriodRgBreakdown;
using Cocona;
using MediatR;

namespace AzCostTgBot.Terminal;

public static class CommandsRegistration
{
    public static void AddAppCommands(this CoconaApp app)
    {
        app.AddSubCommand("send", cmd =>
        {
            cmd.AddCommand("forecast", async (IMediator mediator) =>
            {
                await mediator.Send(new SendAccumulatedCostForecastCommand());
            });
            cmd.AddCommand("rg-pie", async (IMediator mediator) =>
            {
                await mediator.Send(new SendLastBillingPeriodRgBreakdownCommand());
            });
        });
    }
}
