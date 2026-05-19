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
        IContainer container = new Container();

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
        IContainer container = new Container();
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
        IContainer container = new Container();

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
    public void ShouldOverwritePreviousRegistration()
    {
        // arrange
        IContainer container = new Container();

        // act
        container.RegisterSingleton<IService, Service>();
        IService firstInstance = container.Resolve<IService>();
        
        container.RegisterSingleton<IService, Service>();
        IService secondInstance = container.Resolve<IService>();

        // assert
        firstInstance.Should().NotBeSameAs(secondInstance, because: "RegisterSingleton should create a new singleton when re-registering the service");
    }
}