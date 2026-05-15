using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace WB.Logging.LogSinks.Base;

internal delegate void Dispatcher(ILogMessage logMessage);

internal sealed class LogMessageWriterPipeline : IDisposable
{
    private readonly ConcurrentDictionary<Type, Dispatcher> dispatchers = new();

    private readonly ConcurrentDictionary<Type, Func<object>> logMessageWriterFactories = new();

    private readonly Container container = new();

    public ILogMessageWriter<object>? DefaultLogMessageWriter { get; set; }

    public IContainer Container => container;

    public void Dispose()
    {
        container.Dispose();
    }

    public void RegisterWriter(Type logMessageWriterType, Type payloadType)
    {
        container.RegisterSingleton(logMessageWriterType, logMessageWriterType);

        logMessageWriterFactories[payloadType] = () => container.Resolve(logMessageWriterType);
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
        if (logMessageWriterFactories.TryGetValue(payloadType, out Func<object>? logMessageWriter))
        {
            return CreateTypedDispatcher(payloadType, logMessageWriter());
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