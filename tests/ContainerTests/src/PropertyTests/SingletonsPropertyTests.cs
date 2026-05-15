using System;
using System.Collections.Generic;
using AwesomeAssertions;
using WB.Logging.LogSinks.Base;

namespace ContainerTests.PropertyTests.SingletonsPropertyTests;

public sealed class TheSingletonsProperty
{
    [Test]
    public void ShouldBeEmptyInitially()
    {
        // arrange
        Container container = new();

        // act
        IReadOnlyDictionary<Type, object> singletons = container.Singletons;

        // assert
        singletons.Should().BeEmpty(because: "no singleton registrations have been made yet");
    }
}