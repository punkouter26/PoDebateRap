using PoDebateRap.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoDebateRap.Web.Services.News
{
    public interface INewsService
    {
        Task<List<NewsHeadline>> GetTopHeadlinesAsync(int count);
    }
}
