using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// A base implementation of <see cref="IAsyncLogSink"/> that manages log message writers for different payload types.
/// </summary>
public abstract class AsyncLogSinkBase<TLogSinkBase>(IAsyncLogMessageWriter<object> defaultLogMessageWriter) : IAsyncLogSink
    where TLogSinkBase : AsyncLogSinkBase<TLogSinkBase>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly AsyncLogMessageWriterPipeline logMessageWriterPipeline = new()
    {
        DefaultLogMessageWriter = defaultLogMessageWriter
    };

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public virtual ValueTask SubmitAsync<TPayload>(ILogMessage<TPayload> logMessage, CancellationToken cancellationToken)
        where TPayload : notnull
    {
        ArgumentNullException.ThrowIfNull(logMessage, nameof(logMessage));

        return logMessageWriterPipeline.WriteAsync(logMessage, cancellationToken);
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
    public virtual IDisposable RegisterLogMessageWriter<TPayload>(IAsyncLogMessageWriter<TPayload> logMessageWriter)
        where TPayload : notnull
        => logMessageWriterPipeline.RegisterWriter(logMessageWriter);
}
