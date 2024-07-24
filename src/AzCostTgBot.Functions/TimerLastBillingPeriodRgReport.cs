using System.Diagnostics.CodeAnalysis;
using AzCostTgBot.Core.Commands.SendLastBillingPeriodRgBreakdown;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzCostTgBot.Functions;

public class TimerLastBillingPeriodRgReport
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TimerLastBillingPeriodRgReport> _logger;

    public TimerLastBillingPeriodRgReport(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<TimerLastBillingPeriodRgReport> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

    [Function("TimerLastBillingPeriodRgReport")]
    [FixedDelayRetry(5, "00:00:10")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public async Task Run([TimerTrigger("%LastBillingPeriodRgReport:Cron%")] TimerInfo myTimer, FunctionContext context)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {Now}", DateTime.UtcNow);

        if (!_configuration.GetValue<bool>("LastBillingPeriodRgReport:Enabled"))
        {
            _logger.LogInformation("Resource group report is disabled.");
            return;
        }

        await _mediator.Send(new SendLastBillingPeriodRgBreakdownCommand(), context.CancellationToken);

        _logger.LogInformation("C# Timer trigger finished at: {Now}", DateTime.UtcNow);
    }
}
