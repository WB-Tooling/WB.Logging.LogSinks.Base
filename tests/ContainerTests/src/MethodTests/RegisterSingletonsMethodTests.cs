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

public sealed class TheRegisterSingletonsMethod
{
    [Test]
    public void ShouldRegisterTheImplementationTypeForTheServiceType()
    {
        // arrange
        Container container = new();

        // act
        container.RegisterSingleton<IService, Service>();

        // assert
        Type implementationType = container.Registrations[typeof(IService)];
        object singletonInstance = container.Singletons[typeof(IService)];
        implementationType.Should().Be<Service>(because: "the RegisterSingleton method should register the implementation type for the service type");
        singletonInstance.Should().BeOfType<Service>(because: "the RegisterSingleton method should register the singleton instance for the service type");
    }
}