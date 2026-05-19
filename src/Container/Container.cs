using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private readonly ConcurrentDictionary<Type, Registration> registrations = new();


    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Internal Properties                                                         │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Gets a read-only view of all registered services and their corresponding registrations.
    /// </summary>
    internal ReadOnlyDictionary<Type, Registration> Registrations => registrations.AsReadOnly();

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (Registration registration in registrations.Values.Where(r => r.DisposeWithContainer.HasValue && r.DisposeWithContainer.Value))
        {
            if (registration.Singleton?.IsValueCreated == true)
            {
                switch (registration.Singleton.Value)
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
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        foreach (Registration registration in registrations.Values.Where(r => r.DisposeWithContainer.HasValue && r.DisposeWithContainer.Value))
        {
            if (registration.Singleton?.IsValueCreated == true)
            {
                switch (registration.Singleton.Value)
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
    }

    /// <inheritdoc/>
    public void RegisterTransient(Type serviceType, Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));
        ArgumentNullException.ThrowIfNull(implementationType, nameof(implementationType));

        registrations[serviceType] = new Registration(CompileFactory(implementationType), null, null);
    }

    /// <inheritdoc/>
    public void RegisterSingleton(Type serviceType, Type implType, bool disposeWithContainer = true)
    {
        ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));
        ArgumentNullException.ThrowIfNull(implType, nameof(implType));

        registrations[serviceType] = new Registration(null, new Lazy<object>(() => CompileFactory(implType)(this)), disposeWithContainer);
    }

    /// <inheritdoc/>
    public void RegisterInstance(Type serviceType, object instance, bool disposeWithContainer = true)
    {
        ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));
        ArgumentNullException.ThrowIfNull(instance, nameof(instance));

        registrations[serviceType] = new Registration(null, new Lazy<object>(() => instance), disposeWithContainer);
    }

    /// <inheritdoc/>
    public TService Resolve<TService>()
        where TService : notnull
        => (TService)Resolve(typeof(TService));

    /// <inheritdoc/>
    public object Resolve(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));

        if (registrations.TryGetValue(serviceType, out Registration registration))
        {
            if (registration.Singleton is not null)
            {
                return registration.Singleton.Value;
            }
            else if (registration.Factory is not null)
            {
                return registration.Factory(this);
            }
            else
            {
                throw new InvalidOperationException($"No factory or singleton instance found for service type: {serviceType.FullName}");
            }
        }

        throw new InvalidOperationException($"Service not registered: {serviceType.FullName}");
    }

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
