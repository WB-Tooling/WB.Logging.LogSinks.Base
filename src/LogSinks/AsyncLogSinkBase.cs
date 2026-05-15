using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// A base implementation of <see cref="IAsyncLogSink"/> that manages log message writers for different payload types.
/// </summary>
public abstract class AsyncLogSinkBase<TLogSinkBase> : IAsyncLogSink, IAsyncDisposable
    where TLogSinkBase : AsyncLogSinkBase<TLogSinkBase>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly AsyncLogMessageWriterPipeline logMessageWriterPipeline;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Protected Constructors                                                      │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLogSinkBase{TLogSinkBase}"/> class with the specified default log message writer.
    /// </summary>
    /// <param name="defaultLogMessageWriter">The default log message writer to use for payload types that do not have a specific log message writer registered.</param>
    protected AsyncLogSinkBase(IAsyncLogMessageWriter<object> defaultLogMessageWriter)
    {
        logMessageWriterPipeline = new()
        {
            DefaultLogMessageWriter = defaultLogMessageWriter
        };

        logMessageWriterPipeline.Container.RegisterInstance<IAsyncLogSink>(this);
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets the <see cref="IContainer"/> associated with this log sink.
    /// </summary>
    public IContainer ServiceContainer => logMessageWriterPipeline.Container;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public virtual ValueTask SubmitAsync<TPayload>(ILogMessage<TPayload> logMessage, CancellationToken cancellationToken)
        where TPayload : notnull
    {
        ArgumentNullException.ThrowIfNull(logMessage, nameof(logMessage));

        return logMessageWriterPipeline.WriteAsync(logMessage, cancellationToken);
    }

    /// <summary>
    /// Registers a log message writer of type <typeparamref name="TAsyncLogMessageWriter"/> with this log sink.
    /// </summary>
    /// <typeparam name="TAsyncLogMessageWriter">The <see cref="Type"/> of the log message writer to register.</typeparam>
    public virtual void RegisterLogMessageWriter<TAsyncLogMessageWriter>()
    {
        Type logMessageWriterType = typeof(TAsyncLogMessageWriter);

        if (!typeof(IAsyncLogMessageWriter<>).MakeGenericType(logMessageWriterType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncLogMessageWriter<>))
            .GetGenericArguments()[0]).IsAssignableFrom(logMessageWriterType))
        {
            throw new ArgumentException($"The log message writer type must implement IAsyncLogMessageWriter<TPayload> for some payload type.", nameof(TAsyncLogMessageWriter));
        }

        Type payloadType = logMessageWriterType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncLogMessageWriter<>))
            .GetGenericArguments()[0];

        logMessageWriterPipeline.RegisterWriter(logMessageWriterType, payloadType);
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Protected Methods                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    protected async virtual ValueTask DisposeAsyncCore()
        => await logMessageWriterPipeline.DisposeAsync().ConfigureAwait(false);
}
