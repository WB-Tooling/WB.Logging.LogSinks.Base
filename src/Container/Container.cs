using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// A dependency injection container that manages service registration and resolution.
/// Supports transient (new instance per resolution), singleton (single shared instance),
/// and instance (pre-created object) registration patterns.
/// </summary>
public sealed class Container : IContainer, IDisposable, IAsyncDisposable
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Stores the mapping of service types to their implementation types.
    /// This enables reflection-based factory compilation and testing visibility.
    /// </summary>
    private readonly ConcurrentDictionary<Type, Type> registrations = new();

    /// <summary>
    /// Stores factory delegates for transient service registrations.
    /// Each resolution will invoke the factory to create a new instance.
    /// </summary>
    private readonly ConcurrentDictionary<Type, Func<Container, object>> transients = new();

    /// <summary>
    /// Stores lazy-initialized singleton instances for singleton service registrations.
    /// The instance is created once and reused for all subsequent resolutions.
    /// </summary>
    private readonly ConcurrentDictionary<Type, Lazy<object>> singletons = new();

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Internal Properties                                                         │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets a read-only view of all registered service-to-implementation type mappings.
    /// </summary>
    internal ReadOnlyDictionary<Type, Type> Registrations => new(registrations);

    /// <summary>
    /// Gets a read-only view of all resolved singleton instances.
    /// </summary>
    internal ReadOnlyDictionary<Type, object> Singletons => new(singletons.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value));

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (Lazy<object> lazy in singletons.Values)
        {
            switch (lazy.Value)
            {
                case IAsyncDisposable asyncDisposable:
                    asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        foreach (Lazy<object> lazy in singletons.Values)
        {
            switch (lazy.Value)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }

    /// <inheritdoc/>
    public void RegisterTransient<TService, TImplementation>()
        where TImplementation : TService
        => transients[typeof(TService)] = CompileFactory(typeof(TImplementation));

    /// <inheritdoc/>
    public void RegisterTransient(Type serviceType, Type implType)
        => transients[serviceType] = CompileFactory(implType ?? throw new ArgumentNullException(nameof(implType)));

    /// <inheritdoc/>
    public void RegisterTransient<TService>(Func<Container, TService> factory)
        where TService : notnull
        => RegisterTransient(typeof(TService), c => factory(c)!);

    /// <inheritdoc/>
    public void RegisterTransient(Type serviceType, Func<Container, object> factory)
        => transients[serviceType] = c => factory(c)!;

    /// <inheritdoc/>
    public void RegisterSingleton<TService, TImplementation>()
        where TImplementation : TService
        => RegisterSingleton(typeof(TService), typeof(TImplementation));

    /// <inheritdoc/>
    public void RegisterSingleton(Type serviceType, Type implType)
        => singletons[serviceType] = new Lazy<object>(() => CompileFactory(implType ?? throw new ArgumentNullException(nameof(implType)))(this));

    /// <inheritdoc/>
    public void RegisterSingleton<TService>(Func<Container, TService> factory)
        where TService : notnull
        => RegisterSingleton(typeof(TService), c => factory(c)!);

    /// <inheritdoc/>
    public void RegisterSingleton(Type serviceType, Func<Container, object> factory)
        => singletons[serviceType] = new Lazy<object>(() => factory(this)!);

    /// <inheritdoc/>
    public void RegisterInstance<TService>(TService instance)
        where TService : notnull
        => RegisterInstance(typeof(TService), instance);

    /// <inheritdoc/>
    public void RegisterInstance(Type serviceType, object instance)
    {
        ArgumentNullException.ThrowIfNull(instance, nameof(instance));
        registrations[serviceType] = instance.GetType();
        singletons[serviceType] = new Lazy<object>(() => instance);
    }

    /// <inheritdoc/>
    public TService Resolve<TService>()
        where TService : notnull
        => (TService)Resolve(typeof(TService));

    /// <inheritdoc/>
    public object Resolve(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));

        if (singletons.TryGetValue(serviceType, out var lazy))
        {
            return lazy.Value;
        }

        if (transients.TryGetValue(serviceType, out var factory))
        {
            return factory(this);
        }

        throw new InvalidOperationException($"Service not registered: {serviceType.FullName}");
    }

    /// <inheritdoc/>
    public TService New<TService>()
        where TService : notnull
        => (TService)New(typeof(TService));

    /// <inheritdoc/>
    public object New(Type implType)
    {
        ArgumentNullException.ThrowIfNull(implType, nameof(implType));

        return CompileFactory(implType)(this);
    }

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Methods                                                             │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private static Func<Container, object> CompileFactory(Type implType)
    {
        ConstructorInfo constructorInfo = implType
            .GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault()
            ?? throw new InvalidOperationException($"No public constructor found for {implType.FullName}");

        ParameterExpression containerParam = Expression.Parameter(typeof(Container), "c");

        Expression[] args = [.. constructorInfo.GetParameters()
            .Select(p =>
                Expression.Convert(
                    Expression.Call(containerParam, nameof(Resolve), new[] { p.ParameterType }),
                    p.ParameterType))];

        NewExpression newExpression = Expression.New(constructorInfo, args);

        return Expression.Lambda<Func<Container, object>>(newExpression, containerParam).Compile();
    }
}
