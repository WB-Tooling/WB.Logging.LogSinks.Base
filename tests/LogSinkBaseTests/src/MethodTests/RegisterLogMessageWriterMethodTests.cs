using System;
using System.Collections.Generic;
using AwesomeAssertions;
using WB.Logging;
using WB.Logging.LogSinks.Base;

namespace LogSinkBaseTests.MethodTests.RegisterLogMessageWriterMethodTests;

internal sealed class TestLogSink() : LogSinkBase(new TestLogMessageWriter())
{
}

internal sealed class TestLogMessageWriter : ILogMessageWriter<object>
{
    public void Write(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, object? payload)
    {
        throw new NotImplementedException();
    }
}

public sealed class TheRegisterLogMessageWriterMethod
{
    [Test]
    public void ShouldRegisterLogMessageWriterForPayloadType()
    {
        // Arrange
        TestLogMessageWriter logMessageWriter = new();
        TestLogSink logSink = new();

        // Act
        logSink.RegisterLogMessageWriter(logMessageWriter);

        // Assert
        logSink.LogMessageWriters.Should().ContainSingle().Which.Should().BeSameAs(logMessageWriter);
    }

    [Test]
    public void ShouldUnregisterLogMessageWriterWhenReturnedDisposableIsDisposed()
    {
        // Arrange
        TestLogMessageWriter logMessageWriter = new();
        TestLogSink logSink = new();
        IDisposable registration = logSink.RegisterLogMessageWriter(logMessageWriter);

        // Assert
        logSink.LogMessageWriters.Should().ContainSingle(because: "the log message writer should be registered").Which.Should().BeSameAs(logMessageWriter);

        // Act
        registration.Dispose();

        // Assert
        logSink.LogMessageWriters.Should().BeEmpty(because: "the log message writer should be unregistered when the returned disposable is disposed");
    }
}