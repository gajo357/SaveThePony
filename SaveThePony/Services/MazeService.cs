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

        private string BaseWebsite = @"https://ponychallenge.trustpilot.com/pony-challenge/maze";

        public MazeService(IPathFinderService pathFinderService)
        {
            PathFinderService = pathFinderService;
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private IndexModel CurrentIndexModel { get; set; }
        private Solution CurrentSolution { get; set; }

        public async Task<IndexModel> GetCurrentMazeAsync()
        {
            if (CurrentIndexModel == null)
            {
                // we need to create a new
                return null;
            }
            else if (!string.IsNullOrEmpty(CurrentIndexModel.MazeId))
            {
                await PopulateCurrentStateAsync(CurrentIndexModel);
            }

            return CurrentIndexModel;
        }

        public async Task<bool> CreateNewGameAsync(IndexModel model)
        {
            CurrentIndexModel = null;
            CurrentSolution = null;

            var mazePattern = new MazeTemplate()
            {
                Width = model.Width,
                Height = model.Height,
                Difficulty = model.Difficulty,
                PonyName = model.PonyName
            };

            var mazeSpeck = JsonConvert.SerializeObject(mazePattern);

            var response = await HttpClient.PostAsync(BaseWebsite, new StringContent(mazeSpeck, Encoding.UTF8, "application/json"));

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var maze = JsonConvert.DeserializeObject<MazeModel>(content);
                model.MazeId = maze.MazeId;

                await PopulateCurrentStateAsync(model);

                CurrentIndexModel = model;

                return true;
            }

            return false;
        }

        public async Task<bool> FastForwardAsync(IndexModel model)
        {
            var solution = CurrentSolution;
            model.MazeId = CurrentIndexModel.MazeId;

            var i = 0;
            foreach (var step in solution.MovesToEnd.Values)
            {
                if (i++ >= model.FastForwardSteps)
                    break;

                var response = await MoveAsync(model.MazeId, step);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var state = JsonConvert.DeserializeObject<GameState>(content);

                    model.Message = state.StateResult;

                    return false;
                }
            }

            await PopulateCurrentStateAsync(model);

            CurrentIndexModel = model;

            return true;
        }

        public async Task<bool> MoveAsync(IndexModel model, DirectionsEnum direction)
        {
            model.MazeId = CurrentIndexModel.MazeId;

            var response = await MoveAsync(model.MazeId, direction);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                CurrentSolution = null;

                await PopulateCurrentStateAsync(model);

                CurrentIndexModel = model;

                return true;
            }

            return false;
        }

        private async Task<HttpResponseMessage> MoveAsync(string mazeId, DirectionsEnum direction)
        {
            var template = new DirectionTemplate()
            {
                Direction = direction.ToString().ToLower()
            };

            var content = JsonConvert.SerializeObject(template);

            return await HttpClient.PostAsync($"{BaseWebsite}/{mazeId}", new StringContent(content, Encoding.UTF8, "application/json"));
        }

        private async Task PopulateCurrentStateAsync(IndexModel indexModel)
        {
            var mazeTask = GetCurrentMazeState(indexModel.MazeId);
            var printTask = GetMazePrintStringAsync(indexModel.MazeId);

            var maze = await mazeTask;
            var print = await printTask;

            PopulateIndexModelFromMaze(indexModel, maze, print);

            var solution = await FindMazeSolutionAsync(indexModel);

            PopulateIndexModelFromSolution(indexModel, solution);
        }

        private async Task<MazeModel> GetCurrentMazeState(string mazeId)
        {
            var response = await HttpClient.GetAsync($"{BaseWebsite}/{mazeId}");
            var mazeContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<MazeModel>(mazeContent);
        }

        private async Task<string> GetMazePrintStringAsync(string mazeId)
        {
            var printResponse = await HttpClient.GetAsync($"{BaseWebsite}/{mazeId}/print");

            return await printResponse.Content.ReadAsStringAsync();
        }

        private async Task<Solution> FindMazeSolutionAsync(IndexModel model)
        {
            var solution = await Task.Run(() => CreateNetworkFromMaze(model));

            CurrentSolution = solution;

            return solution;
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
            var nextMove = moves[startNode.Id];
            var domokun = path.IndexOf(domokunNode);
            var movesToEnd = path.Count - 1;
            var allowedMoves = GetAllowedMoves(startNode, network.GetNodeConnections(startNode), model.Height, model.Width);

            return new Solution()
            {
                NoMovesToDomokun = domokun,
                NoMovesToEnd = movesToEnd,
                MovesToEnd = moves,
                NextMove = nextMove,
                AllowedMoves = allowedMoves
            };
        }

        private DirectionsEnum GetAllowedMoves(Node startNode, IEnumerable<Node> connections, int noRows, int noColumns)
        {
            var allowedMoves = DirectionsEnum.None;
            foreach(var node in connections)
            {
                var direction = GetDirectionBetweenNodes(startNode.Id, node.Id, noRows, noColumns);
                allowedMoves |= direction;
            }

            return allowedMoves;
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

        private static void PopulateIndexModelFromMaze(IndexModel indexModel, MazeModel maze, string print)
        {
            indexModel.Difficulty = maze.Difficulty;
            indexModel.Height = maze.Size[0];
            indexModel.Width = maze.Size[1];
            indexModel.Walls = maze.Data;
            indexModel.Pony = maze.Pony[0];
            indexModel.Domokun = maze.Domokun[0];
            indexModel.EndPoint = maze.EndPoint[0];

            indexModel.Maze = print;
        }

        private static void PopulateIndexModelFromSolution(IndexModel model, Solution solution)
        {
            var message = new StringBuilder();
            message.AppendLine($"Number of moves to Domokun: {solution.NoMovesToDomokun}");
            message.AppendLine($"Number of moves to the Exit: {solution.NoMovesToEnd}");
            message.AppendLine($"Your next move should be: {solution.NextMove}");

            model.Message = message.ToString();
            model.NextMove = solution.NextMove;
            model.AllowedMoves = solution.AllowedMoves;
        }

        private class Solution
        {
            public int NoMovesToDomokun { get; set; }
            public int NoMovesToEnd { get; set; }
            public DirectionsEnum NextMove { get; set; }

            public IDictionary<int, DirectionsEnum> MovesToEnd { get; set; }
            public DirectionsEnum AllowedMoves { get; internal set; }
        }
    }
}
