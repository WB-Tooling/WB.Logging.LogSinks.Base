using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

internal delegate ValueTask AsyncDispatcher(ILogMessage logMessage, CancellationToken cancellationToken);

internal sealed class AsyncLogMessageWriterPipeline
{
    private readonly ConcurrentDictionary<Type, AsyncDispatcher> dispatchers = new();

    private readonly ConcurrentDictionary<Type, object> logMessageWriters = new();

    public IAsyncLogMessageWriter<object>? DefaultLogMessageWriter { get; set; }

    public IDisposable RegisterWriter<TPayload>(IAsyncLogMessageWriter<TPayload> logMessageWriter)
        where TPayload : notnull
    {
        logMessageWriters[typeof(TPayload)] = logMessageWriter;

        return new ActionDisposable(() => logMessageWriters.TryRemove(typeof(TPayload), out _));
    }

    public ValueTask WriteAsync<TPayload>(ILogMessage<TPayload> message, CancellationToken cancellationToken)
        where TPayload : notnull
    {
        AsyncDispatcher dispatcher = dispatchers.GetOrAdd(
            typeof(TPayload),
            static (t, self) => self.CreateDispatcher(t),
            this);

        return dispatcher(message, cancellationToken);
    }

    private AsyncDispatcher CreateDispatcher(Type payloadType)
    {
        if (logMessageWriters.TryGetValue(payloadType, out var writerObj))
        {
            return CreateTypedDispatcher(payloadType, writerObj);
        }

        if (DefaultLogMessageWriter is not null)
        {
            return (logMessage, cancellationToken) => DefaultLogMessageWriter.WriteAsync((ILogMessage<object>)logMessage, cancellationToken);
        }

        return (_, _) => ValueTask.CompletedTask;
    }

    private AsyncDispatcher CreateTypedDispatcher(Type payloadType, object writerObj)
    {
        ParameterExpression logMessageParameter = Expression.Parameter(typeof(ILogMessage), "logMessage");
        ParameterExpression cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

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
            typedMsg,
            cancellationTokenParameter);

        return Expression.Lambda<AsyncDispatcher>(methodCall, logMessageParameter, cancellationTokenParameter).Compile();
    }
}