using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using WB.Logging;
using WB.Logging.LogSinks.Base;

namespace LogSinkBaseTests.MethodTests.SubmitMethodTests;

internal sealed class LogMessage<TPayload> : ILogMessage<TPayload>
    where TPayload : notnull
{
    public required TPayload Payload { get; init; }

    public DateTimeOffset Timestamp { get; set; }

    public IReadOnlyList<string> Senders { get; set; } = [];

    public LogLevel? LogLevel { get; set; }

    object ILogMessage.Payload => Payload;
}

internal sealed class DefaultLogMessageWriter : ILogMessageWriter<object>
{
    public List<ILogMessage> WrittenMessages { get; } = [];

    public void Write(ILogMessage<object> logMessage)
    {
        WrittenMessages.Add(logMessage);
    }
}

internal sealed class StringLogMessageWriter : ILogMessageWriter<string>
{
    public List<ILogMessage<string>> WrittenMessages { get; } = [];
    
    public void Write(ILogMessage<string> logMessage)
    {
        WrittenMessages.Add(logMessage);
    }
}

internal sealed class TestLogSink : LogSinkBase<TestLogSink>
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
    public void ShouldWriteLogMessageUsingRegisteredLogMessageWriter()
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
        logSink.Submit(logMessage);

        // Assert
        logMessageWriter.WrittenMessages.Should().ContainSingle().Which.Should().Be(logMessage);
    }
}