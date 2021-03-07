using System;
using SA.Domain;

namespace SA.Application
{
    public interface ITripSummaryRepository
    {
        void Add(Driver driver, double miles, double milesPerHour, Guid processId);
    }
}