using System;
using System.Collections.Generic;
using AwesomeAssertions;
using WB.Logging.LogSinks.Base;

namespace ContainerTests.MethodTests.ResolveMethodTests;

internal interface IService
{
}

internal sealed class Service : IService
{
}

internal interface IDependency
{
}

internal sealed class Dependency : IDependency
{
}

internal sealed class ServiceWithDependency : IService
{
    internal IDependency Dependency { get; }

    public ServiceWithDependency(IDependency dependency)
    {
        Dependency = dependency;
    }
}

internal sealed class ServiceWithoutPublicConstructor : IService
{
    private ServiceWithoutPublicConstructor()
    {
    }
}

internal sealed class ServiceWithMultipleConstructors : IService
{
    internal string ConstructorCalled { get; }

    public ServiceWithMultipleConstructors()
    {
        ConstructorCalled = "parameterless";
    }

    public ServiceWithMultipleConstructors(IDependency dependency)
    {
        ConstructorCalled = "with-dependency";
    }
}

public sealed class TheResolveMethod
{
    [Test]
    public void ShouldResolveTheRegisteredSingletonInstanceForTheServiceType()
    {
        // arrange
        Container container = new();
        container.RegisterSingleton<IService, Service>();

        // act
        IService resolvedInstance = container.Resolve<IService>();

        // assert
        resolvedInstance.Should().BeOfType<Service>(because: "the Resolve method should resolve the registered singleton instance for the service type");
    }

    [Test]
    public void ShouldResolveTheRegisteredInstanceForTheServiceType()
    {
        // arrange
        Container container = new();
        Service serviceInstance = new();
        container.RegisterInstance<IService>(serviceInstance);

        // act
        IService resolvedInstance = container.Resolve<IService>();

        // assert
        resolvedInstance.Should().BeSameAs(serviceInstance, because: "the Resolve method should resolve the registered instance for the service type");
    }

    [Test]
    public void ShouldThrowAnExceptionIfTheServiceTypeIsNotRegistered()
    {
        // arrange
        Container container = new();

        // act
        Action act = () => container.Resolve<IService>();

        // assert
        act.Should().Throw<KeyNotFoundException>(because: "the Resolve method should throw an exception if the service type is not registered");
    }

    [Test]
    public void ShouldResolveTheRegisteredFactoryAndCreateANewInstanceEachTime()
    {
        // arrange
        Container container = new();
        container.Register<IService, Service>();

        // act
        IService firstInstance = container.Resolve<IService>();
        IService secondInstance = container.Resolve<IService>();

        // assert
        firstInstance.Should().BeOfType<Service>(because: "the Resolve method should resolve the registered factory");
        firstInstance.Should().NotBeSameAs(secondInstance, because: "the Resolve method should create a new instance each time for non-singleton registrations");
    }

    [Test]
    public void ShouldResolveSingletonInstanceMultipleTimesAndReturnTheSameInstance()
    {
        // arrange
        Container container = new();
        container.RegisterSingleton<IService, Service>();

        // act
        IService firstInstance = container.Resolve<IService>();
        IService secondInstance = container.Resolve<IService>();

        // assert
        firstInstance.Should().BeSameAs(secondInstance, because: "the Resolve method should return the same singleton instance each time");
    }

    [Test]
    public void ShouldResolveServiceWithConstructorDependencies()
    {
        // arrange
        Container container = new();
        container.Register<IDependency, Dependency>();
        container.Register<IService, ServiceWithDependency>();

        // act
        IService resolvedInstance = container.Resolve<IService>();

        // assert
        resolvedInstance.Should().BeOfType<ServiceWithDependency>(because: "the Resolve method should resolve the service with constructor dependencies");
        ServiceWithDependency serviceWithDependency = (ServiceWithDependency)resolvedInstance;
        serviceWithDependency.Dependency.Should().BeOfType<Dependency>(because: "the Resolve method should recursively resolve constructor dependencies");
    }

    [Test]
    public void ShouldThrowAnExceptionWhenImplementationHasNoPublicConstructor()
    {
        // arrange
        Container container = new();
        container.Register<IService, ServiceWithoutPublicConstructor>();

        // act
        Action act = () => container.Resolve<IService>();

        // assert
        act.Should().Throw<InvalidOperationException>(because: "the Resolve method should throw an exception if the implementation has no public constructors");
    }

    [Test]
    public void ShouldSelectConstructorWithMostParametersWhenMultipleConstructorsExist()
    {
        // arrange
        Container container = new();
        container.Register<IDependency, Dependency>();
        container.Register<IService, ServiceWithMultipleConstructors>();

        // act
        IService resolvedInstance = container.Resolve<IService>();

        // assert
        resolvedInstance.Should().BeOfType<ServiceWithMultipleConstructors>(because: "the Resolve method should resolve the service with multiple constructors");
        ServiceWithMultipleConstructors serviceWithMultipleConstructors = (ServiceWithMultipleConstructors)resolvedInstance;
        serviceWithMultipleConstructors.ConstructorCalled.Should().Be("with-dependency", because: "the Resolve method should select the constructor with the most parameters");
    }

}