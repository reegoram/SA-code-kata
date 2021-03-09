using SA.Domain;
using System;
using System.Linq;

namespace SA.Application
{
    public class TripSummaryComputation
    {
        private ITripRepository _tripRepo;
        private ITripSummaryRepository _tripSummaryRepo;

        public TripSummaryComputation(
            ITripRepository tripRepository, 
            ITripSummaryRepository tripSummaryRepository)
        {
            _tripRepo = tripRepository;
            _tripSummaryRepo = tripSummaryRepository;
        }

        public bool ComputeSummary(Guid processId)
        {
            var trips = _tripRepo.Find(processId);
            var tripsPerDriver = trips.GroupBy(
                t => t.Driver,
                (k, v) => TripSummary.Generate(
                    processId,
                    k,
                    v.Select(t => Tuple.Create(t.StartTime, t.EndTime))
                        .ToList(),
                    v.Sum(t => t.Distance)));

            if (tripsPerDriver.Any())
                _tripSummaryRepo.AddRange(tripsPerDriver);

            return tripsPerDriver.Any();
        }
    }
}