using System;

namespace WB.Logging.LogSinks.Base;

internal sealed class ActionDisposable(Action disposeAction) : IDisposable
{
    private readonly Action disposeAction = disposeAction;

    public void Dispose()
        => disposeAction();
}