using System;
using System.Linq;

namespace SA.Application
{
    public class TripSummary
    {
        private ITripRepository _tripRepo;
        private ITripSummaryRepository _tripSummaryRepo;

        public TripSummary(
            ITripRepository tripRepository, 
            ITripSummaryRepository tripSummaryRepository)
        {
            _tripRepo = tripRepository;
            _tripSummaryRepo = tripSummaryRepository;
        }

        public void ComputeSummary(Guid processId)
        {
            var trips = _tripRepo.Find(processId);
            var tripsPerDriver = trips.GroupBy(t => t.Driver,
                    (k, v) => new 
                    { 
                        Driver = k,
                        Distance = v.Sum(t => t.Distance),
                        DeltaTime = v.Sum(t => (t.TripTimeInMinutes))
                    })
                .Select(x => new 
                    {
                        Driver = x.Driver,
                        Distance = Math.Round(x.Distance),
                        AverageVelocity = Math.Round(x.Distance / x.DeltaTime * 60)
                    });
            
            foreach(var driverTrips in tripsPerDriver) 
            {
                _tripSummaryRepo.Add(
                    driverTrips.Driver, 
                    driverTrips.Distance, 
                    driverTrips.AverageVelocity, 
                    processId);
            }
        }
    }
}