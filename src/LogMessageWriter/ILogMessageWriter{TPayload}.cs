using System;
using System.Collections.Generic;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// Defines a log message writer for <see cref="ILogMessage{TPayload}"/> with a specific 
/// payload type <typeparamref name="TPayload"/>.
/// </summary>
/// <typeparam name="TPayload">The type of the payload of the log messages that this writer can write.</typeparam>
/// <typeparam name="TWriter">The type of the writer that this log message writer uses to write log messages.</typeparam>
public interface ILogMessageWriter<TPayload, TWriter> : IHasWriter<TWriter>
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets or sets the <see cref="IAsyncLogSink"/> that this log message writer belongs to.
    /// </summary>
    public ILogSink? LogSink { get; set; }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Write a log message with the specified <paramref name="timestamp"/>, <paramref name="logLevel"/>,
    /// <paramref name="senders"/> and <paramref name="payload"/>.
    /// </summary>
    /// <param name="timestamp">The timestamp of the log message.</param>
    /// <param name="logLevel">The <see cref="LogLevel"/> of the log message.</param>
    /// <param name="senders">The senders of the log message.</param>
    /// <param name="payload">The payload of the log message.</param>
    public void Write(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, TPayload? payload);
}