using System.Diagnostics.CodeAnalysis;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// Defines a log message writer for <see cref="ILogMessage{TPayload}"/> with a specific 
/// </summary>
public interface ILogMessageWriter<TPayload>
    where TPayload : notnull
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Writes the <paramref name="logMessage"/>.
    /// </summary>
    /// <param name="logMessage">The log message to write.</param>
    public void Write(ILogMessage<TPayload> logMessage);
}