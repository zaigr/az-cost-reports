using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace AzCostTgBot.Infra.BotClients;

public class TelegramSender : ITelegramSender
{
    private readonly string _chatId;
    private readonly TelegramBotClient _client;
    private readonly ILogger<TelegramSender> _logger;

    public TelegramSender(
        HttpClient httpClient,
        IOptions<TelegramConfiguration> configuration,
        ILogger<TelegramSender> logger)
    {
        _chatId = configuration.Value.ChatId;
        _logger = logger;

        var botClientOptions = new TelegramBotClientOptions(
            configuration.Value.Token,
            configuration.Value.UatProxy,
            useTestEnvironment: configuration.Value.UatProxy is not null);
        _client = new TelegramBotClient(botClientOptions, httpClient);
    }

    public async Task<Result> SendMessageAsync(string message, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Result.Fail($"{nameof(message)} cannot be null or white space.");
        }
        
        try
        {
            await _client.SendTextMessageAsync(_chatId, message, cancellationToken: cancellation);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in telegram client");
            return Result.Fail(e.Message);
        }

        return Result.Ok();
    }
}
