using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SA.Domain;

namespace SA.Application.Tests
{
    public class InputFileProcessorTests
    {
        InputFileProcessor processor;
        Mock<ILogger> mockLogger;
        Mock<IDriverRepository> mockDriverRepository;
        Mock<ITripRepository> mockTripRepository;

        [Test] public void ReadValidDriverWritesItToRepository()
        {
            mockDriverRepository = new Mock<IDriverRepository>();
            mockLogger = new Mock<ILogger>();
            mockTripRepository = new Mock<ITripRepository>();

            processor = new InputFileProcessor(
                mockDriverRepository.Object, 
                mockLogger.Object,
                mockTripRepository.Object);

            mockDriverRepository.Setup(x => x.Add(It.IsAny<string>()));
            processor.NewBatch("Driver Dan", Guid.NewGuid());
            mockDriverRepository.Verify(x => x.Add("Dan"), Times.Once);
        }

        [Test] public void ReadValidTripWritesItToRepository()
        {
            mockDriverRepository = new Mock<IDriverRepository>();
            mockLogger = new Mock<ILogger>();
            mockTripRepository = new Mock<ITripRepository>();

            processor = new InputFileProcessor(
                mockDriverRepository.Object, 
                mockLogger.Object,
                mockTripRepository.Object);

            mockDriverRepository.Setup(x => x.Add(It.IsAny<string>()));
            processor.NewBatch(
                string.Join(
                    Environment.NewLine,
                    new [] 
                    { 
                        "Driver Dan",
                        "Trip Dan 10:40 10:50 34.0"
                    }),
                Guid.NewGuid());
            mockDriverRepository.Verify(x => x.Add("Dan"), Times.Once);
            mockTripRepository.Verify(x => x.Add(
                new Trip(new Driver("Dan"),
                    new StartTime(10, 40),
                    new EndTime(10, 50),
                    34f)), Times.Once);
        }
    }
}