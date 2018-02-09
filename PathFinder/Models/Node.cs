namespace PathFinder.Models
{
    public class Node
    {
        public Node(int id)
        {
            Id = id;
        }

        public int Id { get; }

        /// <summary>
        /// A bit extra bookkeeping for quicker access
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Distance to the start
        /// </summary>
        public int Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Node val)
            {
                return val.Id == Id;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
