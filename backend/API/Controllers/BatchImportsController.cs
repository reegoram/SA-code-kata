using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SA.Application;

namespace SA.API
{
    public class BatchImportsController : Controller
    {
        private IInputFileProcessor _inputFileProcessor;
        private IDriverRepository _driverRepo;
        private ITripSummaryRepository _tripSummaryRepo;
        private IInputFileImporterRepository _importerRepo;

        public BatchImportsController(
            IDriverRepository driverRepository,
            IInputFileProcessor inputFileProcessor,
            IInputFileImporterRepository importerRepository,
            ITripSummaryRepository tripSummaryRepository)
        {
            _driverRepo = driverRepository
                ?? throw new ArgumentNullException(nameof(driverRepository));
            _importerRepo = importerRepository 
                ?? throw new ArgumentNullException(nameof(importerRepository));
            _inputFileProcessor = inputFileProcessor
                ?? throw new ArgumentNullException(nameof(inputFileProcessor));
            _tripSummaryRepo = tripSummaryRepository
                ?? throw new ArgumentNullException(nameof(tripSummaryRepository));
        }

        [HttpGet] public async Task<ActionResult> Report(Guid processId)
        {
            var importerStory = _importerRepo.Find(processId);

            if (!importerStory.Any())
                return NotFound(new
                {
                    status = "fail",
                    message = "Not found"
                });

            var lastStatus = importerStory.OrderBy(x => x.Value).Last();
            object data = null;

            switch (lastStatus.Key)
            {
                case ImporterStatus.Started:
                    data = "File is being analyzed";
                    break;
                case ImporterStatus.Computing:
                    data = "File is being computed";
                    break;
                case ImporterStatus.Fail:
                    data = "File computing has been aborted due to an unexpected error";
                    break;
                case ImporterStatus.Completed:        
                    // Retrieve driver registered in this batch
                    var drivers = _driverRepo.GetByProcessId(processId);

                    // Retrieves the trips stats per driver
                    var trips = _tripSummaryRepo.GetAllByProcessId(processId);

                    data = drivers.GroupJoin(
                            trips,
                            d => d,
                            t => t.Driver,
                            (d, t) => new
                            {
                                Driver = d,
                                Miles = t.Any() ? t.Sum(x => x.Miles) : (int?)null,
                                MilesPerHour = t.Any() ? t.Sum(x => x.MilesPerHour) : (int?)null
                            })
                        .ToList();

                    break;
                default:
                    throw new NotImplementedException("Unsupported scenario");
            }

            return Ok(new
            {
                status = "success",
                data
            });
        }

        [HttpPost] public async Task<ActionResult> Upload(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new 
                {
                    status = "fail",
                    message = "File not received"   
                });

            var fileContent = string.Empty;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                fileContent = (await reader.ReadToEndAsync()).Trim();
            }
            
            if (string.IsNullOrEmpty(fileContent))
                return Ok(new 
                {
                    status = "success",
                    data = (object) null
                });

            Guid processId = Guid.NewGuid();
            _importerRepo.SaveStatus(processId, ImporterStatus.Started);

            _ = Task.Run(
                    () => _inputFileProcessor.NewBatch(fileContent, processId)
                ).ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                data = processId
            });
        }
    }
}