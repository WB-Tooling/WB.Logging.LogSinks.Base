using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using FakeItEasy;
using WB.Logging;
using WB.Logging.LogSinks.Base;

namespace AsyncLogSinkBaseTests.MethodTests.DisableMethodTests;

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

public sealed class TheDisableMethod
{
    [Test]
    public void ShouldSetIsDisabledToTrue()
    {
        // Arrange
        TestLogSink logSink = new();

        // Act
        logSink.Disable();

        // Assert
        logSink.IsDisabled.Should().BeTrue(because: "calling Disable() should set IsDisabled to true");
    }

    [Test]
    public void ShouldThrowInvalidOperationExceptionIfAlreadyDisabled()
    {
        // Arrange
        TestLogSink logSink = new();

        // Act
        Action action = () =>
        {
            logSink.Disable();
            logSink.Disable();
        };

        // Act & Assert
        action.Should().Throw<InvalidOperationException>(because: "calling Disable() on an already disabled log sink should throw an InvalidOperationException");
    }

    [Test]
    [Arguments(true, false, DisplayName = "when log sink is disabled")]
    [Arguments(false, true, DisplayName = "when log sink is enabled")]
    public async Task ShouldPreventProcessingLogMessages(bool isDisabled, bool logMessageWriterCalled)
    {
        // Arrange
        TestLogMessageWriter logMessageWriter = new();
        TestLogSink logSink = new();
        logSink.RegisterLogMessageWriter(logMessageWriter);

        if (isDisabled)
        {
            logSink.Disable();
        }

        ILogMessage<object> logMessage = A.Fake<ILogMessage<object>>();

        // Act
        await logSink.SubmitAsync(logMessage);

        // Assert
        logMessageWriter.Called.Should().Be(logMessageWriterCalled, because: "the log message writer should only be called if the log sink is not disabled");
    }
}