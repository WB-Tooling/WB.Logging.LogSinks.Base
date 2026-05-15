using System;
using AwesomeAssertions;
using WB.Logging.LogSinks.Base;

namespace ContainerTests.MethodTests.RegisterTransientMethodTests;

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

public sealed class TheRegisterTransientMethod
{
    [Test]
    public void ShouldRegisterServiceWithGenericParameters()
    {
        // arrange
        Container container = new();

        // act
        container.RegisterTransient<IService, Service>();

        // assert
        IService instance = container.Resolve<IService>();
        instance.Should().BeOfType<Service>(because: "RegisterTransient should register the implementation type for the service type");
    }

    [Test]
    public void ShouldCreateNewInstanceEachTimeGenericParametersAreUsed()
    {
        // arrange
        Container container = new();
        container.RegisterTransient<IService, Service>();

        // act
        IService firstInstance = container.Resolve<IService>();
        IService secondInstance = container.Resolve<IService>();

        // assert
        firstInstance.Should().NotBeSameAs(secondInstance, because: "RegisterTransient should create a new instance each time the service is resolved");
    }

    [Test]
    public void ShouldRegisterServiceWithTypeParameters()
    {
        // arrange
        Container container = new();

        // act
        container.RegisterTransient(typeof(IService), typeof(Service));

        // assert
        object instance = container.Resolve(typeof(IService));
        instance.Should().BeOfType<Service>(because: "RegisterTransient with type parameters should register the implementation type");
    }

    [Test]
    public void ShouldCreateNewInstanceEachTimeTypeParametersAreUsed()
    {
        // arrange
        Container container = new();
        container.RegisterTransient(typeof(IService), typeof(Service));

        // act
        object firstInstance = container.Resolve(typeof(IService));
        object secondInstance = container.Resolve(typeof(IService));

        // assert
        firstInstance.Should().NotBeSameAs(secondInstance, because: "RegisterTransient with type parameters should create a new instance each time");
    }

    [Test]
    public void ShouldRegisterServiceWithFactoryGeneric()
    {
        // arrange
        Container container = new();
        ServiceFactory factory = new();

        // act
        container.RegisterTransient<IService>(c => factory.CreateService());

        // assert
        IService instance = container.Resolve<IService>();
        instance.Should().BeOfType<Service>(because: "RegisterTransient with factory should create instance using the factory");
    }

    [Test]
    public void ShouldCallFactoryEachTimeForGenericFactory()
    {
        // arrange
        Container container = new();
        ServiceFactory factory = new();
        container.RegisterTransient<IService>(c => factory.CreateService());

        // act
        container.Resolve<IService>();
        container.Resolve<IService>();

        // assert
        factory.CallCount.Should().Be(2, because: "RegisterTransient with factory should call the factory each time the service is resolved");
    }

    [Test]
    public void ShouldRegisterServiceWithFactoryTypeParameters()
    {
        // arrange
        Container container = new();
        ServiceFactory factory = new();

        // act
        container.RegisterTransient(typeof(IService), c => factory.CreateService()!);

        // assert
        object instance = container.Resolve(typeof(IService));
        instance.Should().BeOfType<Service>(because: "RegisterTransient with type parameters and factory should create instance using the factory");
    }

    [Test]
    public void ShouldCallFactoryEachTimeForTypeParametersFactory()
    {
        // arrange
        Container container = new();
        ServiceFactory factory = new();
        container.RegisterTransient(typeof(IService), c => factory.CreateService()!);

        // act
        container.Resolve(typeof(IService));
        container.Resolve(typeof(IService));

        // assert
        factory.CallCount.Should().Be(2, because: "RegisterTransient with type parameters and factory should call the factory each time");
    }

    [Test]
    public void ShouldOverwritePreviousRegistration()
    {
        // arrange
        Container container = new();

        // act
        container.RegisterTransient<IService, Service>();
        IService firstInstance = container.Resolve<IService>();
        
        container.RegisterTransient<IService, Service>();
        IService secondInstance = container.Resolve<IService>();

        // assert
        firstInstance.Should().NotBeSameAs(secondInstance, because: "RegisterTransient should overwrite previous registrations");
    }
}