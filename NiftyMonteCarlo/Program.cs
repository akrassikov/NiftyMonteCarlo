using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace NiftyMonteCarlo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("################################################");
            Console.WriteLine("Nifty Monte Carlo Simulator");
            Console.WriteLine("Simulates number of runs required to receive at least one copy of all items with arbitrary probabilities");
            if (args.Length < 3)
            {
                throw new Exception("Not enough arguments passed to application. Input required separated by spaces: simulations, max runs per simulation, and at least one probability value.");
            }
            int simulations = int.Parse(args[0]);
            int maxRunsPerSimulation = int.Parse(args[1]);
            var probabilities = PrepareProbabilities(args);

            Console.WriteLine("################################################");
            Console.WriteLine("Number of simulations: {0}", simulations);
            Console.WriteLine("Max runs per simulation: {0}", maxRunsPerSimulation);
            Console.WriteLine("Item probabilities:");
            for (var i = 0; i < probabilities.Length; i++)
            {
                Console.WriteLine("Item {0}: {1}", i+1, probabilities[i]);
            }

            Console.WriteLine("################################################");
            Console.WriteLine("Beginning simulations:");
            Random generator = new Random();
            var results = RunSimulations(probabilities, simulations, maxRunsPerSimulation, generator);
            var filename = "results-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
            OutputResultsToFile(results, filename);

            Console.WriteLine("################################################");
            Console.WriteLine("Finished execution. Output in file {0}. Press any key to close...", filename);
            Console.ReadKey();
        }

        public static double[] PrepareProbabilities(string[] args)
        {
            var probabilities = new double[args.Length - 2];
            for (var i = 0; i < args.Length - 2; i++)
            {
                probabilities[i] = double.Parse(args[i + 2]);
            }

            if (probabilities.Sum() > 1)
            {
                throw new Exception("Sum of probabilities cannot be greater than one.");
            }

            return probabilities;
        }

        public static double[] PrepareCumulativeProbabilities(double[] probabilities)
        {
            var cumulativeProbabilities = new double[probabilities.Length];
            cumulativeProbabilities[0] = probabilities[0];
            for (var i = 1; i < probabilities.Length; i++)
            {
                cumulativeProbabilities[i] = cumulativeProbabilities[i - 1] + probabilities[i];
            }

            return cumulativeProbabilities;
        }

        public static int[] RunSimulations(double[] probabilities, int simulations, int maxRunsPerSimulation, Random generator)
        {
            var simulatedOccurences = new int[maxRunsPerSimulation];
            var progress = 0;
            var tenPercentComplete = simulations / 10;
            for (var s = 0; s < simulations; s++)
            {
                if (s % tenPercentComplete == 0)
                {
                    Console.WriteLine("Progress: {0}0%", progress);
                    progress++;
                }
                var runs = RunSingleSimulation(probabilities, simulations, maxRunsPerSimulation, generator);
                simulatedOccurences[runs-1]++;
            }

            Console.WriteLine("Progress: 100%");
            return simulatedOccurences;
        }

        public static int RunSingleSimulation(double[] probabilities, int simulations, int maxRunsPerSimulation, Random generator)
        {
            // return the number of runs required to get at least one copy of all items
            var cumulativeProbabilities = PrepareCumulativeProbabilities(probabilities);
            var foundItems = new int[probabilities.Length];
            var runs = 0;

            // keep doing runs until we find at least one of each item or we reach the max number of runs
            while (foundItems.Contains(0) && runs < maxRunsPerSimulation)
            {
                runs++;
                var result = DoRun(cumulativeProbabilities, generator);
                if (result < probabilities.Length)
                {
                    foundItems[result]++;
                }
                else
                {
                    // found trash
                }
            }

            return runs;
        }

        public static int DoRun(double[] cumulativeProbabilities, Random generator)
        {
            // return the run result as array index if found an item
            var r = generator.NextDouble();
            for (var i = 0; i < cumulativeProbabilities.Length; i++)
            {
                if (r <= cumulativeProbabilities[i])
                {
                    return i; // found item
                }
            }

            return cumulativeProbabilities.Length; // trash
        }

        public static void OutputResultsToFile(int[] results, string fileName)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                file.WriteLine("Number of Runs for Full Set,Occurences");
                for (var i = 0; i < results.Length; i++)
                {
                    file.WriteLine((i + 1) + "," + results[i]);
                }
            }
        }
    }
}
