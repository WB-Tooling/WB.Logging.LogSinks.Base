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

internal sealed class AnotherService : IService
{
}

public sealed class TheRegisterInstanceMethod
{
    [Test]
    public void ShouldRegisterThePreCreatedInstanceWithGenericParameters()
    {
        // arrange
        IContainer container = new Container();
        Service serviceInstance = new();

        // act
        container.RegisterInstance<IService>(serviceInstance);

        // assert
        IService resolvedInstance = container.Resolve<IService>();
        resolvedInstance.Should().BeSameAs(serviceInstance, because: "RegisterInstance should register the pre-created instance for the service type");
    }

    [Test]
    public void ShouldReturnTheSameInstanceEachTimeGenericParametersAreUsed()
    {
        // arrange
        IContainer container = new Container();
        Service serviceInstance = new();
        container.RegisterInstance<IService>(serviceInstance);

        // act
        IService firstInstance = container.Resolve<IService>();
        IService secondInstance = container.Resolve<IService>();

        // assert
        firstInstance.Should().BeSameAs(secondInstance, because: "RegisterInstance should return the same instance each time the service is resolved");
    }

    [Test]
    public void ShouldRegisterThePreCreatedInstanceWithTypeParameters()
    {
        // arrange
        IContainer container = new Container();
        Service serviceInstance = new();

        // act
        container.RegisterInstance(typeof(IService), serviceInstance);

        // assert
        object resolvedInstance = container.Resolve(typeof(IService));
        resolvedInstance.Should().BeSameAs(serviceInstance, because: "RegisterInstance with type parameters should register the pre-created instance");
    }

    [Test]
    public void ShouldReturnTheSameInstanceEachTimeTypeParametersAreUsed()
    {
        // arrange
        IContainer container = new Container();
        Service serviceInstance = new();
        container.RegisterInstance(typeof(IService), serviceInstance);

        // act
        object firstInstance = container.Resolve(typeof(IService));
        object secondInstance = container.Resolve(typeof(IService));

        // assert
        firstInstance.Should().BeSameAs(secondInstance, because: "RegisterInstance with type parameters should return the same instance each time");
    }

    [Test]
    public void ShouldThrowArgumentNullExceptionWhenNullInstanceIsProvidedGeneric()
    {
        // arrange
        IContainer container = new Container();

        // act
        Action act = () => container.RegisterInstance<IService>(null!);

        // assert
        act.Should().Throw<ArgumentNullException>(because: "RegisterInstance should throw an exception when a null instance is provided");
    }

    [Test]
    public void ShouldThrowArgumentNullExceptionWhenNullInstanceIsProvidedTypeParameters()
    {
        // arrange
        IContainer container = new Container();

        // act
        Action act = () => container.RegisterInstance(typeof(IService), null!);

        // assert
        act.Should().Throw<ArgumentNullException>(because: "RegisterInstance with type parameters should throw an exception when a null instance is provided");
    }

    [Test]
    public void ShouldOverwritePreviousInstanceRegistration()
    {
        // arrange
        IContainer container = new Container();
        Service firstInstance = new();
        Service secondInstance = new();

        // act
        container.RegisterInstance<IService>(firstInstance);
        container.RegisterInstance<IService>(secondInstance);

        // assert
        IService resolvedInstance = container.Resolve<IService>();
        resolvedInstance.Should().BeSameAs(secondInstance, because: "RegisterInstance should overwrite the previous registration");
    }

    [Test]
    public void ShouldRegisterDifferentImplementationTypes()
    {
        // arrange
        IContainer container = new Container();
        Service service = new();
        AnotherService anotherService = new();

        // act
        container.RegisterInstance<IService>(service);
        container.RegisterInstance<IService>(anotherService);

        // assert
        IService resolvedInstance = container.Resolve<IService>();
        resolvedInstance.Should().BeSameAs(anotherService, because: "RegisterInstance should register the last provided instance");
    }

    [Test]
    public void ShouldReturnInstanceMultipleTimesConsistently()
    {
        // arrange
        IContainer container = new Container();
        Service serviceInstance = new();
        container.RegisterInstance<IService>(serviceInstance);

        // act & assert
        for (int i = 0; i < 5; i++)
        {
            IService resolvedInstance = container.Resolve<IService>();
            resolvedInstance.Should().BeSameAs(serviceInstance, because: "RegisterInstance should consistently return the same instance");
        }
    }
}