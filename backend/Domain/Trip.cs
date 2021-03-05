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

            _driver = driver;
            _endTime = endTime;
            _startTime = startTime;
            _distance = distance;
        }
    }
}