using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SaveThePony.Models
{
    public class IndexModel
    {
        public string MazeId { get; set; }

        [Required]
        public string PonyName { get; set; }

        [Range(15, 25)]
        public int Width { get; set; }
        [Range(15, 25)]
        public int Height { get; set; }
        [Range(0, 10)]
        public int Difficulty { get; set; }

        public List<List<string>> Walls { get; set; }
        public int Pony { get; set; }
        public int Domokun { get; set; }
        public int EndPoint { get; set; }

        /// <summary>
        /// Contains some info for the user, fx next step, steps to end, steps to domokun
        /// </summary>
        public List<string> Messages { get; set; }
        
        /// <summary>
        /// Represents the printout of the maze
        /// </summary>
        public string Maze { get; set; }
        public string Prize { get; internal set; }
    }
}
