using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SA.Domain;

namespace SA.Application
{
    public class InputFileProcessor
    {
        private IDriverRepository _driverRepo;
        private ILogger _logger;
        private ITripRepository _tripRepo;

        private readonly Dictionary<string, Action<IList<string>>> supportedCommands;

        public InputFileProcessor(
            IDriverRepository driverRepository,
            ILogger logger,
            ITripRepository tripRepository)
        {
            _driverRepo = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _logger = logger ??  throw new ArgumentNullException(nameof(logger));
            _tripRepo = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));

            supportedCommands = new Dictionary<string, Action<IList<string>>>
            {
                { "Driver", param => AddDriver(param) },
                { "Trip", param => AddTrip(param) }
            };
        }

        private void AddDriver(IList<string> name) 
        {
            var driverName = name.FirstOrDefault();
            if (_driverRepo.Exists(driverName))
                throw new Exception("Driver already exists");

            _driverRepo.Add(driverName ?? throw new ArgumentNullException(nameof(driverName)));
        }

        private void AddTrip(IList<string> values)
        {
            if (values.Count() != 4)
                throw new ArgumentException("Missing arguments");

            var startHour = values[1].Split(':').FirstOrDefault() ?? throw new ArgumentNullException("Invalid start time");
            var startMinutes = values[1].Split(':').LastOrDefault() ?? throw new ArgumentNullException("Invalid start time");
            var endHour = values[2].Split(':').FirstOrDefault() ?? throw new ArgumentNullException("Invalid end time");
            var endMinutes = values[2].Split(':').LastOrDefault() ?? throw new ArgumentNullException("Invalid end time");
            
            float distance;
            int hour;
            int minutes;

            StartTime startTime;
            EndTime endTime;

            if (!float.TryParse((string)values[3], out distance))
                throw new ArgumentException("Invalid distance");

            if (!int.TryParse(startHour, out hour))
                throw new ArgumentException("Invalid start time");
            if (!int.TryParse(startMinutes, out minutes))
                throw new ArgumentException("Invalid start time");
            startTime = new StartTime(hour, minutes);

            if (!int.TryParse(endHour, out hour))
                throw new ArgumentException("Invalid end time");
            if (!int.TryParse(endMinutes, out minutes))
                throw new ArgumentException("Invalid end time");
            endTime = new EndTime(hour, minutes);

            var driver = _driverRepo.Find(values[0]);
            if (driver is null)
                throw new ArgumentException($"Driver { values[0] } does not exist");

            var trip = new Trip(driver, startTime, endTime, distance);
            if (trip.AverageVelocity >= 5 && trip.AverageVelocity <= 100)
                _tripRepo.Add(trip);
        }

        public void NewBatch(string data, Guid guid) 
        {
            var instructions = data.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.RemoveEmptyEntries);

            foreach(var instruction in instructions) 
            {
                var command = instruction.Split(' ').FirstOrDefault();
                var arguments = instruction.Split(' ').Skip(1)?.ToList();

                if (command is null)
                {
                    _logger.Log(LogLevel.Error, "Invalid command", instruction);
                    continue;
                }

                if (arguments is null)
                {
                    _logger.Log(LogLevel.Error, "Missing arguments", instruction);
                    continue;
                }

                if (!supportedCommands.ContainsKey(command))
                {
                    _logger.Log(LogLevel.Error, "Invalid command", command);
                    continue;
                }

                var function = supportedCommands[command];

                try 
                {
                    function.Invoke(arguments);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, ex.Message, ex);
                    continue;
                }
            }
        }
    }
}