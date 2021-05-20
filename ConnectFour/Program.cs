using System;
using MPI;

namespace ConnectFour
{
    class Program
    {
        static int BOARDDEPTH = 6;
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to connect four");
            Console.WriteLine("Computer moves are numbered with 9, your moves are numbered with 1");
            Console.Write("Enter 1 if you want to play first, or 9 if you want computer to play first: ");
            string input = Console.ReadLine();

            int[,] playingBoard = new int[BOARDDEPTH, 7];
            int[] availableFields = new int[7];
            var rand = new Random();
            int playingColumnNumber;
            int maxDepth = 1;
            bool computerMove;

            //read who goes first
            if (input.Equals("9"))
            {
                computerMove = true;
            }
            else computerMove = false;
            
            //available fields are on row 0
            for(int a = 0; a < 7; ++a)
            {
                availableFields[a] = 0;
            }

            while (true)
            {
                //computer turn
                if (computerMove)
                {
                    Console.WriteLine("Computer move");
                    
                    playingColumnNumber = rand.Next(0, 6);
                    while (availableFields[playingColumnNumber] == BOARDDEPTH) {
                        playingColumnNumber = rand.Next(0, 6);
                    }

                    playingBoard[availableFields[playingColumnNumber], playingColumnNumber] = 9;

                    Console.Write("|");
                    for (int a = 0; a < maxDepth; ++a) 
                    {
                        for (int b = 0; b < 7; ++b)
                        {
                            if (playingBoard[a, b] != 0)
                            {
                                Console.Write("  " + playingBoard[a, b] + "  ");
                                Console.Write("|");
                            }
                            else
                            {
                                Console.Write("     ");
                                Console.Write("|");
                            }
                        }

                        if (a < maxDepth - 1)
                        {
                            Console.WriteLine("");
                            Console.Write("|");
                        }
                    }

                    Console.WriteLine("");
                    if (maxDepth == BOARDDEPTH) Console.WriteLine("|-----------------------------------------|");

                    if (VictoryCalculation(playingBoard, computerMove))
                    {
                        Console.WriteLine("Computer won!");
                        break;
                    }

                    ++availableFields[playingColumnNumber];
                    if (availableFields[playingColumnNumber] == maxDepth && maxDepth < 6) ++maxDepth;
                    computerMove = false;
                } else
                //player turn
                {
                    Console.Write("Enter column number you want to play (0-6): ");
                    playingColumnNumber = int.Parse(Console.ReadLine());
                    while (playingColumnNumber > 6)
                    {
                        Console.Write("Please enter valid column number (0-6): ");
                        playingColumnNumber = int.Parse(Console.ReadLine());
                    }

                    while (availableFields[playingColumnNumber] == BOARDDEPTH)
                    {
                        Console.Write("Column is full. Enter some other column number: ");
                        playingColumnNumber = int.Parse(Console.ReadLine());
                    }

                    playingBoard[availableFields[playingColumnNumber], playingColumnNumber] = 1;

                    Console.Write("|");
                    for (int a = 0; a < maxDepth; ++a)
                    {
                        for (int b = 0; b < 7; ++b)
                        {
                            if (playingBoard[a, b] != 0)
                            {
                                Console.Write("  " + playingBoard[a, b] + "  ");
                                Console.Write("|");
                            }
                            else
                            {
                                Console.Write("     ");
                                Console.Write("|");
                            }
                        }

                        if (a < maxDepth-1)
                        {
                            Console.WriteLine("");
                            Console.Write("|");
                        }
                    }

                    Console.WriteLine("");
                    if (maxDepth == BOARDDEPTH) Console.WriteLine("|-----------------------------------------|");

                    if (VictoryCalculation(playingBoard, computerMove))
                    {
                        Console.WriteLine("You won!");
                        break;
                    }

                    ++availableFields[playingColumnNumber];
                    if (availableFields[playingColumnNumber] == maxDepth && maxDepth < 6) ++maxDepth;
                    computerMove = true;
                }
            }

            Console.WriteLine("Enter anything to close");
            Console.ReadLine();
        }

        static bool VictoryCalculation(int[,] playingBoard, bool computerTurn)
        {
            int wantedNumber;
            if (computerTurn) wantedNumber = 9;
            else wantedNumber = 1;

            int horizontalVictoryCount = 0;

            for (int a = 0; a < BOARDDEPTH; ++a)
            {
                for (int b = 0; b < 7; ++b)
                {
                    //horizontal check
                    if (playingBoard[a, b] == wantedNumber) ++horizontalVictoryCount;
                    else horizontalVictoryCount = 0;
                    if (horizontalVictoryCount == 4) return true;

                    if (a <= BOARDDEPTH - 4)
                    {
                        //vertical check
                        if (playingBoard[a, b] == wantedNumber
                            && playingBoard[a + 1, b] == wantedNumber
                            && playingBoard[a + 2, b] == wantedNumber
                            && playingBoard[a + 3, b] == wantedNumber) return true;

                        //left diagonal check
                        if (b <= 3)
                        {
                            if (playingBoard[a, b] == wantedNumber
                                && playingBoard[a + 1, b + 1] == wantedNumber
                                && playingBoard[a + 2, b + 2] == wantedNumber
                                && playingBoard[a + 3, b + 3] == wantedNumber) return true;
                        }

                        //right diagonal check
                        if (b >= 3)
                        {
                            if (playingBoard[a, b] == wantedNumber
                                && playingBoard[a + 1, b - 1] == wantedNumber
                                && playingBoard[a + 2, b - 2] == wantedNumber
                                && playingBoard[a + 3, b - 3] == wantedNumber) return true;
                        }
                    }
                }
                horizontalVictoryCount = 0;
            }

            return false;
        }

    }
}