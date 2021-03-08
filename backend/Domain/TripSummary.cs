using System;
using System.Collections.Generic;
using System.Linq;

namespace SA.Domain
{
    public class TripSummary
    {
        private Driver _driver;
        private Guid _importId;
        private int _miles;
        private int _milesPerHour;

        public Driver Driver => _driver;
        public int Miles => _miles;
        public int MilesPerHour => _milesPerHour;

        public TripSummary(
            Guid importId, 
            Driver driver, 
            int miles, 
            int milesPerHour)
        {
            _importId = importId;
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _miles = miles;
            _milesPerHour = milesPerHour;
        }

        public static TripSummary Generate(
            Guid importId,
            Driver driver,
            IList<Tuple<StartTime, EndTime>> tripTimes,
            float totalDistance)
        {
            if (tripTimes is null)
                throw new ArgumentNullException(nameof(tripTimes));

            if (!tripTimes.Any())
                throw new ArgumentException(
                    $"{nameof(tripTimes)} must have at least one trip statistic", nameof(tripTimes));

            var totalTimeInMinutes = tripTimes.Sum(x => x.Item2.TotalMinutes - x.Item1.TotalMinutes);

            return new TripSummary(importId,
                driver,
                (int) Math.Round(totalDistance),
                (int) Math.Round(totalDistance / (totalTimeInMinutes == 0 ? 1 : totalTimeInMinutes) * 60));
        }
    }
}