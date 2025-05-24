using PoDebateRap.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Data
{
    public interface IRapperRepository
    {
        Task<List<Rapper>> GetAllRappersAsync();
        Task SeedInitialRappersAsync();
        Task UpdateWinLossRecordAsync(string winnerName, string loserName);
    }
}
