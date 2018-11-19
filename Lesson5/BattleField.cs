using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Lesson5
{
    enum FleetType
    {
        PlayerType,
        AIType
    }
    public class BattleField
    {
        public BattlePosition[,] battleField;

        FleetType fleetType;

        Position currentPosition = new Position(-1, -1);

        List<Position> currentShip = new List<Position>();

        List<Position> linearBattleField;

        public BattleField()
        {
            battleField = new BattlePosition[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    battleField[i, j] = new BattlePosition();
                }
            }
            GenerateInformationField();
        }

        public BattleField(string fileName)
        {
            battleField = new BattlePosition[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    battleField[i, j] = new BattlePosition();
                }
            }
            GenerateInformationField();
            var startDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            var filePosition = startDir + @"\" + fileName;
            StreamReader streamReader = new StreamReader(filePosition);

            using (streamReader)
            {
                for (int i = 0; i < 10; i++)
                {
                    var line = streamReader.ReadLine();
                    for (int j = 0; j < 10; j++)
                    {
                        if (line[j] == 'x')
                        {
                            battleField[i, j].Ship = true;
                        }
                    }
                }
            }
            fleetType = FleetType.PlayerType;
        }

        public bool PlayerShot()
        {
            Console.WriteLine("Enter Vertical coordinate");
            int y = EnterCoordinate() - 1;
            Console.WriteLine("Enter Horizontal coordinate");
            int x = EnterCoordinate() - 1;

            if (battleField[y, x].Shot)
            {
                Console.WriteLine("We have already checked this position, try again [PRESS ANY KEY]");
                Console.ReadLine();
                return true;
            }

            Shot(x, y);
            var ship = ReturnShipFromOnePosition(new Position(x, y));
            if (ship != null)
            {
                Console.WriteLine("Your artilley hit them [PRESS ANY KEY]");
                Console.ReadLine();
                if (Checks.CheckIsDestroyed(ship, this))
                {
                    foreach (Position position in ship)
                    {
                        ShotAllPositionsNearPosition(position);
                    }
                }
                return true;
            }
            Console.WriteLine("Your artilley missed [PRESS ANY KEY]");
            Console.ReadLine();
            return false;

        }

        static int EnterCoordinate()
        {
            int coordinate = 0;

            while (coordinate < 1 || coordinate > 10)
            {
                string yString = Console.ReadLine();
                int.TryParse(yString, out coordinate);
                if (coordinate < 1 || coordinate > 10)
                {
                    Console.WriteLine("Please, Enter again");
                }
            }

            return coordinate;
        }

        public bool AIShot()
        {
            Random random = new Random();
            bool shotSuccess = false;

            var ship = ReturnShipFromOnePosition(currentPosition);
            if (Checks.CheckIsDestroyed(ship, this))
            {
                foreach (Position position in ship)
                {
                    ShotAllPositionsNearPosition(position);
                    RemoveAllNearPosition(position, linearBattleField);
                }
                currentPosition = new Position(-1, -1);
                currentShip = new List<Position>();
            }
            if (currentPosition.X != -1)
            {
                if (currentShip.Count <= 1)
                {
                    shotSuccess = AIShotIntoPosition();
                }
                else
                {
                    shotSuccess = AIShotIntoShip();
                }
            }
            else
            {


                int positionNumber = random.Next(0, linearBattleField.Count);
                Position position = linearBattleField[positionNumber];
                int x = position.X;
                int y = position.Y;

                battleField[y, x].Shot = true;
                linearBattleField.Remove(new Position(y, x));
                if (battleField[y, x].Ship)
                {
                    currentPosition = new Position(x, y);
                    currentShip.Add(currentPosition);
                    shotSuccess = true;
                }
            }
            return shotSuccess;
        }

        public bool AIShotIntoShip()
        {
            Position oneEnd = currentShip[0];
            Position secondEnd = currentShip[1];
            bool vertical = oneEnd.X == secondEnd.X;
            string xORy = vertical ? "y" : "x";
            foreach (Position deck in currentShip)
            {
                int coordToCheck = vertical ? deck.Y : deck.X;

                if (coordToCheck < oneEnd.ReturnXorY(xORy))
                {
                    oneEnd = deck;
                }
                if (coordToCheck > secondEnd.ReturnXorY(xORy))
                {
                    secondEnd = deck;
                }
            }
            int dangerousCoordMin = oneEnd.ReturnXorY(xORy) - 1;
            bool canShotInOneEnd = !Checks.OutBorder(dangerousCoordMin);
            if (canShotInOneEnd)
            {
                bool positionShot = xORy == "y" ? battleField[oneEnd.Y - 1, oneEnd.X].Shot
                    : battleField[oneEnd.Y, oneEnd.X - 1].Shot;
                canShotInOneEnd = canShotInOneEnd && !positionShot;
            }
            bool ship = false;
            Position position;
            if (canShotInOneEnd)
            {
                if (vertical)
                {
                    battleField[oneEnd.Y - 1, oneEnd.X].Shot = true;
                    ship = battleField[oneEnd.Y - 1, oneEnd.X].Ship;
                }
                else
                {
                    battleField[oneEnd.Y, oneEnd.X - 1].Shot = true;
                    ship = battleField[oneEnd.Y, oneEnd.X - 1].Ship;
                }

                Direction direction = vertical ? Direction.Up : Direction.Left;
                position = PositionReturn(direction, oneEnd, 1);
            }
            else
            {
                if (vertical)
                {
                    battleField[secondEnd.Y + 1, secondEnd.X].Shot = true;
                    ship = battleField[secondEnd.Y + 1, secondEnd.X].Ship;
                }
                else
                {
                    battleField[secondEnd.Y, secondEnd.X + 1].Shot = true;
                    ship = battleField[secondEnd.Y, secondEnd.X + 1].Ship;
                }
                Direction direction = vertical ? Direction.Down : Direction.Right;
                position = PositionReturn(direction, secondEnd, 1);
            }
            if (ship)
            {
                currentPosition = position;
                currentShip.Add(currentPosition);
                return true;
            }
            return false;
        }

        public bool AIShotIntoPosition()
        {
            Random random = new Random();
            var notCheckedDirections = new List<Direction>() { Direction.Left, Direction.Right,
                                                                Direction.Up, Direction.Down };

            while (notCheckedDirections.Count != 0)
            {
                int directionNumber = random.Next(0, notCheckedDirections.Count);
                Direction goodDirection = notCheckedDirections[directionNumber];
                var nearPosition = PositionReturn(goodDirection, currentPosition, 1);

                bool ifShot = !Checks.OutBorder(nearPosition.X) && !Checks.OutBorder(nearPosition.Y) &&
                    !battleField[nearPosition.Y, nearPosition.X].Shot;

                if (ifShot)
                {
                    BattlePosition battlePosition = battleField[nearPosition.Y, nearPosition.X];
                    battlePosition.Shot = true;
                    if (battlePosition.Ship)
                    {
                        currentPosition = nearPosition;
                        currentShip.Add(currentPosition);
                        return true;
                    }
                    return false;
                }
                else
                {
                    notCheckedDirections.Remove(goodDirection);
                }
            }

            return false;
        }

        void GenerateInformationField()
        {
            linearBattleField = new List<Position>();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var position = new Position(j, i);
                    linearBattleField.Add(position);
                }
            }
        }

        static Position PositionReturn(Direction direction, Position startposition, int distance)
        {
            switch (direction)
            {
                case Direction.Left:
                    return new Position(startposition.X - distance, startposition.Y);

                case Direction.Right:
                    return new Position(startposition.X + distance, startposition.Y);

                case Direction.Down:
                    return new Position(startposition.X, startposition.Y + distance);

                case Direction.Up:
                    return new Position(startposition.X, startposition.Y - distance);
                default:
                    return new Position(-1, -1);
            }
        }

        void ShotAllPositionsNearPosition(Position position)
        {
            int x = position.X;
            int y = position.Y;
            for (int i = y - 1; i < y + 2; i++)
            {
                for (int j = x - 1; j < x + 2; j++)
                {
                    if (!Checks.OutBorder(i) && !Checks.OutBorder(j))
                    {
                        battleField[i, j].Shot = true;
                    }
                }
            }
        }

        List<Position> ReturnShipFromOnePosition(Position startPosition)
        {
            var ship = new List<Position>();

            int y = startPosition.Y;
            int x = startPosition.X;
            if (Checks.OutBorder(y) || Checks.OutBorder(x) || !battleField[y, x].Ship)
                return null;

            ship.Add(startPosition);
            bool vertical = true;
            if ((!Checks.OutBorder(x + 1) && battleField[y, x + 1].Ship)
                || (!Checks.OutBorder(x - 1) && battleField[y, x - 1].Ship))
            {
                vertical = false;
            }
            bool end;

            for (int i = 1, j = 1; ;)
            {
                end = true;
                if (vertical)
                {
                    if (!Checks.OutBorder(y + i) && battleField[y + i, x].Ship)
                    {
                        ship.Add(new Position(x, y + i));
                        end = false;
                        i++;
                    }

                    if (!Checks.OutBorder(y - j) && battleField[y - j, x].Ship)
                    {
                        ship.Add(new Position(x, y - j));
                        end = false;
                        j++;
                    }
                }
                else
                {
                    if (!Checks.OutBorder(x + i) && battleField[y, x + i].Ship)
                    {
                        ship.Add(new Position(x + i, y));
                        end = false;
                        i++;
                    }
                    if (!Checks.OutBorder(x - j) && battleField[y, x - j].Ship)
                    {
                        ship.Add(new Position(x - j, y));
                        end = false;
                        j++;
                    }
                }
                if (end)
                {
                    break;
                }
            }

            return ship;
        }

        public void ShowFleet()
        {

            if (fleetType == FleetType.AIType)
                ConsoleMoveColumn(0);

            Console.WriteLine("   A B C D E F G H I J ");
            if (fleetType == FleetType.AIType)
                ConsoleMoveColumn(1);

            Console.WriteLine("  ======================");
            for (int i = 0; i < 10; i++)
            {
                if (fleetType == FleetType.AIType)
                    ConsoleMoveColumn(i + 2);

                if (!Checks.OutBorder(i + 1))
                {
                    Console.Write(" {0}|", i + 1);
                }
                else
                {
                    Console.Write("{0}|", i + 1);
                }
                for (int j = 0; j < 10; j++)
                {
                    if (battleField[i, j].Ship)
                    {
                        if (!battleField[i, j].Shot)
                        {
                            if (fleetType == FleetType.PlayerType)
                            {
                                Console.Write("@");
                            }
                            else
                            {
                                Console.Write(" ");
                            }
                        }
                        else
                        {
                            Console.Write("X");
                        }
                        if (!Checks.OutBorder(j + 1) && battleField[i, j + 1].Ship)
                        {
                            if (fleetType == FleetType.PlayerType)
                            {
                                Console.Write(":");
                            }
                            else if (battleField[i, j].Shot && battleField[i, j + 1].Shot)
                            {
                                Console.Write(":");
                            }
                            else
                            {
                                Console.Write(" ");
                            }
                        }
                        else
                        {
                            Console.Write(" ");
                        }
                    }
                    else
                    {
                        if (battleField[i, j].Shot)
                        {
                            Console.Write("* ");
                        }
                        else
                        {
                            Console.Write("  ");
                        }
                    }
                }
                Console.WriteLine("|");
            }
            if (fleetType == FleetType.AIType)
            {
                Console.CursorLeft = 30;
                Console.CursorTop = 12;
            }
            Console.WriteLine("  ======================");
        }

        void Shot(int coordinateX, int coordinateY)
        {
            battleField[coordinateY, coordinateX].Shot = true;
        }

        static void ConsoleMoveColumn(int row)
        {
            Console.CursorLeft = 30;
            Console.CursorTop = row;
        }

        void RemoveAllNearPosition(Position position, List<Position> linearBattlefield)
        {
            int y = position.Y;
            int x = position.X;
            for (int i = y - 1; i < y + 2; i++)
            {
                for (int j = x - 1; j < x + 2; j++)
                {
                    linearBattlefield.Remove(new Position(j, i));
                }
            }
        }

        bool CheckDirection(ref Direction direction, Position startposition, int numberOfDecks)
        {
            Position currentPosition;

            for (int i = 1; i < numberOfDecks; i++)
            {
                currentPosition = PositionReturn(direction, startposition, i);
                if (!linearBattleField.Contains(currentPosition))
                {
                    return false;
                }
            }

            return true;
        }

        static Position GenerateStartPosition(List<Position> linearBattleField)
        {
            Random random = new Random();
            var currentPosition = new Position(-1, -1);

            while (!linearBattleField.Contains(currentPosition))
            {
                currentPosition = new Position(random.Next(0, 9), random.Next(0, 9));
            }

            return currentPosition;
        }

        public void GenerateFleet()
        {
            linearBattleField = new List<Position>();

            fleetType = FleetType.AIType;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var position = new Position(j, i);
                    linearBattleField.Add(position);
                }
            }

            GenerateXDeck(4);
            GenerateXDeck(3);
            GenerateXDeck(3);
            GenerateXDeck(2);
            GenerateXDeck(2);
            GenerateXDeck(2);
            GenerateXDeck(1);
            GenerateXDeck(1);
            GenerateXDeck(1);
            GenerateXDeck(1);
        }

        void GenerateXDeck(int numberOfDecks)
        {
            bool found = false;
            Direction goodDirection = Direction.Down;
            Position startPosition = new Position();

            while (!found)
            {
                startPosition = GenerateStartPosition(linearBattleField);
                var notCheckedDirections = new List<Direction>() { Direction.Left, Direction.Right,
                                                                Direction.Up, Direction.Down };
                Random random = new Random();

                while (!found && notCheckedDirections.Count != 0)
                {
                    int directionNumber = random.Next(0, notCheckedDirections.Count);
                    goodDirection = notCheckedDirections[directionNumber];
                    found = CheckDirection(ref goodDirection, startPosition, numberOfDecks);
                    notCheckedDirections.RemoveAt(directionNumber);
                }
            }

            Position currentPosition;

            for (int i = 0; i < numberOfDecks; i++)
            {
                currentPosition = PositionReturn(goodDirection, startPosition, i);
                int x = currentPosition.X;
                int y = currentPosition.Y;
                battleField[y, x].Ship = true;
                RemoveAllNearPosition(currentPosition, linearBattleField);
            }
            return;
        }
    }
}
