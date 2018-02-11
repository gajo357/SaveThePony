using SaveThePony.Models;
using System.Threading.Tasks;

namespace SaveThePony.Services
{
    public interface IMazeService
    {
        Task<bool> CreateAndRunGameAsync(IndexModel model);
    }
}
