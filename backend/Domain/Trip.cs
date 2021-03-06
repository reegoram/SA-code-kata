using System;

namespace SA.Domain
{
    public class Trip
    {
        private Driver _driver;
        private StartTime _startTime;
        private EndTime _endTime;
        private float _distance;

        public Driver Driver => _driver;
        public float AverageVelocity 
        {
            get 
            {
                // TODO: When the time span is zero, the velocity will be indeterminate, 
                // due to the delta of time will tend to zero makes the division unoperable.
                // We might prefer throwing an exception like DivByZero.
                var timespan = (_endTime?.TotalMinutes ?? 0) - (_startTime?.TotalMinutes ?? 0);
                return _distance / (timespan == 0 ? 1 : timespan) * 60;
            }
        }

        private Trip() { }

        public Trip(
            Driver driver, 
            StartTime startTime, 
            EndTime endTime, 
            float distance)
        {
            if (driver is null)
                throw new ArgumentNullException(nameof(driver));
            if (startTime is null)
                throw new ArgumentNullException(nameof(startTime));
            if (endTime is null)
                throw new ArgumentNullException(nameof(endTime));
            if (startTime > endTime)
                throw new ArgumentException(
                    $"{ nameof(startTime) } must be earlier than {nameof(endTime)}",
                    nameof(startTime));
            if (distance < 0)
                throw new ArgumentException(
                    "Distance can't be less than zero", nameof(distance));

            _driver = driver;
            _endTime = endTime;
            _startTime = startTime;
            _distance = distance;
        }
    }
}