using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SA.Domain;

namespace SA.Application.Tests
{
    public class InputFileProcessorTests
    {
        const string DRIVER_NAME = "Dan";
        private readonly Driver _dan = new Driver(DRIVER_NAME);
        InputFileProcessor _processor;
        Mock<IInputFileImporterRepository> _mockImporterRepo;
        Mock<ILogger> _mockLogger;
        Mock<IDriverRepository> _mockDriverRepository;
        Mock<ITripRepository> _mockTripRepository;
        Mock<TripSummary> _mockTripSummary;
        Mock<ITripSummaryRepository> _mockTripSummaryRepo;

        [Test] public void IfTripIsGreaterThan100MPHDontAddItToRepository()
        {
            _mockDriverRepository = new Mock<IDriverRepository>();
            _mockImporterRepo = new Mock<IInputFileImporterRepository>();
            _mockLogger = new Mock<ILogger>();
            _mockTripRepository = new Mock<ITripRepository>();
            _mockTripSummaryRepo = new Mock<ITripSummaryRepository>();
            _mockTripSummary = new Mock<TripSummary>(
                _mockTripRepository.Object,
                _mockTripSummaryRepo.Object);

            _processor = new InputFileProcessor(
                _mockDriverRepository.Object,
                _mockImporterRepo.Object,
                _mockLogger.Object,
                _mockTripRepository.Object,
                _mockTripSummary.Object);

            _mockDriverRepository.Setup(x => 
                x.Add(It.IsAny<string>(), It.IsAny<Guid>()));
            _processor.NewBatch(
                string.Join(
                    Environment.NewLine,
                    new [] 
                    { 
                        "Driver Dan",
                        "Trip Dan 10:40 11:40 101.0"
                    }),
                Guid.NewGuid());
            _mockTripRepository.Verify(x => x.Add(
                new Trip(_dan,
                    new StartTime(10, 40),
                    new EndTime(11, 40),
                    101f),
                It.IsAny<Guid>()), Times.Never);
        }

        [Test] public void IfTripIsLowerThan5MPHDontAddItToRepository()
        {
            _mockDriverRepository = new Mock<IDriverRepository>();
            _mockImporterRepo = new Mock<IInputFileImporterRepository>();
            _mockLogger = new Mock<ILogger>();
            _mockTripRepository = new Mock<ITripRepository>();
            _mockTripSummaryRepo = new Mock<ITripSummaryRepository>();
            _mockTripSummary = new Mock<TripSummary>(
                _mockTripRepository.Object,
                _mockTripSummaryRepo.Object);

            _processor = new InputFileProcessor(
                _mockDriverRepository.Object,
                _mockImporterRepo.Object,
                _mockLogger.Object,
                _mockTripRepository.Object,
                _mockTripSummary.Object);

            _mockDriverRepository.Setup(x => 
                x.Add(It.IsAny<string>(), It.IsAny<Guid>()));
            _processor.NewBatch(
                string.Join(
                    Environment.NewLine,
                    new [] 
                    { 
                        "Driver Dan",
                        "Trip Dan 10:40 11:40 1.0"
                    }),
                Guid.NewGuid());
            _mockTripRepository.Verify(x => x.Add(
                new Trip(_dan,
                    new StartTime(10, 40),
                    new EndTime(11, 40),
                    1f), It.IsAny<Guid>()), Times.Never);
        }

        [Test] public void ReadValidDriverWritesItToRepository()
        {
            _mockDriverRepository = new Mock<IDriverRepository>();
            _mockImporterRepo = new Mock<IInputFileImporterRepository>();
            _mockLogger = new Mock<ILogger>();
            _mockTripRepository = new Mock<ITripRepository>();
            _mockTripSummaryRepo = new Mock<ITripSummaryRepository>();
            _mockTripSummary = new Mock<TripSummary>(
                _mockTripRepository.Object,
                _mockTripSummaryRepo.Object);

            _processor = new InputFileProcessor(
                _mockDriverRepository.Object,
                _mockImporterRepo.Object,
                _mockLogger.Object,
                _mockTripRepository.Object,
                _mockTripSummary.Object);

            _mockDriverRepository.Setup(x => 
                x.Add(It.IsAny<string>(), It.IsAny<Guid>()));
                
            _processor.NewBatch("Driver Dan", Guid.NewGuid());
            _mockDriverRepository.Verify(x => 
                x.Add(DRIVER_NAME, It.IsAny<Guid>()), Times.Once);
        }

        [Test] public void ReadValidTripWritesItToRepository()
        {
            _mockDriverRepository = new Mock<IDriverRepository>();
            _mockImporterRepo = new Mock<IInputFileImporterRepository>();
            _mockLogger = new Mock<ILogger>();
            _mockTripRepository = new Mock<ITripRepository>();
            _mockTripSummaryRepo = new Mock<ITripSummaryRepository>();
            _mockTripSummary = new Mock<TripSummary>(
                _mockTripRepository.Object,
                _mockTripSummaryRepo.Object);

            _processor = new InputFileProcessor(
                _mockDriverRepository.Object,
                _mockImporterRepo.Object,
                _mockLogger.Object,
                _mockTripRepository.Object,
                _mockTripSummary.Object);

            _mockDriverRepository.Setup(x => 
                x.Add(It.IsAny<string>(), It.IsAny<Guid>()));
            _mockDriverRepository.Setup(x => x.Find(DRIVER_NAME))
                                .Returns(_dan);
            _processor.NewBatch(
                string.Join(
                    Environment.NewLine,
                    new [] 
                    { 
                        "Driver Dan",
                        "Trip Dan 10:41 10:42 1.0"
                    }),
                Guid.NewGuid());
            _mockDriverRepository.Verify(x => 
                x.Add(DRIVER_NAME, It.IsAny<Guid>()), Times.Once);
            _mockTripRepository.Verify(x => 
                x.Add(It.IsAny<Trip>(), It.IsAny<Guid>()), Times.Once);
        }
    }
}