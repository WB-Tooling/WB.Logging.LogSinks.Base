using System;

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
    /// Registers a transient service with its implementation type using generic parameters.
    /// A new instance will be created each time the service is resolved.
    /// </summary>
    /// <typeparam name="TService">The service type to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type that implements <typeparamref name="TService"/>.</typeparam>
    public void RegisterTransient<TService, TImplementation>()
        where TImplementation : TService
        => RegisterTransient(typeof(TService), typeof(TImplementation));

    /// <summary>
    /// Registers a transient service with its implementation type using type parameters.
    /// A new instance will be created each time the service is resolved.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="implementationType">The implementation type that implements <paramref name="serviceType"/>.</param>
    public void RegisterTransient(Type serviceType, Type implementationType);

    /// <summary>
    /// Registers a singleton service with its implementation type using generic parameters.
    /// A single instance will be created and reused for all resolutions.
    /// </summary>
    /// <typeparam name="TService">The service type to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type that implements <typeparamref name="TService"/>.</typeparam>
    /// <param name="disposeWithContainer">Indicates whether the instance should be disposed when the container is disposed.</param>
    public void RegisterSingleton<TService, TImplementation>(bool disposeWithContainer = true)
        where TImplementation : TService
        => RegisterSingleton(typeof(TService), typeof(TImplementation), disposeWithContainer);

    /// <summary>
    /// Registers a singleton service with its implementation type using type parameters.
    /// A single instance will be created and reused for all resolutions.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="implType">The implementation type that implements <paramref name="serviceType"/>.</param>
    /// <param name="disposeWithContainer">Indicates whether the instance should be disposed when the container is disposed.</param>
    public void RegisterSingleton(Type serviceType, Type implType, bool disposeWithContainer = true);

    /// <summary>
    /// Registers a pre-created instance as a singleton service using generic parameters.
    /// The provided instance will be returned for all resolutions of the service.
    /// </summary>
    /// <typeparam name="TService">The service type to register.</typeparam>
    /// <param name="instance">The instance to register as a singleton.</param>
    /// <param name="disposeWithContainer">Indicates whether the instance should be disposed when the container is disposed.</param>
    public void RegisterInstance<TService>(TService instance, bool disposeWithContainer = true)
        where TService : notnull
        => RegisterInstance(typeof(TService), instance!, disposeWithContainer);

    /// <summary>
    /// Registers a pre-created instance as a singleton service using type parameters.
    /// The provided instance will be returned for all resolutions of the service.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="instance">The instance to register as a singleton.</param>
    /// <param name="disposeWithContainer">Indicates whether the instance should be disposed when the container is disposed.</param>
    public void RegisterInstance(Type serviceType, object instance, bool disposeWithContainer = true);

    /// <summary>
    /// Resolves and returns an instance of the registered service using generic parameters.
    /// </summary>
    /// <typeparam name="TService">The service type to resolve.</typeparam>
    /// <returns>An instance of the service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service type is not registered.</exception>
    public TService Resolve<TService>()
        where TService : notnull;

    /// <summary>
    /// Resolves and returns an instance of the registered service using type parameters.
    /// </summary>
    /// <param name="serviceType">The service type to resolve.</param>
    /// <returns>An instance of the service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service type is not registered.</exception>
    public object Resolve(Type serviceType);

    /// <summary>
    /// Creates and returns a new instance of the implementation type without using the container's registrations.
    /// This bypasses the container's registry and always creates a new instance.
    /// </summary>
    /// <typeparam name="TService">The service type to instantiate.</typeparam>
    /// <returns>A new instance of the service type.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "The method name 'New' is chosen to clearly indicate that it creates a new instance, bypassing the container's registrations. This is a common convention in some DI containers for such functionality.")]
    public TService New<TService>()
        where TService : notnull
        => (TService)New(typeof(TService));

    /// <summary>
    /// Creates and returns a new instance of the implementation type without using the container's registrations.
    /// This bypasses the container's registry and always creates a new instance.
    /// </summary>
    /// <param name="implType">The implementation type to instantiate.</param>
    /// <returns>A new instance of the implementation type.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "The method name 'New' is chosen to clearly indicate that it creates a new instance, bypassing the container's registrations. This is a common convention in some DI containers for such functionality.")]
    public object New(Type implType);
}
