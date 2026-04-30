using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// A base implementation of <see cref="IAsyncLogSink"/> that manages log message writers for different payload types.
/// </summary>
/// <param name="defaultLogMessageWriter">The default <see cref="IAsyncLogMessageWriter{TAsyncLogSink, TPayload}"/> to use when no 
/// specific writer is registered for a payload type.</param>
public abstract class AsyncLogSinkBase<TLogSinkBase>(IAsyncLogMessageWriter<TLogSinkBase, object> defaultLogMessageWriter) : IAsyncLogSink
    where TLogSinkBase : AsyncLogSinkBase<TLogSinkBase>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly ConcurrentDictionary<Type, object> logMessageWriters = new();

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets the default <see cref="IAsyncLogMessageWriter{TAsyncLogSink, TPayload}"/> to use when no specific writer is registered for a payload type.
    /// </summary>
    public IAsyncLogMessageWriter<TLogSinkBase, object> DefaultLogMessageWriter => defaultLogMessageWriter;

    /// <summary>
    /// Gets the registered log message writers.
    /// </summary>
    /// <remarks>
    /// This is used for testing purposes to verify that log message writers are registered correctly.
    /// </remarks>
    public IReadOnlyList<object> LogMessageWriters => (IReadOnlyList<object>)logMessageWriters.Values;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public virtual async ValueTask SubmitAsync<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
    {
        ArgumentNullException.ThrowIfNull(logMessage, nameof(logMessage));

        if (TryGetLogMessageWriter(out IAsyncLogMessageWriter<TLogSinkBase, TPayload>? logMessageWriter))
        {
            await logMessageWriter.WriteAsync(logMessage.Timestamp, logMessage.LogLevel, logMessage.Senders, logMessage.Payload).ConfigureAwait(false);
        }
        else
        {
            await defaultLogMessageWriter.WriteAsync(logMessage.Timestamp, logMessage.LogLevel, logMessage.Senders, logMessage.Payload?.ToString() ?? "null").ConfigureAwait(false);
        }
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
    public IDisposable RegisterLogMessageWriter<TPayload>(IAsyncLogMessageWriter<TLogSinkBase, TPayload> logMessageWriter)
        where TPayload : notnull
    {
        ArgumentNullException.ThrowIfNull(logMessageWriter);

        logMessageWriter.LogSink = (TLogSinkBase)this;

        logMessageWriters[typeof(TPayload)] = logMessageWriter;

        return new DelegateDisposable(() => logMessageWriters.TryRemove(typeof(TPayload), out _));
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Methods                                                             │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private bool TryGetLogMessageWriter<TPayload>([NotNullWhen(true)] out IAsyncLogMessageWriter<TLogSinkBase, TPayload>? logMessageWriter)
        where TPayload : notnull
    {
        if (logMessageWriters.TryGetValue(typeof(TPayload), out var writer))
        {
            logMessageWriter = (IAsyncLogMessageWriter<TLogSinkBase, TPayload>)writer;

            return true;
        }
        else
        {
            logMessageWriter = default;

            return false;
        }
    }
}
