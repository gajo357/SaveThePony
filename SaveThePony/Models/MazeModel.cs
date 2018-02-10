using Newtonsoft.Json;
using System.Collections.Generic;

namespace SaveThePony.Models
{
    public class MazeModel
    {
        [JsonProperty(PropertyName = "maze_id")]
        public string MazeId { get; set; }

        [JsonProperty(PropertyName = "pony")]
        public List<int> Pony { get; set; }
        [JsonProperty(PropertyName = "domokun")]
        public List<int> Domokun { get; set; }
        [JsonProperty(PropertyName = "end-point")]
        public List<int> EndPoint { get; set; }

        [JsonProperty(PropertyName = "difficulty")]
        public int Difficulty { get; set; }

        [JsonProperty(PropertyName = "size")]
        public List<int> Size { get; set; } = new List<int>();

        [JsonProperty(PropertyName = "data")]
        public List<List<string>> Data { get; set; } = new List<List<string>>();

        [JsonProperty(PropertyName = "game-state")]
        public GameState GameState { get; set; }
    }

    public class GameState
    {
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "state-result")]
        public string StateResult { get; set; }

        [JsonProperty(PropertyName = "hidden-url")]
        public string HiddenUrl { get; set; }        
    }
}
