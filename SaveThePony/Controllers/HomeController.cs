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
            var model = new IndexModel();

            return View(model);
        }

        [ActionName(nameof(Index))]
        [HttpPost]
        public async Task<IActionResult> PostIndexAsync(IndexModel model)
        {
            if (ModelState.IsValid)
            {
                if (await MazeService.CreateNewGameAsync(model))
                    return RedirectToAction(CurrentGameActionName);
            }

            return View(model);
        }

        private const string CurrentGameActionName = "CurrentGame";
        [ActionName(CurrentGameActionName)]
        [HttpGet]
        public async Task<IActionResult> CurrentGameAsync()
        {
            var model = await MazeService.GetCurrentMazeAsync();
            if (model == null)
                return RedirectToAction(nameof(Index));

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PostFastForwardAsync(IndexModel model)
        {
            await MazeService.FastForwardAsync(model);

            return RedirectToAction(CurrentGameActionName);
            //return await CurrentGameAsync();
        }

        [HttpPost]
        public async Task<IActionResult> PostNorthAsync(IndexModel model)
        {
            await MazeService.MoveAsync(model, DirectionsEnum.North);

            return RedirectToAction(CurrentGameActionName);
            //return await CurrentGameAsync();
        }

        [HttpPost]
        public async Task<IActionResult> PostSouthAsync(IndexModel model)
        {
            await MazeService.MoveAsync(model, DirectionsEnum.South);

            return RedirectToAction(CurrentGameActionName);
            //return await CurrentGameAsync();
        }

        [HttpPost]
        public async Task<IActionResult> PostEastAsync(IndexModel model)
        {
            await MazeService.MoveAsync(model, DirectionsEnum.East);

            return RedirectToAction(CurrentGameActionName);
            //return await CurrentGameAsync();
        }

        [HttpPost]
        public async Task<IActionResult> PostWestAsync(IndexModel model)
        {
            await MazeService.MoveAsync(model, DirectionsEnum.West);

            return RedirectToAction(CurrentGameActionName);
            //return await CurrentGameAsync();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
