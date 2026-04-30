using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// Defines a log message writer for <see cref="ILogMessage{TPayload}"/> with a specific 
/// payload type <typeparamref name="TPayload"/>.
/// </summary>
/// <typeparam name="TLogSink">The type of the log sink that this log message writer belongs to.</typeparam>
/// <typeparam name="TPayload">The type of the payload of the log messages that this writer can write.</typeparam>
public interface ILogMessageWriter<TLogSink, TPayload>
    where TLogSink : ILogSink
    where TPayload : notnull
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Properties                                                           │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets or sets the <see cref="IAsyncLogSink"/> that this log message writer belongs to.
    /// </summary>
    [NotNull]
    public TLogSink? LogSink { get; set; }

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
    public void Write(DateTimeOffset timestamp, LogLevel? logLevel, IEnumerable<string> senders, TPayload payload);
}