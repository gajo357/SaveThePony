using System;

namespace SaveThePony.Models
{
    [Flags]
    public enum DirectionsEnum
    {
        None = 0,
        North = 1,
        South = 2,
        West = 4,
        East = 8
    }
}
