using System;
using AwesomeAssertions;
using WB.Logging.LogSinks.Base;

namespace ContainerTests.MethodTests.RegisterSingletonsMethodTests;

internal interface IService
{
}

internal sealed class Service : IService
{
}

internal sealed class ServiceFactory
{
    private int callCount = 0;

    public IService CreateService()
    {
        callCount++;
        return new Service();
    }

    internal int CallCount => callCount;
}

public sealed class TheRegisterSingletonsMethod
{
    [Test]
    public void ShouldRegisterServiceWithGenericParameters()
    {
        // arrange
        Container container = new();

        // act
        container.RegisterSingleton<IService, Service>();

        // assert
        IService instance = container.Resolve<IService>();
        instance.Should().BeOfType<Service>(because: "RegisterSingleton should register the implementation type for the service type");
    }

    [Test]
    public void ShouldReturnSameInstanceEachTimeGenericParametersAreUsed()
    {
        // arrange
        Container container = new();
        container.RegisterSingleton<IService, Service>();

        // act
        IService firstInstance = container.Resolve<IService>();
        IService secondInstance = container.Resolve<IService>();

        // assert
        firstInstance.Should().BeSameAs(secondInstance, because: "RegisterSingleton should return the same instance each time the service is resolved");
    }

    [Test]
    public void ShouldRegisterServiceWithTypeParameters()
    {
        // arrange
        Container container = new();

        // act
        container.RegisterSingleton(typeof(IService), typeof(Service));

        // assert
        object instance = container.Resolve(typeof(IService));
        instance.Should().BeOfType<Service>(because: "RegisterSingleton with type parameters should register the implementation type");
    }

    [Test]
    public void ShouldReturnSameInstanceEachTimeTypeParametersAreUsed()
    {
        // arrange
        Container container = new();
        container.RegisterSingleton(typeof(IService), typeof(Service));

        // act
        object firstInstance = container.Resolve(typeof(IService));
        object secondInstance = container.Resolve(typeof(IService));

        // assert
        firstInstance.Should().BeSameAs(secondInstance, because: "RegisterSingleton with type parameters should return the same instance each time");
    }

    [Test]
    public void ShouldRegisterServiceWithFactoryGeneric()
    {
        // arrange
        Container container = new();
        ServiceFactory factory = new();

        // act
        container.RegisterSingleton<IService>(c => factory.CreateService());

        // assert
        IService instance = container.Resolve<IService>();
        instance.Should().BeOfType<Service>(because: "RegisterSingleton with factory should create instance using the factory");
    }

    [Test]
    public void ShouldCallFactoryOnlyOnceForGenericFactory()
    {
        // arrange
        Container container = new();
        ServiceFactory factory = new();
        container.RegisterSingleton<IService>(c => factory.CreateService());

        // act
        container.Resolve<IService>();
        container.Resolve<IService>();

        // assert
        factory.CallCount.Should().Be(1, because: "RegisterSingleton with factory should call the factory only once to create the singleton instance");
    }

    [Test]
    public void ShouldRegisterServiceWithFactoryTypeParameters()
    {
        // arrange
        Container container = new();
        ServiceFactory factory = new();

        // act
        container.RegisterSingleton(typeof(IService), c => factory.CreateService()!);

        // assert
        object instance = container.Resolve(typeof(IService));
        instance.Should().BeOfType<Service>(because: "RegisterSingleton with type parameters and factory should create instance using the factory");
    }

    [Test]
    public void ShouldCallFactoryOnlyOnceForTypeParametersFactory()
    {
        // arrange
        Container container = new();
        ServiceFactory factory = new();
        container.RegisterSingleton(typeof(IService), c => factory.CreateService()!);

        // act
        container.Resolve(typeof(IService));
        container.Resolve(typeof(IService));

        // assert
        factory.CallCount.Should().Be(1, because: "RegisterSingleton with type parameters and factory should call the factory only once");
    }

    [Test]
    public void ShouldOverwritePreviousRegistration()
    {
        // arrange
        Container container = new();

        // act
        container.RegisterSingleton<IService, Service>();
        IService firstInstance = container.Resolve<IService>();
        
        container.RegisterSingleton<IService, Service>();
        IService secondInstance = container.Resolve<IService>();

        // assert
        firstInstance.Should().NotBeSameAs(secondInstance, because: "RegisterSingleton should create a new singleton when re-registering the service");
    }
}