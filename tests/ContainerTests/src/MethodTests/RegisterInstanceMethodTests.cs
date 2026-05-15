using System;
using AwesomeAssertions;
using WB.Logging.LogSinks.Base;

namespace ContainerTests.MethodTests.RegisterInstanceMethodTests;

internal interface IService
{
}

internal sealed class Service : IService
{
}

public sealed class TheRegisterInstanceMethod
{
    [Test]
    public void ShouldRegisterTheInstanceForTheServiceType()
    {
        // arrange
        Container container = new();

        // act
        Service serviceInstance = new();
        container.RegisterInstance<IService>(serviceInstance);

        // assert
        Type implementationType = container.Registrations[typeof(IService)];
        object singletonInstance = container.Singletons[typeof(IService)];
        implementationType.Should().Be<Service>(because: "the RegisterInstance method should register the implementation type for the service type");
        singletonInstance.Should().BeSameAs(serviceInstance, because: "the RegisterInstance method should register the instance for the service type");
    }

    [Test]
    public void ShouldThrowAnExceptionIfTheInstanceIsNull()
    {
        // arrange
        Container container = new();

        // act
        Action act = () => container.RegisterInstance<IService>(null!);

        // assert
        act.Should().Throw<ArgumentNullException>(because: "the RegisterInstance method should throw an exception if the instance is null");
    }
}