using System;
using MPI;

namespace ConnectFour
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to connect four");
            Console.WriteLine("Computer moves are numbered with 9, your moves are numbered with 1");
            Console.Write("Enter 1 if you want to play first, or 9 if you want computer to play first: ");
            string input = Console.ReadLine();

            int[,] playingBoard = new int[7, 7];
            int[] availableFields = new int[7];
            var rand = new Random();
            int playingColumnNumber;
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
                if (computerMove) 
                {
                    playingColumnNumber = rand.Next(0, 6);
                    playingBoard[availableFields[playingColumnNumber], playingColumnNumber] = 9;

                    Console.Write("|");
                    for (int a = 0; a < 7; ++a)
                    {
                        if(playingBoard[availableFields[a],a] != 0)
                        {
                            Console.Write("  " + playingBoard[availableFields[a], a] + "  ");
                            Console.Write("|");
                        } else
                        {
                            Console.Write("     ");
                            Console.Write("|");
                        }
                    }

                    Console.WriteLine("");

                    ++availableFields[playingColumnNumber];
                    computerMove = false;
                } else
                {
                    Console.Write("Enter column number you want to play (0-6): ");
                    playingColumnNumber = int.Parse(Console.ReadLine());

                    playingBoard[availableFields[playingColumnNumber], playingColumnNumber] = 1;

                    Console.Write("|");


                    for (int a = 0; a < 7; ++a)
                    {
                        if (playingBoard[availableFields[a], a] != 0)
                        {
                            Console.Write("  " + playingBoard[availableFields[a], a] + "  ");
                            Console.Write("|");
                        }
                        else
                        {
                            Console.Write("     ");
                            Console.Write("|");
                        }
                    }

                    Console.WriteLine("");

                    ++availableFields[playingColumnNumber];
                    computerMove = true;
                }
            }


            for (int a = 0; a < 7; ++a)
            {

                Console.Write("|");

                for (int b = 0; b < 7; ++b)
                {
                    if (a == 0 && b == 0) Console.Write("  a  ");
                    else Console.Write("     ");
                    Console.Write("|");
                }

                Console.WriteLine(" ");
            }

        }
    }
}
