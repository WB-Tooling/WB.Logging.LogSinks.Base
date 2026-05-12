using System;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// A base implementation of <see cref="ILogSink"/> that manages log message writers for different payload types.
/// </summary>
public abstract class LogSinkBase<TLogSinkBase>(ILogMessageWriter<object> defaultLogMessageWriter) : ILogSink
    where TLogSinkBase : LogSinkBase<TLogSinkBase>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly LogMessageWriterPipeline logMessageWriterPipeline = new()
    {
        DefaultLogMessageWriter = defaultLogMessageWriter
    };

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public virtual void Submit<TPayload>(ILogMessage<TPayload> logMessage) 
        where TPayload : notnull
    {
        ArgumentNullException.ThrowIfNull(logMessage, nameof(logMessage));

        logMessageWriterPipeline.Write(logMessage);
    }

    /// <summary>
    /// Registers the <paramref name="logMessageWriter"/> for the payload type 
    /// <typeparamref name="TPayload"/>. If a writer is already registered for the 
    /// payload type, it will be replaced.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload.</typeparam>
    /// <param name="logMessageWriter">The log message writer to register.</param>
    /// <returns>A <see cref="IDisposable"/> that, when disposed, unregisters the log message writer.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logMessageWriter"/> is <c>null</c>.</exception>
    public IDisposable RegisterLogMessageWriter<TPayload>(ILogMessageWriter<TPayload> logMessageWriter)
        where TPayload : notnull
        => logMessageWriterPipeline.RegisterWriter(logMessageWriter);
}
