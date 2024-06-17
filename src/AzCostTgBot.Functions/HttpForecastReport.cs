using System.Net;
using AzCostTgBot.Core.Commands.SendLastBillingPeriodRgBreakdown;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzCostTgBot.Functions;

public static class HttpForecastReport
{
    [Function("HttpForecastReport")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
        FunctionContext context)
    {
        var logger = context.GetLogger("HttpForecastReport");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var mediator = context.InstanceServices.GetRequiredService<IMediator>();

        await mediator.Send(new SendLastBillingPeriodRgBreakdownCommand(), context.CancellationToken);

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
