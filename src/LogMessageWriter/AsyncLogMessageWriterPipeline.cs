using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

internal delegate ValueTask AsyncDispatcher(ILogMessage logMessage, CancellationToken cancellationToken);

internal sealed class AsyncLogMessageWriterPipeline : IAsyncDisposable  
{
    private readonly ConcurrentDictionary<Type, AsyncDispatcher> dispatchers = new();

    private readonly ConcurrentDictionary<Type, Func<object>> logMessageWriterFactories = new();

    private readonly Container container = new();

    public IAsyncLogMessageWriter<object>? DefaultLogMessageWriter { get; set; }

    public IContainer Container => container;

    public async ValueTask DisposeAsync()
    {
        await container.DisposeAsync().ConfigureAwait(false);
    }

    public void RegisterWriter(Type logMessageWriterType, Type payloadType)
    {
        container.RegisterSingleton(logMessageWriterType, logMessageWriterType);
    
        logMessageWriterFactories[payloadType] = () => container.Resolve(logMessageWriterType); 
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
        if (logMessageWriterFactories.TryGetValue(payloadType, out Func<object>? logMessageWriter))
        {
            return CreateTypedDispatcher(payloadType, logMessageWriter());
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
        UnaryExpression typedLogMessage = Expression.Convert(
            logMessageParameter,
            typeof(ILogMessage<>).MakeGenericType(payloadType));

        // writer → ILogMessageWriter<TPayload>
        UnaryExpression typedWriter = Expression.Convert(
            Expression.Constant(writerObj),
            typeof(IAsyncLogMessageWriter<>).MakeGenericType(payloadType));

        MethodCallExpression methodCall = Expression.Call(
            typedWriter,
            typeof(IAsyncLogMessageWriter<>)
                .MakeGenericType(payloadType)
                .GetMethod(nameof(IAsyncLogMessageWriter<>.WriteAsync))!,
            typedLogMessage,
            cancellationTokenParameter);

        return Expression.Lambda<AsyncDispatcher>(methodCall, logMessageParameter, cancellationTokenParameter).Compile();
    }
}