using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SA.Application;
using SA.Domain;

namespace SA.API.Tests
{
    public class BatchImportsControllerTests
    {
        private BatchImportsController _batchController;
        private Mock<IDriverRepository> _mockDriverRepo;
        private Mock<IFormFile> _mockFormFile;
        private Mock<IFormCollection> _mockFormCollection;
        private Mock<IInputFileImporterRepository> _mockImporterRepo;
        private Mock<IInputFileProcessor> _mockInputFileProcessor;
        private Mock<ITripSummaryRepository> _mockTripSummaryRepo;
        private Guid _processId;
        private FormFileCollection _formFileCollection;

        [SetUp] public void Initialize()
        {
            _mockFormFile = new Mock<IFormFile>();
            _mockDriverRepo = new Mock<IDriverRepository>();
            _mockImporterRepo = new Mock<IInputFileImporterRepository>();
            _mockInputFileProcessor = new Mock<IInputFileProcessor>();
            _mockTripSummaryRepo = new Mock<ITripSummaryRepository>();
            _mockFormCollection = new Mock<IFormCollection>();
            _formFileCollection = new FormFileCollection();

            _mockImporterRepo.Setup(i => 
                i.SaveStatus(
                    It.IsAny<Guid>(), 
                    It.IsAny<ImporterStatus>(), 
                    It.IsAny<string>()));

            _batchController = new BatchImportsController(
                _mockDriverRepo.Object,
                _mockInputFileProcessor.Object,
                _mockImporterRepo.Object,
                _mockTripSummaryRepo.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            _processId = Guid.NewGuid();
        }

        [Test] public async Task EmptyFileProvidedReturnsOKWithMessageNothingToProcess()
        {
             var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(string.Join("", new [] 
            {
                Environment.NewLine,
                Environment.NewLine,
                Environment.NewLine, 
                string.Empty
            }));
            writer.Flush();
            ms.Position = 0;

            _mockFormFile.Setup(_ => _.OpenReadStream())
                         .Returns(ms);
            _mockFormFile.Setup(_ => _.Length)
                         .Returns(ms.Length);

            _mockInputFileProcessor.Setup(
                _ => _.NewBatch(It.IsAny<string>(), It.IsAny<Guid>()));
            var formFileCollection = new FormFileCollection();
            formFileCollection.Add(_mockFormFile.Object);

            var m = new Mock<IFormCollection>();
            m.SetupGet(x => x.Files)
             .Returns(formFileCollection);

            var response = await _batchController.Upload(m.Object);

            response.Should().BeOfType<OkObjectResult>();
            var content = (response as OkObjectResult).Value;
            content.GetType()
                   .GetProperty("status")
                   .GetValue(content)
                   .Should()
                   .Be("success");
            content.GetType()
                   .GetProperty("data")
                   .GetValue(content)
                   .Should()
                   .BeNull();
        }

        [Test] public async Task NoFileProvidedReturnsBadRequest()
        {
            var response = await _batchController.Upload(null);
            response.Should().BeOfType<BadRequestObjectResult>();
            var content = (response as BadRequestObjectResult).Value;
            content.GetType()
                   .GetProperty("status")
                   .GetValue(content)
                   .Should()
                   .Be("fail");
            content.GetType()
                   .GetProperty("message")
                   .GetValue(content)
                   .Should()
                   .Be("File not received");

            _mockFormCollection.SetupGet(x => x.Files)
                               .Returns(_formFileCollection);
            response = await _batchController.Upload(_mockFormCollection.Object);
            response.Should().BeOfType<BadRequestObjectResult>();
            content = (response as BadRequestObjectResult).Value;
            content.GetType()
                   .GetProperty("status")
                   .GetValue(content)
                   .Should()
                   .Be("fail");
            content.GetType()
                   .GetProperty("message")
                   .GetValue(content)
                   .Should()
                   .Be("File not received");
        }
    
        [Test] public void ProcessIdNotInDatabaseReturnsNotFound()
        {
            _mockImporterRepo.Setup(i => i.Find(_processId))
                             .Returns(new Dictionary<ImporterStatus, DateTime>());

            var response = _batchController.Report(_processId);
            response.Should().BeOfType<NotFoundObjectResult>();

            var content = (response as NotFoundObjectResult).Value;
            content.GetType()
                   .GetProperty("status")?
                   .GetValue(content)
                   .Should()
                   .Be("fail");
            content.GetType()
                   .GetProperty("message")?
                   .GetValue(content)
                   .Should()
                   .Be("Not found");
        }

        [Test] public void ProcessStartedButHasntBeenComputedYetReturnsOkWithAnalyzedMessage()
        {
            _mockImporterRepo.Setup(i => i.Find(_processId))
                             .Returns(new Dictionary<ImporterStatus, DateTime>()
                             {
                                { ImporterStatus.Started, DateTime.Now }
                             });

            var response = _batchController.Report(_processId);
            response.Should().BeOfType<OkObjectResult>();

            var content = (response as OkObjectResult).Value;
            content.GetType()
                   .GetProperty("status")?
                   .GetValue(content)
                   .Should()
                   .Be("success");
            content.GetType()
                   .GetProperty("data")?
                   .GetValue(content)
                   .Should()
                   .Be("File is being analyzed");
        }

        [Test] public void ProcessHasNotBeenComputedYetReturnsOkWithComputingMessage()
        {
            _mockImporterRepo.Setup(i => i.Find(_processId))
                             .Returns(new Dictionary<ImporterStatus, DateTime>()
                             {
                                { ImporterStatus.Started, DateTime.Now.AddMinutes(-1) },
                                { ImporterStatus.Computing, DateTime.Now },
                             });

            var response = _batchController.Report(_processId);
            response.Should().BeOfType<OkObjectResult>();

            var content = (response as OkObjectResult).Value;
            content.GetType()
                   .GetProperty("status")?
                   .GetValue(content)
                   .Should()
                   .Be("success");
            content.GetType()
                   .GetProperty("data")?
                   .GetValue(content)
                   .Should()
                   .Be("File is being computed");
        }

        [Test] public void ProcessHasFailedReturnsOkWithFailMessage()
        {
            _mockImporterRepo.Setup(i => i.Find(_processId))
                             .Returns(new Dictionary<ImporterStatus, DateTime>()
                             {
                                { ImporterStatus.Started, DateTime.Now.AddMinutes(-2) },
                                { ImporterStatus.Computing, DateTime.Now.AddMinutes(-1) },
                                { ImporterStatus.Fail, DateTime.Now }
                             });

            var response = _batchController.Report(_processId);
            response.Should().BeOfType<OkObjectResult>();

            var content = (response as OkObjectResult).Value;
            content.GetType()
                   .GetProperty("status")?
                   .GetValue(content)
                   .Should()
                   .Be("success");
            content.GetType()
                   .GetProperty("data")?
                   .GetValue(content)
                   .Should()
                   .Be("File computing has been aborted due to an unexpected error");
        }

        [Test] public void ProcessCompletedReturnsOkWithTripSummary()
        {
            var dan = new Driver("Dan");
            var alex = new Driver("Alex");
            var bob = new Driver("Bob");
            var tripSummaryPerDriver = new Dictionary<Driver, TripSummary>
            {
                { dan, new TripSummary(_processId, dan, 39, 47) },
                { alex, new TripSummary(_processId, alex, 42, 34) },
                { bob, null}
            };

            _mockDriverRepo.Setup(d => d.GetByProcessId(_processId))
                           .Returns(new[] { dan, alex, bob });
            _mockTripSummaryRepo.Setup(t => t.GetAllByProcessId(_processId))
                                .Returns(tripSummaryPerDriver.Where(x => !(x.Value is null))
                                                             .Select(x => x.Value));
            _mockImporterRepo.Setup(i => i.Find(_processId))
                             .Returns(new Dictionary<ImporterStatus, DateTime>()
                             {
                                { ImporterStatus.Started, DateTime.Now.AddMinutes(-2) },
                                { ImporterStatus.Computing, DateTime.Now.AddMinutes(-1) },
                                { ImporterStatus.Completed, DateTime.Now }
                             });

            var response = _batchController.Report(_processId);
            response.Should().BeOfType<OkObjectResult>();

            var content = (response as OkObjectResult).Value;
            content.GetType()
                   .GetProperty("status")?
                   .GetValue(content)
                   .Should()
                   .Be("success");
            var data = (IList)(content.GetType()
                                      .GetProperty("data")?
                                      .GetValue(content));
            data.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(3);
            data.Cast<dynamic>()
                .Should()
                .BeEquivalentTo(tripSummaryPerDriver.Select(t => new 
                {
                    Driver = t.Key,
                    Miles = t.Value?.Miles,
                    MilesPerHour = t.Value?.MilesPerHour
                }));
        }

        [Test] public async Task ValidFileReturnsProcessId()
        {
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(string.Join("", new [] 
            {
                "Driver Dan",
                "Trip Dan 10:40 10:56 10.2"
            }));
            writer.Flush();
            ms.Position = 0;

            _mockFormFile.Setup(_ => _.OpenReadStream())
                         .Returns(ms);
            _mockFormFile.Setup(_ => _.Length)
                         .Returns(ms.Length);

            _mockInputFileProcessor.Setup(
                _ => _.NewBatch(It.IsAny<string>(), It.IsAny<Guid>()));
            var formFileCollection = new FormFileCollection();
            formFileCollection.Add(_mockFormFile.Object);

            var m = new Mock<IFormCollection>();
            m.SetupGet(x => x.Files)
             .Returns(formFileCollection);

            var response = await _batchController.Upload(m.Object);
            response.Should().BeOfType<OkObjectResult>();
            var content = (response as OkObjectResult).Value;
            content.GetType()
                   .GetProperty("status")?
                   .GetValue(content)
                   .Should()
                   .Be("success");
            content.GetType()
                   .GetProperty("data")?
                   .GetValue(content)
                   .Should()
                   .BeOfType<Guid>();

            _mockImporterRepo.Verify(
                _ => _.SaveStatus(It.IsAny<Guid>(), ImporterStatus.Started, null),
                Times.Once);
            _mockInputFileProcessor.Verify( 
                _ => _.NewBatch(It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Once);
        }
    }
}