using MediatR;

namespace AzCostTgBot.Core.Notifications.BotMessageCreated;

public record BotMessageCreatedNotification(BotMessageBase Message) : INotification;
