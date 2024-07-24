using AzCostTgBot.Core.Notifications.BotMessageCreated;
using AzCostTgBot.Core.Providers.CostManagement;
using AzCostTgBot.Drawing;
using MediatR;

namespace AzCostTgBot.Core.Commands.SendLastBillingPeriodRgBreakdown;

public record SendLastBillingPeriodRgBreakdownCommand : IRequest;

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
                Width = 2500,
                FontSize = 48,
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

        var grouped = new List<ResourceCost>();
        var groupedPercentage = 0m;
        var maxPercentage = 90;

        var totalCost = resourceGroups.Sum(r => r.Cost);
        for (var i = 0; i < resourceGroups.Count && (groupedPercentage <= maxPercentage); i++)
        {
            var resourceCostPercentage = ordered[i].Cost / totalCost * 100;
            if ((groupedPercentage + resourceCostPercentage) <= maxPercentage || (grouped.Count == 0))
            {
                grouped.Add(ordered[i]);
                groupedPercentage += resourceCostPercentage;
            }
        }

        var skippedResourcesCost = ordered
            .Skip(grouped.Count)
            .Sum(rg => rg.Cost);
        if (skippedResourcesCost > 0)
        {
            grouped.Add(ordered[0] with
            {
                Name = "other",
                Cost = skippedResourcesCost,
            });
        }

        return grouped
            .Select(r => (r.Name, r.Cost))
            .ToList();
    }
}
