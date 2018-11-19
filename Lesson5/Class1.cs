using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lesson5
{
    enum Direction
    {
        Left = 1,
        Right = 2,
        Up = 3,
        Down = 4,
        NoDirection
    }

    public struct Position
    {
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int ReturnXorY(string xy)
        {
            if (xy == "x")
            {
                return X;
            }
            return Y;
        }
    }
}
