using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;
using WB.Logging.LogSinks.Base;

namespace LogSinkBaseTests.PropertyTests.IsDisabledPropertyTests;

internal sealed class TestWriter
{
}

internal sealed class TestLogSink() : LogSinkBase<TestWriter>(new TestLogMessageWriter(), new TestWriter())
{
}

internal sealed class TestLogMessageWriter : ILogMessageWriter<object, TestWriter>
{
    public bool Called { get; private set; }

    public TestWriter Writer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ILogSink? LogSink { get; set; }

    public void Write(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, object? payload)
    {
        Called = true;
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
    public void ShouldSetAndGetIsDisabledProperty(bool isDisabled, bool logMessageWriterCalled)
    {
        // Arrange
        TestLogSink logSink = new()
        {
            IsDisabled = isDisabled
        };

        TestLogMessageWriter logMessageWriter = (TestLogMessageWriter)logSink.DefaultLogMessageWriter;

        // Act
        logSink.Submit(new LogMessage<object>() { Payload = new object() });

        // Assert
        logMessageWriter.Called.Should().Be(logMessageWriterCalled, because: "the log message writer should only be called when the log sink is not disabled");
    }
}