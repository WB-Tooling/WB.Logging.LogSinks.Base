using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// Defines an asynchronous log message writer for <see cref="ILogMessage{TPayload}"/> with a specific 
/// payload type <typeparamref name="TPayload"/>.
/// </summary>
/// <typeparam name="TPayload">The type of the payload of the log messages that this writer can write.</typeparam>
public interface IAsyncLogMessageWriter<TPayload>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Write a log message with the specified <paramref name="timestamp"/>, <paramref name="logLevel"/>, 
    /// <paramref name="senders"/> and <paramref name="payload"/> asynchronously.
    /// </summary>
    /// <param name="timestamp">The timestamp of the log message.</param>
    /// <param name="logLevel">The <see cref="LogLevel"/> of the log message.</param>
    /// <param name="senders">The senders of the log message.</param>
    /// <param name="payload">The payload of the log message.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
    public Task WriteAsync(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, TPayload? payload);
}