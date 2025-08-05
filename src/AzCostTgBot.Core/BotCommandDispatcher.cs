using MediatR;

namespace AzCostTgBot.Core;

public class BotCommandDispatcher
{
    private class CommandRegistration
    {
        public string Description { get; set; } = string.Empty;
        public Func<string[], IRequest> Factory { get; set; } = default!;
    }

    private readonly Dictionary<string, CommandRegistration> _commands = new(StringComparer.OrdinalIgnoreCase) { };

    public void Register<TCommand>(string commandName, string description, Func<string[], TCommand> factory)
        where TCommand : IRequest
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            throw new ArgumentException("Command name cannot be null or whitespace.", nameof(commandName));
        }

        ArgumentNullException.ThrowIfNull(factory);
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be null or whitespace.", nameof(description));
        }

        _commands[commandName] = new CommandRegistration
        {
            Description = description,
            Factory = args => factory(args),
        };
    }

    public async Task<bool> DispatchAsync(string commandName, string[] args, IMediator mediator, CancellationToken cancellationToken)
    {
        if (_commands.TryGetValue(commandName, out var registration))
        {
            var command = registration.Factory(args);
            await mediator.Send(command, cancellationToken);
            return true;
        }

        return false;
    }

    public string GetHelpMessage()
    {
        if (_commands.Count == 0)
        {
            return "No commands available.";
        }

        return "Available commands:\n" + string.Join("\n", _commands.Select(kvp => $"{kvp.Key} - {kvp.Value.Description}"));
    }

    public IEnumerable<string> RegisteredCommands => _commands.Keys;
}
