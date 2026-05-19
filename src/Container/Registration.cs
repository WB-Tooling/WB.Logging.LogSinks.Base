using System;

namespace WB.Logging.LogSinks.Base;

internal readonly record struct Registration(
    Func<Container, object>? Factory,
    Lazy<object>? Singleton,
    bool? DisposeWithContainer);