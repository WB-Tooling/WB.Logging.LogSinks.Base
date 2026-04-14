using System;
using System.Collections.Generic;
using WB.Logging;

namespace AsyncLogSinkBaseTests;

internal sealed class LogMessage<TPayload> : ILogMessage<TPayload>
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.MinValue;

    public IReadOnlyList<string> Senders { get; set; } = [];

    public LogLevel? LogLevel { get; set; }

    public TPayload? Payload { get; set; }
}