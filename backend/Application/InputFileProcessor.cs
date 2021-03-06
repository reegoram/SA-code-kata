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

        private readonly Dictionary<string, Action<string>> supportedCommands;

        public InputFileProcessor(
            IDriverRepository driverRepository,
            ILogger logger,
            ITripRepository tripRepository)
        {
            _driverRepo = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _logger = logger ??  throw new ArgumentNullException(nameof(logger));
            _tripRepo = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));

            supportedCommands = new Dictionary<string, Action<string>>
            {
                { "Driver", param => AddDriver(param) },
                { "Trip", null }
            };
        }

        private void AddDriver(string driverName) 
        {
            if (_driverRepo.Exists(driverName))
                throw new Exception("Driver already exists");

            _driverRepo.Add(driverName ?? throw new ArgumentNullException(nameof(driverName)));
        }

        private void AddTrip(string arguments)
        {
            var values = arguments.Split(' ');
            
            if (values.Count() != 4)
                throw new ArgumentException("Missing arguments");

            // WIP
            var driver = new Driver(values[0]);
            var startHour = values[1].Split(':').FirstOrDefault() ?? throw new ArgumentNullException("Invalid start time");
            var startMinutes = values[1].Split(':').LastOrDefault() ?? throw new ArgumentNullException("Invalid start time");
            var endHour = values[2].Split(':').FirstOrDefault() ?? throw new ArgumentNullException("Invalid end time");
            var endMinutes = values[2].Split(':').LastOrDefault() ?? throw new ArgumentNullException("Invalid end time");
            
            float distance;
            int hour;
            int minutes;

            StartTime startTime;
            EndTime endTime;

            if (!float.TryParse(values[3], out distance))
                throw new ArgumentException("Invalid distance");
            if (!int.TryParse(startHour, out hour))
                throw new ArgumentException("");


            // _tripRepo.Add(new Trip(
            //     new Driver(values[0]),
            //     new StartTime(
            //         int.Parse(values[1].Split(':') ?? throw new ArgumentNullException(nameof(StartTime)))),

            //     )))
        }

        public void NewBatch(string data, Guid guid) 
        {
            var instructions = data.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.RemoveEmptyEntries);

            foreach(var instruction in instructions) 
            {
                var command = instruction.Split(' ').FirstOrDefault();
                var arguments = instruction.Split(' ').LastOrDefault();

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