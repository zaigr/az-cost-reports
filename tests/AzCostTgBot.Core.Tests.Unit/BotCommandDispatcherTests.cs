using MediatR;
using Moq;

namespace AzCostTgBot.Core.Tests.Unit;

public class BotCommandDispatcherTests
{
    private readonly BotCommandDispatcher _dispatcher;
    private readonly Mock<IMediator> _mediatorMock;

    public BotCommandDispatcherTests()
    {
        _dispatcher = new BotCommandDispatcher();
        _mediatorMock = new Mock<IMediator>();
    }

    private void RegisterTestCommand(string commandName = "/test", string description = "Test command")
    {
        _dispatcher.Register(commandName, description, args => new TestRequest());
    }

    private class TestRequest : IRequest { }

    [Fact]
    public async Task DispatchAsync_KnownCommand_CallsMediatorAndReturnsTrue()
    {
        // Arrange
        RegisterTestCommand();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(MediatR.Unit.Value))
            .Verifiable();

        // Act
        var result = await _dispatcher.DispatchAsync("/test", [], _mediatorMock.Object, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_UnknownCommand_DoesNotCallMediatorAndReturnsFalse()
    {
        // Arrange
        // No command registered

        // Act
        var result = await _dispatcher.DispatchAsync("/unknown", [], _mediatorMock.Object, CancellationToken.None);

        // Assert
        Assert.False(result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void GetHelpMessage_ReturnsDescriptionsForAllRegisteredCommands()
    {
        // Arrange
        RegisterTestCommand("/foo", "Foo description");
        RegisterTestCommand("/bar", "Bar description");

        // Act
        var help = _dispatcher.GetHelpMessage();

        // Assert
        Assert.Contains("/foo", help);
        Assert.Contains("Foo description", help);
        Assert.Contains("/bar", help);
        Assert.Contains("Bar description", help);
    }
}
