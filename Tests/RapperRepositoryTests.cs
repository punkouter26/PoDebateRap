using Moq;
using Xunit;
using PoDebateRap.Shared.Models;
using PoDebateRap.ServerApi.Services.Data; // Add this using directive
using Microsoft.Extensions.Logging; // Required for ILogger

namespace PoDebateRap.Tests;

public class RapperRepositoryTests
{
    private readonly Mock<ITableStorageService> _mockTableStorageService;
    private readonly Mock<ILogger<RapperRepository>> _mockLogger;
    private readonly RapperRepository _repository;

    public RapperRepositoryTests()
    {
        _mockTableStorageService = new Mock<ITableStorageService>();
        _mockLogger = new Mock<ILogger<RapperRepository>>();
        _repository = new RapperRepository(_mockTableStorageService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateWinLossRecordAsync_UpdatesBothRappersCorrectly()
    {
        // Arrange
        var winnerName = "RapperWin";
        var loserName = "RapperLose";
        
        // Use RapperRepository.RapperEntity for mocking
        var initialWinnerEntity = new RapperRepository.RapperEntity("Rappers", winnerName) { Wins = 5, Losses = 2 };
        var initialLoserEntity = new RapperRepository.RapperEntity("Rappers", loserName) { Wins = 3, Losses = 4 };

        // Setup mock GetEntityAsync to return the initial rapper entities
        _mockTableStorageService.Setup(s => s.GetEntityAsync<RapperRepository.RapperEntity>("Rappers", "Rappers", winnerName))
                                .ReturnsAsync(initialWinnerEntity);
        _mockTableStorageService.Setup(s => s.GetEntityAsync<RapperRepository.RapperEntity>("Rappers", "Rappers", loserName))
                                .ReturnsAsync(initialLoserEntity);

        // Setup mock UpsertEntityAsync to capture the updated entities
        RapperRepository.RapperEntity? updatedWinnerEntity = null;
        RapperRepository.RapperEntity? updatedLoserEntity = null;
        _mockTableStorageService.Setup(s => s.UpsertEntityAsync("Rappers", It.IsAny<RapperRepository.RapperEntity>()))
                                .Callback<string, RapperRepository.RapperEntity>((tableName, entity) =>
                                {
                                    if (entity.RowKey == winnerName) updatedWinnerEntity = entity;
                                    if (entity.RowKey == loserName) updatedLoserEntity = entity;
                                })
                                .Returns(Task.CompletedTask);

        // Act
        await _repository.UpdateWinLossRecordAsync(winnerName, loserName);

        // Assert
        // Verify GetEntityAsync was called for both rappers
        _mockTableStorageService.Verify(s => s.GetEntityAsync<RapperRepository.RapperEntity>("Rappers", "Rappers", winnerName), Times.Once);
        _mockTableStorageService.Verify(s => s.GetEntityAsync<RapperRepository.RapperEntity>("Rappers", "Rappers", loserName), Times.Once);

        // Verify UpsertEntityAsync was called twice (once for each rapper)
        _mockTableStorageService.Verify(s => s.UpsertEntityAsync("Rappers", It.IsAny<RapperRepository.RapperEntity>()), Times.Exactly(2));

        // Check if the captured entities have the correct updated stats
        Assert.NotNull(updatedWinnerEntity);
        Assert.Equal(6, updatedWinnerEntity.Wins); // Initial 5 + 1 win
        Assert.Equal(2, updatedWinnerEntity.Losses); // Losses unchanged

        Assert.NotNull(updatedLoserEntity);
        Assert.Equal(3, updatedLoserEntity.Wins); // Wins unchanged
        Assert.Equal(5, updatedLoserEntity.Losses); // Initial 4 + 1 loss
    }

    // TODO: Add more tests for other repository methods (GetAll, GetByName, Seed, Delete, edge cases, error handling)
}
