using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace WB.Logging.LogSinks.Base;

internal delegate void Dispatcher(ILogMessage logMessage);

internal sealed class LogMessageWriterPipeline
{
    private readonly ConcurrentDictionary<Type, Dispatcher> dispatchers = new();

    private readonly ConcurrentDictionary<Type, object> logMessageWriters = new();

    public ILogMessageWriter<object>? DefaultLogMessageWriter { get; set; }

    public IDisposable RegisterWriter<TPayload>(ILogMessageWriter<TPayload> logMessageWriter)
        where TPayload : notnull
    {
        logMessageWriters[typeof(TPayload)] = logMessageWriter;

        return new ActionDisposable(() => logMessageWriters.TryRemove(typeof(TPayload), out _));
    }

    public void Write<TPayload>(ILogMessage<TPayload> message)
        where TPayload : notnull
    {
        Dispatcher dispatcher = dispatchers.GetOrAdd(
            typeof(TPayload),
            static (t, self) => self.CreateDispatcher(t),
            this);

        dispatcher(message);
    }

    private Dispatcher CreateDispatcher(Type payloadType)
    {
        if (logMessageWriters.TryGetValue(payloadType, out var writerObj))
        {
            return CreateTypedDispatcher(payloadType, writerObj);
        }

        if (DefaultLogMessageWriter is not null)
        {
            return logMessage => DefaultLogMessageWriter.Write((ILogMessage<object>)logMessage);
        }

        return _ => { };
    }

    private Dispatcher CreateTypedDispatcher(Type payloadType, object writerObj)
    {
        ParameterExpression logMessageParameter = Expression.Parameter(typeof(ILogMessage), "msg");

        // msg → ILogMessage<TPayload>
        UnaryExpression typedMsg = Expression.Convert(
            logMessageParameter,
            typeof(ILogMessage<>).MakeGenericType(payloadType));

        // writer → ILogMessageWriter<TPayload>
        UnaryExpression typedWriter = Expression.Convert(
            Expression.Constant(writerObj),
            typeof(ILogMessageWriter<>).MakeGenericType(payloadType));

        MethodCallExpression methodCall = Expression.Call(
            typedWriter,
            typeof(ILogMessageWriter<>)
                .MakeGenericType(payloadType)
                .GetMethod(nameof(ILogMessageWriter<>.Write))!,
            typedMsg);

        return Expression.Lambda<Dispatcher>(methodCall, logMessageParameter).Compile();
    }
}