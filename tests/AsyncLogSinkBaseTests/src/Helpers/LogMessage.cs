using System;
using System.Collections.Generic;
using WB.Logging;

namespace AsyncLogSinkBaseTests;

internal sealed class LogMessage<TPayload> : ILogMessage<TPayload>
    where TPayload : notnull
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.MinValue;

    public IReadOnlyList<string> Senders { get; set; } = [];

    public LogLevel? LogLevel { get; set; }

    public required TPayload Payload { get; init; }
}