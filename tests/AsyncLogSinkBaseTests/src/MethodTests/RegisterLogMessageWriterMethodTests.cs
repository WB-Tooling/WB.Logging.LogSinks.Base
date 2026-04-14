using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using WB.Logging;
using WB.Logging.LogSinks.Base;

namespace AsyncLogSinkBaseTests.MethodTests.RegisterLogMessageWriterMethodTests;

internal sealed class TestWriter
{
}

internal sealed class TestLogSink() : AsyncLogSinkBase<TestWriter>(new TestLogMessageWriter(), new TestWriter())
{
}



internal sealed class TestLogMessageWriter : IAsyncLogMessageWriter<object, TestWriter>
{
    public TestWriter Writer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ValueTask WriteAsync(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, object? payload)
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