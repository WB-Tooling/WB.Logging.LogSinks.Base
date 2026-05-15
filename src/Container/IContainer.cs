using System;
using System.Collections.Generic;

namespace WB.Logging.LogSinks.Base;

/// <summary>
/// A container for registering and resolving services and their implementations.
/// </summary>
public interface IContainer
{
    // ┌─────────────────────────────────────────────────────────────────────────────┐
    // │ Public Methods                                                              │
    // └─────────────────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Registers the <typeparamref name="TService"/> with the specified <typeparamref name="TImplementation"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to register for the service.</typeparam>
    public void Register<TService, TImplementation>()
        where TImplementation : TService;

    /// <summary>
    /// Registers the <typeparamref name="TService"/> with the specified <typeparamref name="TImplementation"/> as a singleton.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to register for the service.</typeparam>
    public void RegisterSingleton<TService, TImplementation>()
        where TImplementation : TService;

    /// <summary>
    /// Registers the <paramref name="instance"/> as the implementation for the <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="instance">The instance to register as the implementation for the service.</param>
    public void RegisterInstance<TService>(TService instance);

    /// <summary>
    /// Registers the <see cref="Func{TResult}"/> method for creating instances of the <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service to register.</typeparam>
    /// <param name="factory">The factory method to create instances of the service.</param>
    public void RegisterFactory<TService>(Func<TService> factory);

    /// <summary>
    /// Resolves an instance of the specified <typeparamref name="TService"/> type.
    /// </summary>
    /// <typeparam name="TService">The type of the service to resolve.</typeparam>
    /// <returns>An instance of the specified service type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified service type is not registered.</exception>
    public TService Resolve<TService>();
}
