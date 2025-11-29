using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoDebateRap.ServerApi.Services.Diagnostics;
using PoDebateRap.Shared.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PoDebateRap.UnitTests
{
    public class DiagnosticsServiceTests
    {
        private readonly Mock<ILogger<DiagnosticsService>> _mockLogger;
        private readonly Mock<HealthCheckService> _mockHealthCheckService;

        public DiagnosticsServiceTests()
        {
            _mockLogger = new Mock<ILogger<DiagnosticsService>>();
            _mockHealthCheckService = new Mock<HealthCheckService>();
        }

        [Fact]
        public async Task RunAllChecksAsync_WithHealthyChecks_ReturnsSuccessResults()
        {
            // Arrange
            var healthReport = new HealthReport(
                new Dictionary<string, HealthReportEntry>
                {
                    ["TestCheck1"] = new HealthReportEntry(
                        HealthStatus.Healthy, 
                        "Healthy", 
                        TimeSpan.FromMilliseconds(10), 
                        null, 
                        null),
                    ["TestCheck2"] = new HealthReportEntry(
                        HealthStatus.Healthy, 
                        "All good", 
                        TimeSpan.FromMilliseconds(5), 
                        null, 
                        null)
                },
                TimeSpan.FromMilliseconds(15));

            _mockHealthCheckService
                .Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var service = new DiagnosticsService(_mockLogger.Object, _mockHealthCheckService.Object);

            // Act
            var results = await service.RunAllChecksAsync();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.All(results, r => Assert.True(r.Success));
        }

        [Fact]
        public async Task RunAllChecksAsync_WithUnhealthyCheck_ReturnsFailureResult()
        {
            // Arrange
            var healthReport = new HealthReport(
                new Dictionary<string, HealthReportEntry>
                {
                    ["HealthyCheck"] = new HealthReportEntry(
                        HealthStatus.Healthy, 
                        "OK", 
                        TimeSpan.FromMilliseconds(10), 
                        null, 
                        null),
                    ["UnhealthyCheck"] = new HealthReportEntry(
                        HealthStatus.Unhealthy, 
                        "Connection failed", 
                        TimeSpan.FromMilliseconds(100), 
                        new Exception("Connection refused"), 
                        null)
                },
                TimeSpan.FromMilliseconds(110));

            _mockHealthCheckService
                .Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var service = new DiagnosticsService(_mockLogger.Object, _mockHealthCheckService.Object);

            // Act
            var results = await service.RunAllChecksAsync();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            
            var healthyResult = results.First(r => r.CheckName == "HealthyCheck");
            Assert.True(healthyResult.Success);
            
            var unhealthyResult = results.First(r => r.CheckName == "UnhealthyCheck");
            Assert.False(unhealthyResult.Success);
            Assert.Contains("failed", unhealthyResult.Message);
        }

        [Fact]
        public async Task RunAllChecksAsync_WithDegradedCheck_ReturnsDegradedAsFailure()
        {
            // Arrange
            var healthReport = new HealthReport(
                new Dictionary<string, HealthReportEntry>
                {
                    ["DegradedCheck"] = new HealthReportEntry(
                        HealthStatus.Degraded, 
                        "Performance degraded", 
                        TimeSpan.FromMilliseconds(500), 
                        null, 
                        null)
                },
                TimeSpan.FromMilliseconds(500));

            _mockHealthCheckService
                .Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var service = new DiagnosticsService(_mockLogger.Object, _mockHealthCheckService.Object);

            // Act
            var results = await service.RunAllChecksAsync();

            // Assert
            Assert.Single(results);
            Assert.False(results[0].Success); // Degraded is not Healthy
        }

        [Fact]
        public async Task RunAllChecksAsync_WithEmptyHealthReport_ReturnsEmptyList()
        {
            // Arrange
            var healthReport = new HealthReport(
                new Dictionary<string, HealthReportEntry>(),
                TimeSpan.Zero);

            _mockHealthCheckService
                .Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var service = new DiagnosticsService(_mockLogger.Object, _mockHealthCheckService.Object);

            // Act
            var results = await service.RunAllChecksAsync();

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }
    }
}
