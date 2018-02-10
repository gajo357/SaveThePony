using SaveThePony.Models;
using System.Threading.Tasks;

namespace SaveThePony.Services
{
    public interface IMazeService
    {
        Task<IndexModel> GetCurrentMazeAsync();
        Task<bool> CreateNewGameAsync(IndexModel model);
        Task<bool> FastForwardAsync(IndexModel model);
        Task<bool> MoveAsync(IndexModel model, DirectionsEnum north);
    }
}
