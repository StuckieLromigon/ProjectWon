using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Lesson5
{
    class Program
    {
        static BattleField playerFleet = new BattleField("yourfleet.txt");

        static BattleField AIFleet;

        static void Main(string[] args)
        {
            AIFleet = new BattleField();
            AIFleet.GenerateFleet();
            StartGame();
        }

        static void ShowFleet()
        {
            Console.Clear();
            playerFleet.ShowFleet();
            AIFleet.ShowFleet();
        }

        static void StartGame()
        {
            while (!IsGameOver())
            {

                bool playerShot = true;
                while (playerShot)
                {
                    ShowFleet();
                    playerShot = AIFleet.PlayerShot();
                }
                bool aiShot = true;
                int turns = 0;
                while (aiShot)
                {
                    turns++;
                    for (int i = 0; i < turns; i++)
                    {
                        Console.WriteLine("AI Is Thinking");
                    }
                    System.Threading.Thread.Sleep(2000);
                    aiShot = playerFleet.AIShot();
                    ShowFleet();
                }
            }
        }

        static bool IsGameOver()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if ((playerFleet.battleField[j, i].Ship && !playerFleet.battleField[j, i].Shot) || (AIFleet.battleField[j, i].Ship && !AIFleet.battleField[j, i].Shot))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
