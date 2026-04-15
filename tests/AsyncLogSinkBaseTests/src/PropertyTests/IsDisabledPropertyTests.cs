using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;
using WB.Logging.LogSinks.Base;

namespace AsyncLogSinkBaseTests.PropertyTests.IsDisabledPropertyTests;

internal sealed class TestWriter
{
}

internal sealed class TestLogSink() : AsyncLogSinkBase<TestWriter>(new TestLogMessageWriter(), new TestWriter())
{
}

internal sealed class TestLogMessageWriter : IAsyncLogMessageWriter<object, TestWriter>
{
    public bool Called { get; private set; }

    public TestWriter Writer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IAsyncLogSink? LogSink { get; set; }

    public ValueTask WriteAsync(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, object? payload)
    {
        Called = true;

        return ValueTask.CompletedTask;
    }
}

public sealed class TheIsDisabledProperty
{
    [Test]
    public void ShouldReturnFalseByDefault()
    {
        // Arrange
        TestLogSink logSink = new();

        // Act
        bool isDisabled = logSink.IsDisabled;

        // Assert
        isDisabled.Should().BeFalse(because: "log sinks should be enabled by default");
    }

    [Test]
    [Arguments(true, false)]
    [Arguments(false, true)]
    public async Task ShouldSetAndGetIsDisabledProperty(bool isDisabled, bool logMessageWriterCalled)
    {
        // Arrange
        TestLogSink logSink = new()
        {
            IsDisabled = isDisabled
        };

        TestLogMessageWriter logMessageWriter = (TestLogMessageWriter)logSink.DefaultLogMessageWriter;

        // Act
        await logSink.SubmitAsync(new LogMessage<object>() { Payload = new object() }).ConfigureAwait(false);

        // Assert
        logMessageWriter.Called.Should().Be(logMessageWriterCalled, because: "the log message writer should only be called when the log sink is not disabled");
    }
}