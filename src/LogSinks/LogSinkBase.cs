using System;
using System.Linq;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// A base implementation of <see cref="ILogSink"/> that manages log message writers for different payload types.
/// </summary>
public abstract class LogSinkBase<TLogSinkBase> : ILogSink, IDisposable
    where TLogSinkBase : LogSinkBase<TLogSinkBase>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly LogMessageWriterPipeline logMessageWriterPipeline;

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Protected Constructors                                                      │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSinkBase{TLogSinkBase}"/> class with the specified default log message writer.
    /// </summary>
    /// <param name="defaultLogMessageWriter">The default log message writer to use for payload types that do not have a specific log message writer registered.</param>
    protected LogSinkBase(ILogMessageWriter<object> defaultLogMessageWriter)
    {
        logMessageWriterPipeline = new()
        {
            DefaultLogMessageWriter = defaultLogMessageWriter
        };

        logMessageWriterPipeline.Container.RegisterInstance<ILogSink>(this);
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
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public virtual void Submit<TPayload>(ILogMessage<TPayload> logMessage)
        where TPayload : notnull
    {
        ArgumentNullException.ThrowIfNull(logMessage, nameof(logMessage));

        logMessageWriterPipeline.Write(logMessage);
    }

    /// <summary>
    /// Registers a log message writer of type <typeparamref name="TLogMessageWriter"/> with this log sink.
    /// </summary>
    /// <typeparam name="TLogMessageWriter">The <see cref="Type"/> of the log message writer to register.</typeparam>
    public virtual void RegisterLogMessageWriter<TLogMessageWriter>()
    {
        Type logMessageWriterType = typeof(TLogMessageWriter);

        if (!typeof(ILogMessageWriter<>).MakeGenericType(logMessageWriterType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ILogMessageWriter<>))
            .GetGenericArguments()[0]).IsAssignableFrom(logMessageWriterType))
        {
            throw new ArgumentException($"The log message writer type must implement ILogMessageWriter<TPayload> for some payload type.", nameof(TLogMessageWriter));
        }

        Type payloadType = logMessageWriterType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ILogMessageWriter<>))
            .GetGenericArguments()[0];

        logMessageWriterPipeline.RegisterWriter(logMessageWriterType, payloadType);
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Protected Methods                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            logMessageWriterPipeline.Dispose();
        }
    }
}
