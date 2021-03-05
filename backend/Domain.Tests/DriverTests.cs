using FluentAssertions;
using NUnit.Framework;
using System;

namespace SA.Domain.Tests
{
    public class DriverTests
    {
        [Test] public void EmptyOrNullDriverNameThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Driver(null),
                "Driver should not allow null reference for name")
                .ParamName
                .Should()
                .Be("name");
            Assert.Throws<ArgumentNullException>(
                () => new Driver(string.Empty),
                "Driver should not allow empty names")
                .ParamName
                .Should()
                .Be("name");
        }

        [Test] public void ValidNameForDriverDoesntThrowException()
        {
            Assert.DoesNotThrow(
                () => new Driver("Dan"), 
                "Dan is a valid driver name");
            
            new Driver("Dan").Name.Should().Be("Dan");
        }
    }
}