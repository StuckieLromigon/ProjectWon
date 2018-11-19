using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lesson5
{
    public class BattlePosition
    {
        public bool Ship = false;
        public bool Shot = false;

        public BattlePosition()
        {
            Ship = false;
            Shot = false;
        }
        public BattlePosition(bool ship, bool shot)
        {
            Ship = ship;
            Shot = shot;
        }
    } 
}
