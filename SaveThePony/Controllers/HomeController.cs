using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SaveThePony.Models;
using SaveThePony.Services;

namespace SaveThePony.Controllers
{
    public class HomeController : Controller
    {
        private IMazeService MazeService { get; }

        public HomeController(IMazeService mazeService)
        {
            MazeService = mazeService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new IndexModel()
            {
                PonyName = "Applejack",
                Width = 25,
                Height = 25,
                Difficulty = 10
            };

            return View(model);
        }

        [ActionName(nameof(Index))]
        [HttpPost]
        public async Task<IActionResult> PostIndexAsync(IndexModel model)
        {
            if (ModelState.IsValid)
            {
                await MazeService.CreateAndRunGameAsync(model);
            }

            return View(model);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
