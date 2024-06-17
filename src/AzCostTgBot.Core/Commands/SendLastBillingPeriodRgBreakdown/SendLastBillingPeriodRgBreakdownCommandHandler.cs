using AzCostTgBot.Core.Notifications.BotMessageCreated;
using AzCostTgBot.Core.Providers.CostManagement;
using AzCostTgBot.Drawing;
using MediatR;

namespace AzCostTgBot.Core.Commands.SendLastBillingPeriodRgBreakdown;

public class SendLastBillingPeriodRgBreakdownCommandHandler : IRequestHandler<SendLastBillingPeriodRgBreakdownCommand>
{
    private readonly IMediator _mediator;
    private readonly ICostManagementProvider _costManagementProvider;
    private readonly IPlotting _plotting;

    public SendLastBillingPeriodRgBreakdownCommandHandler(
        IMediator mediator,
        ICostManagementProvider costManagementProvider,
        IPlotting plotting)
    {
        _mediator = mediator;
        _costManagementProvider = costManagementProvider;
        _plotting = plotting;
    }

    public async Task Handle(SendLastBillingPeriodRgBreakdownCommand request, CancellationToken cancellationToken)
    {
        var resourceGroups = await _costManagementProvider.GetRgLastBillingPeriodCost(cancellationToken);
        if (resourceGroups.Count == 0)
        {
            return;
        }

        var grouped = GroupByTopCost(resourceGroups);

        var chart = _plotting.Pie(
            grouped.Select(g => g.Cost).ToArray(),
            grouped.Select(g => g.Name).ToArray(),
            new PlotOptions
            {
                Width = 2000,
                FontSize = 64,
            });

        var botMessage = new ResourceGroupBreakdownBotMessage
        {
            CurrencyCode = resourceGroups.First().Currency,
            Breakdown = grouped,
            Chart = chart,
        };

        var notification = new BotMessageCreatedNotification(botMessage);
        await _mediator.Publish(notification, cancellationToken);
    }

    private static List<(string Name, decimal Cost)> GroupByTopCost(IReadOnlyCollection<ResourceCost> resourceGroups)
    {
        var ordered = resourceGroups
            .OrderByDescending(rg => rg.Cost)
            .ToList();

        var top = ordered
            .Select(rg => (rg.Name, rg.Cost))
            .ToList();

        var other = ordered
            .Skip(5)
            .Sum(rg => rg.Cost);

        top.Add(("other", other));

        return top;
    }
}
