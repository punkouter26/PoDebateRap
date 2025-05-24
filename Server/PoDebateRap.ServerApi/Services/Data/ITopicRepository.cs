using PoDebateRap.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Data
{
    public interface ITopicRepository
    {
        Task<List<Topic>> GetAllTopicsAsync();
        Task SeedInitialTopicsAsync();
    }
}
