using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using PoDebateRap.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Data
{
    /// <summary>
    /// Using Repository Pattern to abstract data access logic for Rapper entities.
    /// This class provides a clean API for data operations, decoupling the application
    /// from the underlying data storage (Azure Table Storage).
    /// </summary>
    public class RapperRepository : IRapperRepository
    {
        private const string TableName = "PoDebateRapRappers";
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<RapperRepository> _logger;

        public RapperRepository(ITableStorageService tableStorageService, ILogger<RapperRepository> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        public async Task<List<Rapper>> GetAllRappersAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve all rappers from Table Storage...");
                var rappers = new List<Rapper>();
                await foreach (var entity in _tableStorageService.GetEntitiesAsync<RapperEntity>(TableName))
                {
                    rappers.Add(new Rapper
                    {
                        Name = entity.RowKey,
                        Wins = entity.Wins,
                        Losses = entity.Losses
                    });
                }
                _logger.LogInformation("Successfully retrieved {Count} rappers from Table Storage", rappers.Count);
                return rappers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all rappers from Table Storage.");
                throw;
            }
        }

        public async Task SeedInitialRappersAsync()
        {
            _logger.LogInformation("Checking if initial rappers need to be seeded...");
            
            try
            {
                var existingRappers = await GetAllRappersAsync();
                _logger.LogInformation("Found {Count} existing rappers in storage", existingRappers.Count);
                
                if (existingRappers.Count > 0)
                {
                    _logger.LogInformation("Existing rappers: {Names}", 
                        string.Join(", ", existingRappers.Select(r => r.Name)));
                }
                
                if (!existingRappers.Any())
                {
                    _logger.LogInformation("No rappers found. Seeding initial data.");
                    var initialRappers = new List<RapperEntity>
                    {
                        new RapperEntity("PoDebateRapRappers", "Eminem") { Wins = 0, Losses = 0 },
                        new RapperEntity("PoDebateRapRappers", "Kendrick Lamar") { Wins = 0, Losses = 0 },
                        new RapperEntity("PoDebateRapRappers", "Tupac Shakur") { Wins = 0, Losses = 0 },
                        new RapperEntity("PoDebateRapRappers", "The Notorious B.I.G.") { Wins = 0, Losses = 0 },
                        new RapperEntity("PoDebateRapRappers", "Nas") { Wins = 0, Losses = 0 },
                        new RapperEntity("PoDebateRapRappers", "Jay-Z") { Wins = 0, Losses = 0 },
                        new RapperEntity("PoDebateRapRappers", "Rakim") { Wins = 0, Losses = 0 },
                        new RapperEntity("PoDebateRapRappers", "Andre 3000") { Wins = 0, Losses = 0 },
                        new RapperEntity("PoDebateRapRappers", "Lauryn Hill") { Wins = 0, Losses = 0 },
                        new RapperEntity("PoDebateRapRappers", "Snoop Dogg") { Wins = 0, Losses = 0 }
                    };

                    foreach (var rapper in initialRappers)
                    {
                        await _tableStorageService.UpsertEntityAsync(TableName, rapper);
                    }
                    _logger.LogInformation("Initial rappers seeded successfully.");
                }
                else
                {
                    _logger.LogInformation("Rappers already exist. Skipping seeding.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during rapper seeding process");
                throw;
            }
        }

        public async Task UpdateWinLossRecordAsync(string winnerName, string loserName)
        {
            _logger.LogInformation("Updating win/loss record for winner: {WinnerName}, loser: {LoserName}", winnerName, loserName);
            try
            {
                var winnerEntity = await _tableStorageService.GetEntityAsync<RapperEntity>(TableName, "PoDebateRapRappers", winnerName);
                if (winnerEntity != null)
                {
                    winnerEntity.Wins++;
                    await _tableStorageService.UpsertEntityAsync(TableName, winnerEntity);
                }
                else
                {
                    _logger.LogWarning("Winner '{WinnerName}' not found in database. Creating new entry.", winnerName);
                    await _tableStorageService.UpsertEntityAsync(TableName, new RapperEntity("PoDebateRapRappers", winnerName) { Wins = 1, Losses = 0 });
                }

                var loserEntity = await _tableStorageService.GetEntityAsync<RapperEntity>(TableName, "PoDebateRapRappers", loserName);
                if (loserEntity != null)
                {
                    loserEntity.Losses++;
                    await _tableStorageService.UpsertEntityAsync(TableName, loserEntity);
                }
                else
                {
                    _logger.LogWarning("Loser '{LoserName}' not found in database. Creating new entry.", loserName);
                    await _tableStorageService.UpsertEntityAsync(TableName, new RapperEntity("PoDebateRapRappers", loserName) { Wins = 0, Losses = 1 });
                }
                _logger.LogInformation("Win/loss record updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating win/loss record.");
                throw;
            }
        }

        // Table Entity for Rapper
        public class RapperEntity : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; } // Rapper Name
            public int Wins { get; set; }
            public int Losses { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public Azure.ETag ETag { get; set; }

            public RapperEntity() { } // Parameterless constructor for deserialization

            public RapperEntity(string partitionKey, string rowKey)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
            }
        }
    }
}
