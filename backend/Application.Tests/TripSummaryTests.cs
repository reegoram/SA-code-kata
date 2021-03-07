using SA.Domain;
using System;
using Moq;
using NUnit.Framework;

namespace SA.Application.Tests
{
    public class TripSummaryTests
    {
        private readonly Driver _dan = new Driver("Dan");
        private readonly EndTime _endTime = new EndTime(11, 00);
        private Guid _processId;
        private readonly StartTime _starTime = new StartTime(10, 40);
        private Mock<ITripRepository> _mockITripRepository;
        private Mock<ITripSummaryRepository> _mockITripSummaryRepository;
        private TripSummary _tripSummary;

        [SetUp] public void Initialize()
        {
            _mockITripRepository = new Mock<ITripRepository>();
            _mockITripSummaryRepository = new Mock<ITripSummaryRepository>();
            _processId = Guid.NewGuid();
        }

        [Test] public void ComputationShouldInvokeAddMethodInRepo()
        {
            _mockITripRepository.Setup(x => x.Find(_processId))
                                .Returns(new []
                                {
                                    new Trip(_dan, _starTime, _endTime, 20f),
                                    new Trip(
                                        _dan,
                                        new StartTime(_starTime.Hour + 1, _starTime.Minutes),
                                        new EndTime(_endTime.Hour + 1, _endTime.Minutes),
                                        20f)
                                });
            _mockITripSummaryRepository.Setup(x =>
                x.Add(It.IsAny<Driver>(),
                      It.IsAny<double>(),
                      It.IsAny<double>(),
                      It.IsAny<Guid>()));

            _tripSummary = new TripSummary(
                _mockITripRepository.Object,
                _mockITripSummaryRepository.Object);
            _tripSummary.ComputeSummary(_processId);

            _mockITripSummaryRepository.Verify(x =>
                x.Add(It.IsAny<Driver>(),
                      It.IsAny<double>(),
                      It.IsAny<double>(),
                      It.IsAny<Guid>()),
                Times.Once);
            _mockITripSummaryRepository.Verify(x => 
                x.Add(_dan,
                      40f,
                      60f,
                      _processId), 
                Times.Once);
        }
    }
}