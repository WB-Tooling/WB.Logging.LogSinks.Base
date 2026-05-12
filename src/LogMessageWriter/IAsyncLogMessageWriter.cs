using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// Defines an asynchronous log message writer for <see cref="ILogMessage{TPayload}"/>s.
/// </summary>
/// <typeparam name="TPayload">The type of the payload that this log message writer can write.</typeparam>
public interface IAsyncLogMessageWriter<TPayload>
    where TPayload : notnull
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets or sets the <see cref="IAsyncLogSink"/> that this log message writer belongs to.
    /// </summary>
    [NotNull]
    public IAsyncLogSink? LogSink { get; set; }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Writes the <paramref name="logMessage"/>.
    /// </summary>
    /// <param name="logMessage">The log message to write.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public ValueTask WriteAsync(ILogMessage<TPayload> logMessage, CancellationToken cancellationToken);
}
