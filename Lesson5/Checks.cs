using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lesson5
{
    public static class Checks
    {
        public static bool OutBorder(int coord)
        {
            if (coord > 9 || coord < 0)
            {
                return true;
            }
            return false;
        }

        public static bool CheckIsDestroyed(List<Position> ship, BattleField fleet)
        {
            if (ship == null)
                return false;

            foreach (Position deck in ship)
            {
                if (!fleet.battleField[deck.Y, deck.X].Shot)
                    return false;
            }
            return true;
        }
    }
}
