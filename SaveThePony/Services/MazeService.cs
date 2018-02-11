using Newtonsoft.Json;
using PathFinder.Models;
using PathFinder.Services;
using SaveThePony.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SaveThePony.Services
{
    public class MazeService : IMazeService
    {
        private IPathFinderService PathFinderService { get; }
        private HttpClient HttpClient { get; }

        private const string BaseWebsite = @"https://ponychallenge.trustpilot.com";
        private const string ApiBaseWebsite = BaseWebsite + @"/pony-challenge/maze";

        public MazeService(IPathFinderService pathFinderService)
        {
            PathFinderService = pathFinderService;
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> CreateAndRunGameAsync(IndexModel model)
        {
            if(!await CreateGameAsync(model))
                return false;

            var solution = await Task.Run(() => CreateNetworkFromMaze(model));
            if (solution == null)
            {
                model.Messages = new List<string>() { "Could not find a solution for the maze" };
                return false;
            }

            return await FastForwardAsync(model, solution);
        }

        private async Task<bool> CreateGameAsync(IndexModel model)
        {
            var mazePattern = new MazeTemplate()
            {
                Width = model.Width,
                Height = model.Height,
                Difficulty = model.Difficulty,
                PonyName = model.PonyName
            };

            var mazeSpeck = JsonConvert.SerializeObject(mazePattern);
            var response = await HttpClient.PostAsync(ApiBaseWebsite, new StringContent(mazeSpeck, Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            var maze = JsonConvert.DeserializeObject<MazeModel>(content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                model.MazeId = maze.MazeId;

                await PopulateCurrentStateAsync(model);
                return true;
            }

            model.Messages = new List<string>();
            model.Messages.Add(maze.GameState.State);
            model.Messages.Add(maze.GameState.StateResult);

            return false;
        }

        private async Task<bool> FastForwardAsync(IndexModel model, Solution solution)
        {
            foreach (var step in solution.MovesToEnd.Values)
            {
                var response = await MoveAsync(model.MazeId, step);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var state = JsonConvert.DeserializeObject<GameState>(content);

                    model.Messages = new List<string>();
                    model.Messages.Add(state.State);
                    model.Messages.Add(state.StateResult);

                    return false;
                }
            }

            await PopulateCurrentStateAsync(model);

            return true;
        }

        private async Task<HttpResponseMessage> MoveAsync(string mazeId, DirectionsEnum direction)
        {
            var template = new DirectionTemplate()
            {
                Direction = direction.ToString().ToLower()
            };

            var content = JsonConvert.SerializeObject(template);

            return await HttpClient.PostAsync($"{ApiBaseWebsite}/{mazeId}", new StringContent(content, Encoding.UTF8, "application/json"));
        }

        private async Task PopulateCurrentStateAsync(IndexModel model)
        {
            var mazeTask = GetCurrentMazeState(model.MazeId);
            //var printTask = GetMazePrintStringAsync(model.MazeId);

            var maze = await mazeTask;
            //var print = await printTask;

            PopulateIndexModelFromMaze(model, maze);
            //model.Maze = print;
        }

        private async Task<MazeModel> GetCurrentMazeState(string mazeId)
        {
            var response = await HttpClient.GetAsync($"{ApiBaseWebsite}/{mazeId}");
            var mazeContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<MazeModel>(mazeContent);
        }

        private async Task<string> GetMazePrintStringAsync(string mazeId)
        {
            var printResponse = await HttpClient.GetAsync($"{ApiBaseWebsite}/{mazeId}/print");

            return await printResponse.Content.ReadAsStringAsync();
        }

        private Solution CreateNetworkFromMaze(IndexModel model)
        {
            var network = new Network();
            Node startNode = null;
            Node endNode = null;
            Node domokunNode = null;

            for (var row = 0; row < model.Height; row++)
            {
                for (var column = 0; column < model.Width; column++)
                {
                    var index = GetIndexFromRowAndColumn(row, column, model.Width);

                    var node = network.AddNode(index);
                    if (model.Pony == index)
                    {
                        startNode = node;
                    }
                    if (model.EndPoint == index)
                    {
                        endNode = node;
                    }
                    if (model.Domokun == index)
                    {
                        domokunNode = node;
                    }

                    if (!model.Walls[index].Contains("north"))
                    {
                        // there is a connection to the north
                        // same column, row above
                        var otherIndex = GetIndexFromRowAndColumn((row - 1), column, model.Width);
                        network.MakeLink(index, otherIndex);
                    }

                    if (!model.Walls[index].Contains("west"))
                    {
                        // there is a connection to the west
                        // same row, left column
                        var otherIndex = index - 1;
                        network.MakeLink(index, otherIndex);
                    }
                }
            }

            var path = PathFinderService.FindShortestPath(network, startNode, endNode);

            var moves = ConvertPathToMoves(path, model.Height, model.Width);
            var domokun = path.IndexOf(domokunNode);
            var movesToEnd = path.Count - 1;

            return new Solution()
            {
                NoMovesToDomokun = domokun,
                NoMovesToEnd = movesToEnd,
                MovesToEnd = moves,
            };
        }

        private IDictionary<int, DirectionsEnum> ConvertPathToMoves(IList<Node> path, int height, int width)
        {
            var moves = new Dictionary<int, DirectionsEnum>();

            for (var i = 1; i < path.Count; i++)
            {
                var node1 = path[i - 1];
                var node2 = path[i];
                var direction = GetDirectionBetweenNodes(node1.Id, node2.Id, height, width);
                moves.Add(node1.Id, direction);
            }

            return moves;
        }

        private DirectionsEnum GetDirectionBetweenNodes(int node1, int node2, int noRows, int noColumns)
        {
            (int row1, int column1) = GetRowAndColumnFromIndex(node1, noRows, noColumns);
            (int row2, int column2) = GetRowAndColumnFromIndex(node2, noRows, noColumns);

            if (row1 == row2)
            {
                return column2 > column1 ? DirectionsEnum.East : DirectionsEnum.West;
            }

            return row2 > row1 ? DirectionsEnum.South : DirectionsEnum.North;
        }

        private (int row, int column) GetRowAndColumnFromIndex(int index, int noRows, int noColumns)
        {
            for (var row = 0; row < noRows; row++)
            {
                for (var column = 0; column < noColumns; column++)
                {
                    if (GetIndexFromRowAndColumn(row, column, noColumns) == index)
                        return (row, column);
                }
            }

            return (-1, -1);
        }

        private static int GetIndexFromRowAndColumn(int row, int column, int noColumns)
        {
            return column + row * noColumns;
        }

        private void PopulateIndexModelFromMaze(IndexModel model, MazeModel maze)
        {
            model.Difficulty = maze.Difficulty;
            model.Height = maze.Size[0];
            model.Width = maze.Size[1];
            model.Walls = maze.Data;
            model.Pony = maze.Pony[0];
            model.Domokun = maze.Domokun[0];
            model.EndPoint = maze.EndPoint[0];

            model.Messages = new List<string>();
            model.Messages.Add(maze.GameState.StateResult);

            if (!string.IsNullOrEmpty(maze.GameState.HiddenUrl))
                model.Prize = $"{BaseWebsite}/{maze.GameState.HiddenUrl}";
        }
        
        private class Solution
        {
            public int NoMovesToDomokun { get; set; }
            public int NoMovesToEnd { get; set; }

            public IDictionary<int, DirectionsEnum> MovesToEnd { get; set; }
        }
    }
}
