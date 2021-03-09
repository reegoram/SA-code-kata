using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SA.Application;

namespace SA.API
{
    [ApiController, Route("api/[controller]")]
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

        [HttpGet("logs")] public ActionResult GetLogs()
            => Ok(new LiteDatabase("log.db").GetCollection("log").Query());

        [HttpGet] public ActionResult GetAll()
        {
            var importHistory = _importerRepo.GetAll();

            return Ok(new
            {
                status = "success",
                data = importHistory
            });
        }

        [HttpGet("{id}")] public ActionResult Report(Guid id)
        {
            var importerStory = _importerRepo.Find(id);

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
                    var drivers = _driverRepo.GetByProcessId(id);

                    // Retrieves the trips stats per driver
                    var trips = _tripSummaryRepo.GetAllByProcessId(id);

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
                        .OrderByDescending(x => x.Miles)
                        .ToList();

                    break;
                case ImporterStatus.NotProcessed:
                    data = "File is not valid or has not valid data";
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

        [HttpPost] public async Task<ActionResult> Upload([FromForm] IFormCollection fileCollection)
        {
            var file = fileCollection?.Files.FirstOrDefault();

            if (file is null)
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