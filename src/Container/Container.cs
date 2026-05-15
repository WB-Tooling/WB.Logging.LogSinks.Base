using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WB.Logging.LogSinks.Base;

internal sealed class Container : IContainer, IDisposable, IAsyncDisposable
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Private Fields                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘
    private readonly ConcurrentDictionary<Type, Type> registrations = new();

    private readonly ConcurrentDictionary<Type, object> singletons = new();

    private readonly ConcurrentDictionary<Type, Lazy<object>> factories = new();

    internal ReadOnlyDictionary<Type, Type> Registrations => new(registrations);

    internal ReadOnlyDictionary<Type, object> Singletons => new(singletons);

    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    public void Dispose()
    {
        foreach (object singleton in singletons.Values)
        {
            if (singleton is IAsyncDisposable asyncDisposable)
            {
                asyncDisposable.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else if (singleton is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        registrations.Clear();
        singletons.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (object singleton in singletons.Values)
        {
            if (singleton is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else if (singleton is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        registrations.Clear();
        singletons.Clear();
    }

    public void Register<TService, TImplementation>()
        where TImplementation : TService
        => registrations[typeof(TService)] = typeof(TImplementation);

    public void RegisterSingleton<TService, TImplementation>()
        where TImplementation : TService
        => RegisterSingleton(typeof(TService), typeof(TImplementation));

    public void RegisterSingleton(Type serviceType, Type implementationType)
    {
        registrations[serviceType] = implementationType;

        singletons[serviceType] = Resolve(serviceType);
    }

    public void RegisterFactory<TService>(Func<TService?> factory)
        => RegisterFactory(typeof(TService), () => factory()!);

    public void RegisterFactory(Type serviceType, Func<object> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        registrations[serviceType] = serviceType;
        factories[serviceType] = new Lazy<object>(factory);
    }

    public void RegisterInstance<TService>(TService instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        Type serviceType = typeof(TService);

        registrations[serviceType] = instance.GetType();

        singletons[serviceType] = instance!;
    }

    public TService Resolve<TService>()
        => (TService)Resolve(typeof(TService));

    public object Resolve(Type serviceType)
    {
        if (singletons.TryGetValue(serviceType, out var singleton))
        {
            return singleton;
        }

        if (factories.TryGetValue(serviceType, out var factory))
        {
            return factory.Value;
        }

        Type implementationType = registrations[serviceType];

        ConstructorInfo? constructorInfo = implementationType
            .GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault() ?? throw new InvalidOperationException($"No public constructors found for type {implementationType.FullName}.");

        object[]? args = [.. constructorInfo.GetParameters().Select(p => Resolve(p.ParameterType))];

        return Activator.CreateInstance(implementationType, args)!;
    }

    public object New(Type serviceType)
    {
        ConstructorInfo? constructorInfo = serviceType
            .GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault() ?? throw new InvalidOperationException($"No public constructors found for type {serviceType.FullName}.");

        object[]? args = [.. constructorInfo.GetParameters().Select(p => Resolve(p.ParameterType))];

        return Activator.CreateInstance(serviceType, args)!;
    }
}
