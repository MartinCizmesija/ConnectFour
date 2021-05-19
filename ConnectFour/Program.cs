using System;
using MPI;

namespace ConnectFour
{
    class Program
    {
        static void Main(string[] args)
        {
            int[,] playingBoard = new int[7,7];
            Console.WriteLine("Welcome to connect four");
            Console.WriteLine("Computer moves are numbered with 0, your moves are numbered with 1");
            Console.WriteLine("");

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
