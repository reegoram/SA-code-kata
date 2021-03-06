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
        Mock<ILogger> _mockLogger;
        Mock<IDriverRepository> _mockDriverRepository;
        Mock<ITripRepository> _mockTripRepository;

        [Test] public void IfTripIsGreaterThan100MPHDontAddItToRepository()
        {
            _mockDriverRepository = new Mock<IDriverRepository>();
            _mockLogger = new Mock<ILogger>();
            _mockTripRepository = new Mock<ITripRepository>();

            _processor = new InputFileProcessor(
                _mockDriverRepository.Object, 
                _mockLogger.Object,
                _mockTripRepository.Object);

            _mockDriverRepository.Setup(x => x.Add(It.IsAny<string>()));
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
                    101f)), Times.Never);
        }

        [Test] public void IfTripIsLowerThan5MPHDontAddItToRepository()
        {
            _mockDriverRepository = new Mock<IDriverRepository>();
            _mockLogger = new Mock<ILogger>();
            _mockTripRepository = new Mock<ITripRepository>();

            _processor = new InputFileProcessor(
                _mockDriverRepository.Object, 
                _mockLogger.Object,
                _mockTripRepository.Object);

            _mockDriverRepository.Setup(x => x.Add(It.IsAny<string>()));
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
                    1f)), Times.Never);
        }

        [Test] public void ReadValidDriverWritesItToRepository()
        {
            _mockDriverRepository = new Mock<IDriverRepository>();
            _mockLogger = new Mock<ILogger>();
            _mockTripRepository = new Mock<ITripRepository>();

            _processor = new InputFileProcessor(
                _mockDriverRepository.Object, 
                _mockLogger.Object,
                _mockTripRepository.Object);

            _mockDriverRepository.Setup(x => x.Add(It.IsAny<string>()));
            _processor.NewBatch("Driver Dan", Guid.NewGuid());
            _mockDriverRepository.Verify(x => x.Add(DRIVER_NAME), Times.Once);
        }

        [Test] public void ReadValidTripWritesItToRepository()
        {
            _mockDriverRepository = new Mock<IDriverRepository>();
            _mockLogger = new Mock<ILogger>();
            _mockTripRepository = new Mock<ITripRepository>();

            _processor = new InputFileProcessor(
                _mockDriverRepository.Object, 
                _mockLogger.Object,
                _mockTripRepository.Object);

            _mockDriverRepository.Setup(x => x.Add(DRIVER_NAME));
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
            _mockDriverRepository.Verify(x => x.Add(DRIVER_NAME), Times.Once);
            _mockTripRepository.Verify(x => x.Add(It.IsAny<Trip>()), Times.Once);
        }
    }
}