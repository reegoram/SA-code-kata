using System;

namespace SA.Domain
{
    public abstract class DrivingTime
    {
        private int _hour;
        private int _minutes;
        protected int TotalMinutes => (_hour * 60) + _minutes;

        protected DrivingTime(int hour, int minutes)
        {
            if (0 > hour && hour >= 24)
                throw new ArgumentException("Invalid hour", nameof(hour));

            if (0 > minutes && minutes >= 60)
                throw new ArgumentException("Invalid minutes", nameof(minutes));

            _hour = hour;
            _minutes = minutes;
        }

        public static bool operator >(DrivingTime left, DrivingTime right)
            => left.TotalMinutes > right.TotalMinutes;
        public static bool operator <(DrivingTime left, DrivingTime right)
            => left.TotalMinutes < right.TotalMinutes;
    }
}