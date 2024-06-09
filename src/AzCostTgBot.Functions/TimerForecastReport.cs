using System.Diagnostics.CodeAnalysis;
using AzCostTgBot.Core.Commands.SendAccumulatedCostForecast;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzCostTgBot.Functions;

public class TimerForecastReport
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TimerForecastReport> _logger;

    public TimerForecastReport(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<TimerForecastReport> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

    [Function("TimerForecastReport")]
    [FixedDelayRetry(5, "00:00:10")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public async Task Run([TimerTrigger("%ForecastReport:Cron%")] TimerInfo myTimer, FunctionContext context)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {Now}", DateTime.UtcNow);

        if (!_configuration.GetValue<bool>("ForecastReport:Enabled"))
        {
            _logger.LogInformation("Daily cost report is disabled.");
            return;
        }

        await _mediator.Send(new SendAccumulatedCostForecastCommand(), context.CancellationToken);

        _logger.LogInformation("C# Timer trigger finished at: {Now}", DateTime.UtcNow);
    }
}
