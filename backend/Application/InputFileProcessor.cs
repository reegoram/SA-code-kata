using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SA.Domain;

namespace SA.Application
{
    public class InputFileProcessor : IInputFileProcessor
    {
        private IDriverRepository _driverRepo;
        private ILogger<InputFileProcessor> _logger;
        private ITripRepository _tripRepo;
        private IInputFileImporterRepository _importerRepo;
        private TripSummaryComputation _tripSummary;

        private readonly Dictionary<string, Action<IList<string>, Guid>> supportedCommands;

        public InputFileProcessor(
            IDriverRepository driverRepository,
            IInputFileImporterRepository inputFileImporterRepository,
            ILogger<InputFileProcessor> logger,
            ITripRepository tripRepository,
            TripSummaryComputation tripSummary)
        {
            _driverRepo = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _importerRepo = inputFileImporterRepository ?? throw new ArgumentNullException(nameof(inputFileImporterRepository));
            _logger = logger ??  throw new ArgumentNullException(nameof(logger));
            _tripRepo = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));
            _tripSummary = tripSummary ?? throw new ArgumentNullException(nameof(TripSummaryComputation));

            supportedCommands = new Dictionary<string, Action<IList<string>, Guid>>
            {
                { "Driver", (param, processId) => AddDriver(param, processId) },
                { "Trip", (param, processId) => AddTrip(param, processId) }
            };
        }

        private void AddDriver(IList<string> name, Guid processId)
        {
            var driverName = name.FirstOrDefault();
            if (_driverRepo.Exists(driverName, processId))
                throw new Exception("Driver already exists");

            _driverRepo.Add(
                driverName ?? throw new ArgumentNullException(nameof(driverName)), 
                processId);
        }

        private void AddTrip(IList<string> values, Guid processId)
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

            var driver = _driverRepo.Find(values[0], processId);
            if (driver is null)
                throw new ArgumentException($"Driver { values[0] } does not exist");

            var trip = new Trip(driver, startTime, endTime, distance);
            if (trip.AverageVelocity >= 5 && trip.AverageVelocity <= 100)
                _tripRepo.Add(trip, processId);
        }

        public void NewBatch(string data, Guid processId) 
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
                    _logger.Log(LogLevel.Error, 
                        "Invalid command", 
                        new { ProcessId = processId, Instruction = instruction });
                    continue;
                }

                if (arguments is null)
                {
                    _logger.Log(
                        LogLevel.Error, 
                        "Missing arguments", 
                        new { ProcessId = processId, Instruction = instruction });
                    continue;
                }

                if (!supportedCommands.ContainsKey(command))
                {
                    _logger.Log(
                        LogLevel.Error, 
                        "Invalid command", 
                        new { ProcessId = processId, Command = command});
                    continue;
                }

                var function = supportedCommands[command];

                try 
                {
                    function.Invoke(arguments, processId);
                }
                catch (Exception ex)
                {
                    _logger.Log(
                        LogLevel.Error, 
                        ex.Message,
                        new { ProcessId = processId, Exception = ex });
                    continue;
                }
            }

            try 
            {
                _importerRepo.SaveStatus(processId, ImporterStatus.Computing);
                if (_tripSummary.ComputeSummary(processId))
                    _importerRepo.SaveStatus(processId, ImporterStatus.Completed);
                else
                    _importerRepo.SaveStatus(processId, ImporterStatus.NotProcessed, "No data to add");
            }
            catch(Exception ex)
            {
                _logger.Log(
                    LogLevel.Error, 
                    ex.Message,
                    new { ProcessId = processId, Exception = ex });
                _importerRepo.SaveStatus(processId, ImporterStatus.Fail, ex.Message);
            }
        }
    }
}