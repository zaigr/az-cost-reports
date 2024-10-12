using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace AzCostTgBot.BotClients.Telegram;

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

    public async Task<Result> SendMessageAsync(TelegramMessage message, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            return Result.Fail($"{nameof(message.Text)} cannot be null or white space.");
        }

        try
        {
            IReadOnlyCollection<IAlbumInputMedia> media = message switch
            {
                TelegramMediaMessage<TelegramMediaPhoto> photoMessage => photoMessage.Media
                    .Select(p => new InputMediaPhoto(new InputFileStream(p.Media, p.Name))
                    {
                        Caption = p.Caption ?? message.Text,
                    })
                    .ToList(),
                _ => []
            };

            if (media.Count != 0)
            {
                await _client.SendMediaGroupAsync(_chatId, media, cancellationToken: cancellation).ConfigureAwait(false);
            }
            else
            {
                await _client.SendTextMessageAsync(_chatId, message.Text, cancellationToken: cancellation).ConfigureAwait(false);
            }
        }
        catch (RequestException e)
        {
            _logger.LogError(e, "Exception in telegram client");
            return Result.Fail(e.Message);
        }

        return Result.Ok();
    }
}
