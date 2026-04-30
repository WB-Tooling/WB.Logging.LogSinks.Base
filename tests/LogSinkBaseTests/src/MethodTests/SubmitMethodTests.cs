using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using WB.Logging;
using WB.Logging.LogSinks.Base;

namespace LogSinkBaseTests.MethodTests.SubmitMethodTests;

internal sealed class DefaultLogMessageWriter : ILogMessageWriter<TestLogSink, object>
{
    [NotNull]
    public TestLogSink? LogSink { get; set; }

    public List<object?> WrittenMessages { get; } = [];

    public void Write(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, object? payload)
    {
        WrittenMessages.Add(payload);
    }
}

internal sealed class StringLogMessageWriter : ILogMessageWriter<TestLogSink, string>
{
    [NotNull]
    public TestLogSink? LogSink { get; set; }

    public List<string?> WrittenMessages { get; } = [];
    
    public void Write(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, string? payload)
    {
        WrittenMessages.Add(payload);
    }
}

internal sealed class TestLogSink() : LogSinkBase<TestLogSink>(new DefaultLogMessageWriter())
{
}

public sealed class TheSubmitMethod
{
    [Test]
    public void ShouldWriteLogMessageUsingRegisteredLogMessageWriter()
    {
        // Arrange
        StringLogMessageWriter logMessageWriter = new();
        TestLogSink logSink = new();
        logSink.RegisterLogMessageWriter(logMessageWriter);
        LogMessage<string> logMessage = new()
        {
            Timestamp = DateTimeOffset.UtcNow,
            Senders = ["TestSender"],
            Payload = "Test log message"
        };

        // Act
        logSink.Submit(logMessage);

        // Assert
        logMessageWriter.WrittenMessages.Should().ContainSingle().Which.Should().Be("Test log message");
    }

    [Test]
    public void ShouldWriteLogMessageUsingDefaultLogMessageWriterWhenNoSpecificWriterIsRegistered()
    {
        // Arrange
        TestLogSink logSink = new();
        LogMessage<string> logMessage = new()
        {
            Timestamp = DateTimeOffset.UtcNow,
            Senders = ["TestSender"],
            Payload = "Test log message"
        };
        DefaultLogMessageWriter defaultLogMessageWriter = (DefaultLogMessageWriter)logSink.DefaultLogMessageWriter;

        // Act
        logSink.Submit(logMessage);

        // Assert
        defaultLogMessageWriter.WrittenMessages.Should().ContainSingle().Which.Should().Be("Test log message");
    }

    [Test]
    public void ShouldThrowArgumentNullExceptionWhenLogMessageIsNull()
    {
        // Arrange
        TestLogSink logSink = new();

        // Act
        Action action = () => logSink.Submit<string>(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logMessage", because: "the log message cannot be null");
    }
}