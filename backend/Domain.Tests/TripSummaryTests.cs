using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace SA.Domain.Tests
{
    public class TripSummaryTests
    {
        [Test] public void ValidDateGeneratesSummary()
        {
            var tripSummary = TripSummary.Generate(
                Guid.NewGuid(),
                new Driver("Dan"),
                new List<Tuple<StartTime, EndTime>>
                {
                    Tuple.Create(new StartTime(10, 10), new EndTime(10, 20)),
                    Tuple.Create(new StartTime(11, 10), new EndTime(11, 20)),
                    Tuple.Create(new StartTime(12, 10), new EndTime(12, 20))
                },
                30f);

            tripSummary.Driver.Should().Be(new Driver("Dan"));
            tripSummary.Miles.Should().Be(30);
            tripSummary.MilesPerHour.Should().Be(60);
        }
    }
}