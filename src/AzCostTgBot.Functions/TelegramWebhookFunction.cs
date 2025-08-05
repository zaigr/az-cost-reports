using System.Net;
using System.Text.Json;
using AzCostTgBot.BotClients.Telegram;
using AzCostTgBot.Core;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace AzCostTgBot.Functions;

public class TelegramWebhookFunction
{
    private readonly ILogger<TelegramWebhookFunction> _logger;
    private readonly string _expectedSecret;
    private readonly IMediator _mediator;
    private readonly ITelegramSender _telegramSender;
    private readonly BotCommandDispatcher _dispatcher;

    public TelegramWebhookFunction(
        ILogger<TelegramWebhookFunction> logger,
        IConfiguration configuration,
        IMediator mediator,
        ITelegramSender telegramSender,
        BotCommandDispatcher dispatcher)
    {
        _logger = logger;
        _expectedSecret = configuration["Telegram:WebhookSecret"] ?? string.Empty;
        _mediator = mediator;
        _telegramSender = telegramSender;
        _dispatcher = dispatcher;
    }

    [Function("TelegramWebhook")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "telegram/webhook")] HttpRequestData req)
    {
        if (!req.Headers.TryGetValues("X-Telegram-Bot-Api-Secret-Token", out var headerValues) ||
            string.IsNullOrEmpty(_expectedSecret) ||
            headerValues.FirstOrDefault() != _expectedSecret)
        {
            _logger.LogWarning("Unauthorized webhook attempt: missing or invalid secret token.");

            var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorized.WriteStringAsync("Unauthorized");

            return unauthorized;
        }

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation("Received Telegram webhook: {Body}", requestBody);

        Update? update = null;
        try
        {
            update = JsonSerializer.Deserialize<Update>(requestBody);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize Telegram update");
        }

        var text = update?.Message?.Text;
        if (!string.IsNullOrWhiteSpace(text) && text.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            var command = text.Split(' ')[0];
            var args = text.Length > command.Length ? text[command.Length..].Trim().Split(' ') : [];
            _logger.LogInformation("Extracted command: {Command}, args: {Args}", command, string.Join(",", args));

            if (command.Equals("/help", StringComparison.OrdinalIgnoreCase))
            {
                var helpMessage = _dispatcher.GetHelpMessage();
                await _telegramSender.SendMessageAsync(new TelegramMessage { Text = helpMessage });
            }
            else if (await _dispatcher.DispatchAsync(command, args, _mediator, default))
            {
                await _telegramSender.SendMessageAsync(new TelegramMessage { Text = $"{command} command received!" });
            }
            else
            {
                var helpMessage = _dispatcher.GetHelpMessage();
                await _telegramSender.SendMessageAsync(new TelegramMessage { Text = $"Unknown command.\n{helpMessage}" });
            }
        }
        else
        {
            _logger.LogInformation("No command found in message or message is null.");
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Webhook received");

        return response;
    }
}
