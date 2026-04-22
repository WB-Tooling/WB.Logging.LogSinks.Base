using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// A base implementation of <see cref="IAsyncLogSink"/> that manages log message writers for different payload types.
/// </summary>
/// <param name="defaultLogMessageWriter">The default <see cref="IAsyncLogMessageWriter{TPayload, TWriter}"/> to use when no 
/// specific writer is registered for a payload type.</param>
/// <param name="writer">The initial writer of type <typeparamref name="TWriter"/> that the log message writers will use to write log messages.</param>
public abstract class AsyncLogSinkBase<TWriter>(IAsyncLogMessageWriter<object, TWriter> defaultLogMessageWriter, TWriter writer) : IAsyncLogSink
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly ConcurrentDictionary<Type, object> logMessageWriters = new();
    
    private int isDisabled;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets the default <see cref="IAsyncLogMessageWriter{TPayload, TWriter}"/> to use when no specific writer is registered for a payload type.
    /// </summary>
    public IAsyncLogMessageWriter<object, TWriter> DefaultLogMessageWriter => defaultLogMessageWriter;

    /// <summary>
    /// Gets the registered log message writers.
    /// </summary>
    /// <remarks>
    /// This is used for testing purposes to verify that log message writers are registered correctly.
    /// </remarks>
    public IReadOnlyList<object> LogMessageWriters => (IReadOnlyList<object>)logMessageWriters.Values;

    /// <summary>
    /// Gets or sets the writer of type <typeparamref name="TWriter"/> that this log message writer uses to write log messages.
    /// </summary>
    /// <remarks>
    /// When setting the writer, it will update the writer of all registered log message writers that 
    /// implement <see cref="IAsyncLogMessageWriter{TPayload, TWriter}"/>.
    /// </remarks>
    public TWriter Writer
    {
        get;
        set
        {
            field = value;

            object[] logMessageWriters = [DefaultLogMessageWriter, .. this.logMessageWriters.Values];

            foreach (object writer in logMessageWriters)
            {
                if (writer is IHasWriter<TWriter> asyncLogMessageWriter)
                {
                    asyncLogMessageWriter.Writer = value;
                }
            }
        }
    } = writer;

    /// <summary>
    /// Gets or sets a value indicating whether this sink is disabled.
    /// </summary>
    /// <remarks>
    /// The value is stored atomically for thread-safe reads and writes.
    /// </remarks>
    public bool IsDisabled
    {
        get => Volatile.Read(ref isDisabled) == 1;
        private set => Interlocked.Exchange(ref isDisabled, value ? 1 : 0);
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public async ValueTask SubmitAsync<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
    {
        if (IsDisabled)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(logMessage, nameof(logMessage));

        if (TryGetLogMessageWriter(out IAsyncLogMessageWriter<TPayload, TWriter>? logMessageWriter))
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
    public IDisposable RegisterLogMessageWriter<TPayload>(IAsyncLogMessageWriter<TPayload, TWriter> logMessageWriter)
        where TPayload : notnull
    {
        ArgumentNullException.ThrowIfNull(logMessageWriter);

        logMessageWriter.LogSink = this;

        logMessageWriters[typeof(TPayload)] = logMessageWriter;

        return new DelegateDisposable(() => logMessageWriters.TryRemove(typeof(TPayload), out _));
    }

    /// <inheritdoc/>
    public IDisposable Disable()
    {
        if (IsDisabled)
        {
            throw new InvalidOperationException("The log sink is already disabled.");
        }

        IsDisabled = true;

        return new DelegateDisposable(() => IsDisabled = false);
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Methods                                                             │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private bool TryGetLogMessageWriter<TPayload>([NotNullWhen(true)] out IAsyncLogMessageWriter<TPayload, TWriter>? logMessageWriter)
        where TPayload : notnull
    {
        if (logMessageWriters.TryGetValue(typeof(TPayload), out var writer))
        {
            logMessageWriter = (IAsyncLogMessageWriter<TPayload, TWriter>)writer;

            return true;
        }
        else
        {
            logMessageWriter = default;

            return false;
        }
    }
}
