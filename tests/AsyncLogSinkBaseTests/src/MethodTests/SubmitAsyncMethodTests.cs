using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;
using WB.Logging.LogSinks.Base;

namespace AsyncLogSinkBaseTests.MethodTests.SubmitAsyncMethodTests;

internal sealed class LogMessage<TPayload> : ILogMessage<TPayload>
    where TPayload : notnull
{
    public required TPayload Payload { get; init; }

    public DateTimeOffset Timestamp { get; set; }

    public IReadOnlyList<string> Senders { get; set; } = [];

    public LogLevel? LogLevel { get; set; }

    object ILogMessage.Payload => Payload;
}


internal sealed class DefaultLogMessageWriter : IAsyncLogMessageWriter<object>
{
    public List<ILogMessage> WrittenMessages { get; } = [];

    public ValueTask WriteAsync(ILogMessage<object> logMessage, CancellationToken cancellationToken)
    {
        WrittenMessages.Add(logMessage);

        return ValueTask.CompletedTask;
    }
}

internal sealed class StringLogMessageWriter : IAsyncLogMessageWriter<string>
{
    public List<ILogMessage<string>> WrittenMessages { get; } = [];

    public ValueTask WriteAsync(ILogMessage<string> logMessage, CancellationToken cancellationToken)
    {
        WrittenMessages.Add(logMessage);
        
        return ValueTask.CompletedTask;
    }
}

internal sealed class TestLogSink : AsyncLogSinkBase<TestLogSink>
{
    [SetsRequiredMembers]
    public TestLogSink()
    {
        DefaultLogMessageWriter = new DefaultLogMessageWriter();
    }
}

public sealed class TheSubmitMethod
{
    [Test]
    public async Task ShouldWriteLogMessagesUsingRegisteredLogMessageWriter()
    {
        // Arrange
        TestLogSink logSink = new();
        logSink.RegisterLogMessageWriter<StringLogMessageWriter>();
        StringLogMessageWriter logMessageWriter = logSink.ServiceContainer.Resolve<StringLogMessageWriter>();
        LogMessage<string> logMessage = new()
        {
            Timestamp = DateTimeOffset.UtcNow,
            Senders = ["TestSender"],
            Payload = "Test log message"
        };

        // Act
        await logSink.SubmitAsync(logMessage, CancellationToken.None);

        // Assert
        logMessageWriter.WrittenMessages.Should().ContainSingle().Which.Should().Be(logMessage);
    }
}