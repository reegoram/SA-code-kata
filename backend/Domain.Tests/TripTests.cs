using System;
using FluentAssertions;
using NUnit.Framework;

namespace SA.Domain.Tests
{
    public class TripTests
    {
        const string DRIVER_NAME = "Dan";
        private readonly Driver _dan = new Driver(DRIVER_NAME);
        private readonly StartTime _startTime = new StartTime(10, 50);

        [Test] public void ComputeVelocityWithZeroTimeSpanInferItToOneMinute()
        {
            var trip = new Trip(
                _dan, 
                _startTime, 
                new EndTime(_startTime.Hour, _startTime.Minutes), 
                1f);

            trip.AverageVelocity.Should().Be(60f);
        }

        [Test] public void DistanceLessThanZeroThrowsAnException()
        {
            Assert.Throws<ArgumentException>(() => new Trip(
                    _dan,
                    _startTime,
                    new EndTime(_startTime.Hour, _startTime.Minutes),
                    -1))
                .ParamName
                .Should()
                .Be("distance");
        }

        [Test] public void EndTimeLessThanStartTimeThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new Trip(_dan, new StartTime(10, 40), new EndTime(9, 32), 0f),
                $"When end time is earlier than start time an argument exception must be thrown")
                .ParamName
                .Should()
                .Be("startTime");
        }

        [Test] public void NullDriverForTripThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Trip(null, null, null, 0f),
                $"A null driver should throw an { nameof(ArgumentNullException) }");
        }

        [Test] public void NullEndTimeThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Trip(_dan, new StartTime(0, 0), null, 0f),
                $"A null end time should throw an {nameof(ArgumentNullException)}");
        }

        [Test] public void NullStartTimeThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Trip(_dan, null, null, 0f),
                $"A null start time should throw an {nameof(ArgumentNullException)}");
        }

        [Test] public void TripTimeInMinutesShouldBeEndTimeMinusStartTime()
        {
            var endTime = new EndTime(_startTime.Hour, _startTime.Minutes + 5);
            var trip = new Trip(_dan, _startTime, endTime, 1f);

            trip.TripTimeInMinutes.Should().Be(endTime.TotalMinutes - _startTime.TotalMinutes);
        }

        [Test] public void ValidDriverDoesntThrowException()
        {
            Trip trip = null;

            Assert.DoesNotThrow(
                () => { trip = new Trip(_dan, new StartTime(0, 0), new EndTime(0, 0), 0f); },
                "A valid driver does not throw an exception");
            
            trip.Driver.Name.Should().Be(DRIVER_NAME);
        }
    }
}