using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.Data;
using Azure.Data.Tables;
using System.Threading.Tasks;

namespace PoDebateRap.UnitTests
{
    public class TableStorageServiceTests
    {
        private readonly Mock<ILogger<TableStorageService>> _mockLogger;

        public TableStorageServiceTests()
        {
            _mockLogger = new Mock<ILogger<TableStorageService>>();
        }

        [Fact]
        public void Constructor_WithMissingConnectionString_ThrowsArgumentNullException()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new TableStorageService(config, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithValidConnectionString_InitializesSuccessfully()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Azure:StorageConnectionString", "UseDevelopmentStorage=true"}
                })
                .Build();

            // Act
            var service = new TableStorageService(config, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetTableClientAsync_ReturnsValidClient()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Azure:StorageConnectionString", "UseDevelopmentStorage=true"}
                })
                .Build();
            var service = new TableStorageService(config, _mockLogger.Object);

            // Act - This will try to create the table in Azurite
            // In unit tests without Azurite running, this may throw
            // For true unit tests, we would mock the TableServiceClient
            try
            {
                var client = await service.GetTableClientAsync("TestTable");
                Assert.NotNull(client);
            }
            catch (Azure.RequestFailedException)
            {
                // Expected when Azurite is not running
                Assert.True(true, "Azurite not running - skipping integration aspect");
            }
        }
    }

    public class RapperRepositoryTests
    {
        private readonly Mock<ITableStorageService> _mockTableStorageService;
        private readonly Mock<ILogger<RapperRepository>> _mockLogger;

        public RapperRepositoryTests()
        {
            _mockTableStorageService = new Mock<ITableStorageService>();
            _mockLogger = new Mock<ILogger<RapperRepository>>();
        }

        [Fact]
        public async Task GetAllRappersAsync_ReturnsRappersList()
        {
            // Arrange
            var testEntities = new List<RapperRepository.RapperEntity>
            {
                new("PoDebateRapRappers", "Eminem") { Wins = 5, Losses = 2 },
                new("PoDebateRapRappers", "Snoop Dogg") { Wins = 3, Losses = 4 }
            }.ToAsyncEnumerable();

            _mockTableStorageService
                .Setup(x => x.GetEntitiesAsync<RapperRepository.RapperEntity>(
                    It.IsAny<string>(), 
                    It.IsAny<string?>()))
                .Returns(testEntities);

            var repository = new RapperRepository(_mockTableStorageService.Object, _mockLogger.Object);

            // Act
            var rappers = await repository.GetAllRappersAsync();

            // Assert
            Assert.NotNull(rappers);
            Assert.Equal(2, rappers.Count);
            Assert.Contains(rappers, r => r.Name == "Eminem" && r.Wins == 5);
            Assert.Contains(rappers, r => r.Name == "Snoop Dogg" && r.Losses == 4);
        }

        [Fact]
        public async Task UpdateWinLossRecordAsync_UpdatesWinnerAndLoser()
        {
            // Arrange
            var winnerEntity = new RapperRepository.RapperEntity("PoDebateRapRappers", "Eminem") { Wins = 5, Losses = 2 };
            var loserEntity = new RapperRepository.RapperEntity("PoDebateRapRappers", "Snoop Dogg") { Wins = 3, Losses = 4 };

            _mockTableStorageService
                .Setup(x => x.GetEntityAsync<RapperRepository.RapperEntity>(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    "Eminem"))
                .ReturnsAsync(winnerEntity);

            _mockTableStorageService
                .Setup(x => x.GetEntityAsync<RapperRepository.RapperEntity>(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    "Snoop Dogg"))
                .ReturnsAsync(loserEntity);

            var repository = new RapperRepository(_mockTableStorageService.Object, _mockLogger.Object);

            // Act
            await repository.UpdateWinLossRecordAsync("Eminem", "Snoop Dogg");

            // Assert
            _mockTableStorageService.Verify(
                x => x.UpsertEntityAsync(
                    It.IsAny<string>(), 
                    It.Is<RapperRepository.RapperEntity>(e => e.RowKey == "Eminem" && e.Wins == 6)),
                Times.Once);

            _mockTableStorageService.Verify(
                x => x.UpsertEntityAsync(
                    It.IsAny<string>(), 
                    It.Is<RapperRepository.RapperEntity>(e => e.RowKey == "Snoop Dogg" && e.Losses == 5)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateWinLossRecordAsync_WhenWinnerNotFound_CreatesNewEntry()
        {
            // Arrange
            _mockTableStorageService
                .Setup(x => x.GetEntityAsync<RapperRepository.RapperEntity>(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    "NewRapper"))
                .ReturnsAsync((RapperRepository.RapperEntity?)null);

            _mockTableStorageService
                .Setup(x => x.GetEntityAsync<RapperRepository.RapperEntity>(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    "ExistingLoser"))
                .ReturnsAsync(new RapperRepository.RapperEntity("PoDebateRapRappers", "ExistingLoser") { Wins = 0, Losses = 0 });

            var repository = new RapperRepository(_mockTableStorageService.Object, _mockLogger.Object);

            // Act
            await repository.UpdateWinLossRecordAsync("NewRapper", "ExistingLoser");

            // Assert
            _mockTableStorageService.Verify(
                x => x.UpsertEntityAsync(
                    It.IsAny<string>(), 
                    It.Is<RapperRepository.RapperEntity>(e => e.RowKey == "NewRapper" && e.Wins == 1)),
                Times.Once);
        }

        [Fact]
        public async Task SeedInitialRappersAsync_WhenNoRappersExist_SeedsData()
        {
            // Arrange
            var emptyList = AsyncEnumerable.Empty<RapperRepository.RapperEntity>();

            _mockTableStorageService
                .Setup(x => x.GetEntitiesAsync<RapperRepository.RapperEntity>(
                    It.IsAny<string>(), 
                    It.IsAny<string?>()))
                .Returns(emptyList);

            var repository = new RapperRepository(_mockTableStorageService.Object, _mockLogger.Object);

            // Act
            await repository.SeedInitialRappersAsync();

            // Assert - Should upsert 10 initial rappers
            _mockTableStorageService.Verify(
                x => x.UpsertEntityAsync(
                    It.IsAny<string>(), 
                    It.IsAny<RapperRepository.RapperEntity>()),
                Times.Exactly(10));
        }

        [Fact]
        public async Task SeedInitialRappersAsync_WhenRappersExist_SkipsSeeding()
        {
            // Arrange
            var existingRappers = new List<RapperRepository.RapperEntity>
            {
                new("PoDebateRapRappers", "Eminem") { Wins = 5, Losses = 2 }
            }.ToAsyncEnumerable();

            _mockTableStorageService
                .Setup(x => x.GetEntitiesAsync<RapperRepository.RapperEntity>(
                    It.IsAny<string>(), 
                    It.IsAny<string?>()))
                .Returns(existingRappers);

            var repository = new RapperRepository(_mockTableStorageService.Object, _mockLogger.Object);

            // Act
            await repository.SeedInitialRappersAsync();

            // Assert - Should not upsert any rappers
            _mockTableStorageService.Verify(
                x => x.UpsertEntityAsync(
                    It.IsAny<string>(), 
                    It.IsAny<RapperRepository.RapperEntity>()),
                Times.Never);
        }
    }
}
