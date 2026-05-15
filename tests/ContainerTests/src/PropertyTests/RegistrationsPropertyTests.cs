using System;
using System.Collections.Generic;
using AwesomeAssertions;
using WB.Logging.LogSinks.Base;

namespace ContainerTests.PropertyTests.RegistrationsPropertyTests;

public sealed class TheRegistrationsProperty
{
    [Test]
    public void ShouldBeEmptyInitially()
    {
        // arrange
        Container container = new();

        // act
        IReadOnlyDictionary<Type, Type> registrations = container.Registrations;

        // assert
        registrations.Should().BeEmpty(because: "no registrations have been made yet");
    }
}
