using System;
using System.Collections.Generic;
using System.Linq;
using MPI;

namespace ConnectFour
{
    class Program
    {
        static int BOARDDEPTH = 11;
        static int SEARCHDEPTH = 8;
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
            
            //available fields are all on row 0
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

                    double[] scores = new double[7];
                    for (int a = 0; a < 7; ++a)
                    {
                        scores[a] = (CalculateColumnScore(playingBoard, availableFields, a, computerMove, 0));
                    }

                    double maxScore = scores.Max();

                    //if score for all columns is 0 randomise row played
                    int zeroCoutner = 0;
                    foreach (double score in scores)
                    {
                        if (score == 0) ++zeroCoutner;
                    }
                    if (zeroCoutner == 7) playingColumnNumber = rand.Next(0, 6);
                    else playingColumnNumber = scores.ToList().IndexOf(maxScore);

                    while (availableFields[playingColumnNumber] == BOARDDEPTH) {
                        if (zeroCoutner == 0) playingColumnNumber = rand.Next(0, 6);
                        else
                        {
                        scores[playingColumnNumber] = -1000;
                        maxScore = scores.Max();
                        playingColumnNumber = scores.ToList().IndexOf(maxScore);
                        }
                    }

                    //write move on playing board
                    playingBoard[availableFields[playingColumnNumber], playingColumnNumber] = 9;

                    //print playing board
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

                    //update available fields and end turn
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

                    //write move on playing board
                    playingBoard[availableFields[playingColumnNumber], playingColumnNumber] = 1;

                    //print playing board
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

                    //update available fields and end turn
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

        static double CalculateColumnScore (int[,] playingBoard, int[] availableFields, int playedColumn, bool computerTurn, int depth)
        {
            //column not available
            if (availableFields[playedColumn] == BOARDDEPTH) return 0;
            //max depth reached
            if (depth == SEARCHDEPTH) return 0;

            List<double> scoreList = new List<double>();

            int wantedNumber;
            if (computerTurn) wantedNumber = 9;
            else wantedNumber = 1;

            int[] cloneField = CloneField(availableFields);
            int[,] cloneBoard = CloneBoard(playingBoard);
            cloneBoard[cloneField[playedColumn], playedColumn] = wantedNumber;
            if (VictoryCalculation(cloneBoard, computerTurn) && computerTurn) return 1;
            if (VictoryCalculation(cloneBoard, computerTurn) && !computerTurn) return -1;

            ++cloneField[playedColumn];

            for (int a = 0; a < 7; ++a)
            {
                scoreList.Add(CalculateColumnScore(cloneBoard, cloneField, a, !computerTurn, depth + 1));
            }

            //calculating average score;
            double endScore = 0;
            foreach (double columnScore in scoreList) 
            {
                endScore += columnScore;
            }

            return endScore / scoreList.Count;
        }

        static int[,] CloneBoard(int[,] playingBoard)
        {
            int[,] clone = new int[BOARDDEPTH,7];
            for (int a = 0; a < BOARDDEPTH; ++a)
            {
                for (int b = 0; b < 7; ++b)
                {
                    clone[a, b] = playingBoard[a, b];
                }
            }
            return clone;
        }

        static int[] CloneField(int[] playingField)
        {
            int[] clone = new int[7];
            for (int a = 0; a < 7; ++a)
            {
                clone[a] = playingField[a];
            }
            return clone;
        }

    }
}