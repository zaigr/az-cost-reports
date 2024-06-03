using AzCostTgBot.Core.Extensions;
using AzCostTgBot.Core.Notifications.BotMessageCreated;
using AzCostTgBot.Core.Providers.Billing;
using AzCostTgBot.Core.Providers.CostManagement;
using MediatR;

namespace AzCostTgBot.Core.Commands.SendAccumulatedCostForecast;

public class SendAccumulatedCostForecastCommandHandler : IRequestHandler<SendAccumulatedCostForecastCommand>
{
    private readonly IMediator _mediator;
    private readonly IBillingProvider _billingProvider;
    private readonly ICostManagementProvider _costManagementProvider;

    public SendAccumulatedCostForecastCommandHandler(
        IMediator mediator,
        IBillingProvider billingProvider,
        ICostManagementProvider costManagementProvider)
    {
        _mediator = mediator;
        _billingProvider = billingProvider;
        _costManagementProvider = costManagementProvider;
    }

    public async Task Handle(SendAccumulatedCostForecastCommand request, CancellationToken cancellationToken)
    {
        var billingPeriod = await GetCurrentBillingPeriod(cancellationToken);

        var forecast = await _costManagementProvider.GetTotalForecast(billingPeriod.Start, billingPeriod.End, cancellationToken);

        // TODO: handle missing forecast
        var botMessage = new AccumulatedCostForecastBotMessage
        {
            ActualCost = forecast.Actual,
            MonthForecastCost = forecast.Actual + (forecast.Forecast ?? 0),
            CurrencyCode = forecast.Currency,
            Date = DateTimeOffset.UtcNow,
        };
        var notification = new BotMessageCreatedNotification(botMessage);

        await _mediator.Publish(notification, cancellationToken);
    }

    private async Task<BillingPeriod> GetCurrentBillingPeriod(CancellationToken cancellation)
    {
        var now = DateTimeOffset.UtcNow;

        var billingDate = AtMonthStart(now.AddMonths(1));
        var billingPeriod = await _billingProvider.GetBillingPeriod(billingDate.Year, billingDate.Month, cancellation);

        if (billingPeriod.End.ToUtcDate() < now)
        {
            billingDate = AtMonthStart(now.AddMonths(2));
            billingPeriod = await _billingProvider.GetBillingPeriod(billingDate.Year, billingDate.Month, cancellation);
        }

        return billingPeriod;

        static DateTimeOffset AtMonthStart(DateTimeOffset date)
        {
            return new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, date.Offset);
        }
    }
}
