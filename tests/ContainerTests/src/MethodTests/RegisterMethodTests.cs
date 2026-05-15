using System;
using AwesomeAssertions;
using WB.Logging.LogSinks.Base;

namespace ContainerTests.MethodTests.RegisterMethodTests;

internal interface IService
{
}

internal sealed class Service : IService
{
}

public sealed class TheRegisterMethod
{
    [Test]
    public void ShouldRegisterTheImplementationTypeForTheServiceType()
    {
        // arrange
        Container container = new();

        // act
        container.RegisterTransient<IService, Service>();

        // assert
        Type implementationType = container.Registrations[typeof(IService)];
        implementationType.Should().Be<Service>(because: "the Register method should register the implementation type for the service type");
    }
}