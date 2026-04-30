using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;
using WB.Logging.LogSinks.Base;

namespace AsyncLogSinkBaseTests.MethodTests.SubmitAsyncMethodTests;

internal sealed class TestWriter
{
}

internal sealed class DefaultLogMessageWriter : IAsyncLogMessageWriter<TestLogSink, object>
{
    public List<string?> WrittenMessages { get; } = [];
    
    [NotNull]
    public TestLogSink? LogSink { get; set; }

    public ValueTask WriteAsync(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, object? payload)
    {
        WrittenMessages.Add(payload?.ToString());

        return ValueTask.CompletedTask;
    }
}

internal sealed class StringLogMessageWriter : IAsyncLogMessageWriter<TestLogSink, string>
{
    public List<string?> WrittenMessages { get; } = [];
    
    [NotNull]
    public TestLogSink? LogSink { get; set; }

    public ValueTask WriteAsync(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, string? payload)
    {
        WrittenMessages.Add(payload);
        
        return ValueTask.CompletedTask;
    }
}

internal sealed class TestLogSink() : AsyncLogSinkBase<TestLogSink>(new DefaultLogMessageWriter())
{
}

public sealed class TheSubmitMethod
{
    [Test]
    public async Task ShouldWriteLogMessageUsingRegisteredLogMessageWriter()
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
        await logSink.SubmitAsync(logMessage);

        // Assert
        logMessageWriter.WrittenMessages.Should().ContainSingle().Which.Should().Be("Test log message");
    }

    [Test]
    public async Task ShouldWriteLogMessageUsingDefaultLogMessageWriterWhenNoSpecificWriterIsRegistered()
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
        await logSink.SubmitAsync(logMessage);

        // Assert
        defaultLogMessageWriter.WrittenMessages.Should().ContainSingle().Which.Should().Be("Test log message");
    }

    [Test]
    public async Task ShouldThrowArgumentNullExceptionWhenLogMessageIsNull()
    {
        // Arrange
        TestLogSink logSink = new();

        // Act
        Func<Task> action = () => logSink.SubmitAsync<string>(null!).AsTask();

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>().WithParameterName("logMessage", because: "the log message cannot be null");
    }
}