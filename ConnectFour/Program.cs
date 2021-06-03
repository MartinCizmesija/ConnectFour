using System;
using System.Collections.Generic;
using System.Linq;
using MPI;

namespace ConnectFour
{
    class Program
    {
        readonly static int BOARDDEPTH = 11;
        readonly static int SEARCHDEPTH = 8;
        readonly static int GRANULATIONLEVEL = 1;

        readonly static List<State> mappedStates = new List<State>();
        static int idCounter = 0;

        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Intracommunicator comm = Communicator.world;
                int procesId = comm.Rank;
                int numberOfWorkers = comm.Size -1;

                if (numberOfWorkers == 0) System.Environment.Exit(0);

                //master
                if (procesId == 0)
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

                    Dictionary<int, double> gatheredValues = new Dictionary<int, double>();

                    //read who goes first
                    if (input.Equals("9")) computerMove = true;
                    else computerMove = false;

                    //available fields are all on row 0
                    for (int a = 0; a < 7; ++a)
                    {
                        availableFields[a] = 0;
                    }

                    while (true)
                    {
                        //computer turn
                        if (computerMove)
                        {
                            Console.WriteLine("Computer move");

                            //create poll of states
                            idCounter = 0;
                            mappedStates.Clear();
                            gatheredValues.Clear();
                            for (int a = 0; a < 7; ++a)
                            {
                                CreateTaskPool(playingBoard, availableFields, a, computerMove, 0, null);
                            }

                            //send tasks to processors until done
                            int jobsDone = 0;
                            int count = mappedStates.Count;
                            while (jobsDone < count)
                            {
                                if (mappedStates[jobsDone].depth != GRANULATIONLEVEL) 
                                {
                                    ++jobsDone;
                                    continue;
                                }
                                comm.Receive(Communicator.anySource, 1, out int WorkerRank);
                                comm.Send(mappedStates[jobsDone], WorkerRank, 0);
                                //Console.WriteLine("sent taks" + jobsDone + " to " + WorkerRank);
                                ++jobsDone;
                            }

                            //gathering data from each process
                            for (int a = 0; a < numberOfWorkers; ++a)
                            {
                                comm.Receive(Communicator.anySource, 1, out int WorkerRank);
                                comm.Send(new State(-1), WorkerRank, 0);
                                comm.Receive(WorkerRank, 2, out Dictionary<int, double> processedValues);
                                Console.WriteLine("Worker rank: " + WorkerRank);
                                foreach (int key in processedValues.Keys)
                                {
                                    mappedStates[key].stateScore = processedValues[key];
                                }
                            }

                            //sending confirm that i got all the data from processors
                            for (int a = 0; a < numberOfWorkers; ++a)
                            {
                                comm.Send(a, a + 1, 5);
                            }

                            //calculate row scores
                            CalculateColumnTaskScore(GRANULATIONLEVEL);
                            double[] scores = new double[7];

                            foreach (State s in mappedStates)
                            {
                                if (s.depth == 0) {
                                    scores[s.column] = s.stateScore;
                                    Console.WriteLine("column: " + s.stateScore);
                                }
                                //scores[s.column] = s.stateScore;
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

                            while (availableFields[playingColumnNumber] == BOARDDEPTH)
                            {
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

                            //exit on win
                            if (VictoryCalculation(playingBoard, computerMove))
                            {
                                Console.WriteLine("Computer won!");
                                for (int a = 0; a < numberOfWorkers; ++a)
                                {
                                    comm.Receive(Communicator.anySource, 1, out int WorkerRank);
                                    comm.Send(new State(-100), WorkerRank, 0);
                                }
                                break;
                            }

                            //update available fields and end turn
                            ++availableFields[playingColumnNumber];
                            if (availableFields[playingColumnNumber] == maxDepth && maxDepth < 6) ++maxDepth;
                            computerMove = false;
                        }
                        else
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

                                if (a < maxDepth - 1)
                                {
                                    Console.WriteLine("");
                                    Console.Write("|");
                                }
                            }

                            Console.WriteLine("");
                            if (maxDepth == BOARDDEPTH) Console.WriteLine("|-----------------------------------------|");

                            //exit on win
                            if (VictoryCalculation(playingBoard, computerMove))
                            {
                                Console.WriteLine("You won!");
                                for (int a = 0; a < numberOfWorkers; ++a)
                                {
                                    comm.Receive(Communicator.anySource, 1, out int WorkerRank);
                                    comm.Send(new State(-100), WorkerRank, 0);
                                }
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

                //worker
                else
                {
                    Dictionary<int, double> scoreMap = new Dictionary<int, double>();
                    while (true)
                    {
                        comm.Send(comm.Rank, 0, 1);
                        comm.Receive(0, 0, out State workingState);
                        if (workingState.stateId == -100) break;
                        if (workingState.stateId == -1) 
                        {
                            comm.Send(scoreMap, 0, 2);
                            scoreMap.Clear();
                            comm.Receive(0, 5, out int val);
                            continue;
                        }
                        if (scoreMap.ContainsKey(workingState.stateId)) continue;

                        List<double> scoreList = new List<double>();
                        for (int a = 0; a < 7; ++a)
                        {
                            scoreList.Add(CalculateColumnScore(workingState.board, workingState.availableFields, a, workingState.computerTurn, workingState.depth + 1));
                        }

                        double endScore = 0;
                        foreach (double columnScore in scoreList)
                        {
                            endScore += columnScore;
                        }

                        workingState.stateScore = endScore / scoreList.Count;
                        scoreMap.Add(workingState.stateId, workingState.stateScore);
                    }
                }
            }

            bool VictoryCalculation(int[,] playingBoard, bool computerTurn)
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

            double CalculateColumnScore(int[,] playingBoard, int[] availableFields, int playedColumn, bool computerTurn, int depth)
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

            void CreateTaskPool(int[,] playingBoard, int[] availableFields, int playedColumn, bool computerTurn, int depth, State state)
            {
                //column not available
                if (availableFields[playedColumn] == BOARDDEPTH) return;
                //max depth reached
                if (depth == SEARCHDEPTH) return;

                int wantedNumber;
                if (computerTurn) wantedNumber = 9;
                else wantedNumber = 1;

                int[] cloneField = CloneField(availableFields);
                int[,] cloneBoard = CloneBoard(playingBoard);
                cloneBoard[cloneField[playedColumn], playedColumn] = wantedNumber;

                ++cloneField[playedColumn];

                State currentState = new State(idCounter, cloneBoard, cloneField, !computerTurn, playedColumn, depth, state);
                if (currentState.rootState != null)
                {
                    currentState.rootState.children.Add(currentState);
                }

                ++idCounter;
                mappedStates.Add(currentState);

                if (depth < GRANULATIONLEVEL)
                {
                    for (int a = 0; a < 7; ++a)
                    {
                        CreateTaskPool(cloneBoard, cloneField, a, !computerTurn, depth + 1, currentState);
                    }
                }
            }

            void CalculateColumnTaskScore(int depth)
            {
                //max depth reached
                foreach (State s in mappedStates)
                {
                    if (s.depth == depth - 1)
                    {
                        //calucating average
                        List<double> scoreList = new List<double>();
                        foreach (State child in s.children)
                        {
                            scoreList.Add(child.stateScore);
                        }
                        double endScore = 0;
                        foreach (double columnScore in scoreList)
                        {
                            endScore += columnScore;
                        }
                        s.stateScore = endScore / scoreList.Count;
                    }
                }

                if (depth - 1 > 0)
                {
                    CalculateColumnTaskScore(depth - 1);
                }
            }

            int[,] CloneBoard(int[,] playingBoard)
            {
                int[,] clone = new int[BOARDDEPTH, 7];
                for (int a = 0; a < BOARDDEPTH; ++a)
                {
                    for (int b = 0; b < 7; ++b)
                    {
                        clone[a, b] = playingBoard[a, b];
                    }
                }
                return clone;
            }

            int[] CloneField(int[] playingField)
            {
                int[] clone = new int[7];
                for (int a = 0; a < 7; ++a)
                {
                    clone[a] = playingField[a];
                }
                return clone;
            }

        }

        [Serializable]
        public class State
        {
            public int stateId;
            public int[,] board;
            public int[] availableFields;
            public bool computerTurn;
            public int depth;
            public int column;
            public double stateScore;
            public List<State> children = new List<State>();
            public State rootState;

            public State(int id, int[,] board, int[] availableFields, bool computerTurn, int column, int depth, State rootTask)
            {
                this.stateId = id;
                this.board = board;
                this.availableFields = availableFields;
                this.computerTurn = computerTurn;
                this.depth = depth;
                this.rootState = rootTask;
                this.column = column;
            }

            public State(int id)
            {
                stateId = id;
            }
        }

    }
}