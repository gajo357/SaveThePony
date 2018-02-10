using Newtonsoft.Json;

namespace SaveThePony.Models
{
    public class MazeTemplate
    {
        [JsonProperty(PropertyName = "maze-player-name")]
        public string PonyName { get; set; }

        [JsonProperty(PropertyName = "maze-width")]
        public int Width { get; set; }
        [JsonProperty(PropertyName = "maze-height")]
        public int Height { get; set; }
        [JsonProperty(PropertyName = "difficulty")]
        public int Difficulty { get; set; }
    }

    public class DirectionTemplate
    {
        [JsonProperty(PropertyName = "direction")]
        public string Direction { get; set; }
    }
}
