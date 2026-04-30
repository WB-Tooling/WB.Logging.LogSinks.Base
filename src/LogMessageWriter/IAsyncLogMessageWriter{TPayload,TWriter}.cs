using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// Defines an asynchronous log message writer for <see cref="ILogMessage{TPayload}"/> with a specific 
/// payload type <typeparamref name="TPayload"/>.
/// </summary>
/// <typeparam name="TAsyncLogSink">The type of the log sink that this log message writer belongs to.</typeparam>
/// <typeparam name="TPayload">The type of the payload of the log messages that this writer can write.</typeparam>
public interface IAsyncLogMessageWriter<TAsyncLogSink, TPayload>
    where TAsyncLogSink : IAsyncLogSink
    where TPayload : notnull
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets or sets the <see cref="IAsyncLogSink"/> that this log message writer belongs to.
    /// </summary>
    [NotNull]
    public TAsyncLogSink? LogSink { get; set; }

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
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous write operation.</returns>
    public ValueTask WriteAsync(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, TPayload payload);
}