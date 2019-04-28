using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneticSharp;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Extensions.Multiple;
using Serilog;

namespace GenerationAlgorithm.GITC {
    class Program {
        static List<IChromosome> mostBestChromosomes = new List<IChromosome>();
        public static IChromosome mostBestChromosome = null;
        static GeneticAlgorithm GA = null;

        static FightExecuter FightExecuter = new FightExecuter();
        static ResultDictionary ResultMap = new ResultDictionary(FightExecuter);

        static void Main(string[] args) {
            //TODO: multiple log destinations for each type & lvl
            // why no any mutation
            // what's problem in selection
            // remove cache from fitness

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Properties.Settings.Default.LogsInfoPath, rollOnFileSizeLimit: true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, fileSizeLimitBytes: 5*1024*1024)
                .WriteTo.File(Properties.Settings.Default.LogsPath, rollOnFileSizeLimit: true, fileSizeLimitBytes: 5*1024*1024)
                .WriteTo.Console()
                .CreateLogger();
            try {
                var population = GetIntPopulation();
                var fitness = GetFitness();
                var selection = GetSelection();
                var crossover = GetCrossover();
                var mutation = GetMutation();


                var ga = GA = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
                ga.FitnessIsIdempotent = false;
                ga.Termination = GetTermination();

                Log.Information("Generation: The strongest algorithm GITC.");

                var latestFitness = 0.0;

                ga.PopulationPrepared += (sender, e) => {
                    ResultMap.Reset();
                    ResultMap.EvalResults(GA);
                };

                ga.GenerationRan += (sender, e) => {
                    var bestChromosome = ga.Population.CurrentGeneration.BestChromosome;
                    var bestFitness = bestChromosome.Fitness.Value;

                    mostBestChromosomes.Add(bestChromosome);

                    if (mostBestChromosome == null)
                    {
                        mostBestChromosome = bestChromosome;
                        mostBestChromosomes.Add(mostBestChromosome);
                    }
                    else if (!mostBestChromosomes.Any(x => x.Equals(bestChromosome)))
                    {
                        var result = ResultMap.GetFightResult(mostBestChromosome, bestChromosome, 5);
                        if (result < 0)
                        {
                            mostBestChromosome = bestChromosome;
                            mostBestChromosomes.Add(mostBestChromosome);
                            Log.Warning("Most Best Chromosome changed to: {0}", mostBestChromosome.GetTransferedString());
                        }
                    }


                    latestFitness = bestFitness;
                    var phenotype = bestChromosome.GetTransferedString();

                    Log.Information(
                        "Generation {0,2}: List: ({1})",
                        ga.GenerationsNumber,
                        string.Join(", ",
                            ga.Population.CurrentGeneration.Chromosomes
                            .OrderByDescending(x => x.Fitness)
                            .Select(x => x.GetTransferedString() + " #" + x.Fitness)
                        )
                    );
                    Log.Warning(
                        "Generation {0,2}: Best: ({1}) = {2}. Most Best Chromosome: ({3})",
                        ga.GenerationsNumber,
                        phenotype,
                        bestFitness,
                        mostBestChromosome.GetTransferedString()
                    );
                };

                ga.Start();
            }
            catch(Exception e) {
                Log.Error(e, "Exception is occured while");
            }

            //Console.WriteLine("Generation is ended.");
            Log.Warning("Most Best Chromosome equal: ({0})", mostBestChromosome.GetTransferedString());
            Log.Information("Generation is ended.");
            Console.ReadKey();
        }
        
        private static IPopulation GetIntPopulation()
        {
            var minValue = -10;
            var maxValue = 90;

            const int parametersSize = 8;

            var list = new List<IChromosome> { };
            for (int i = 0; i < parametersSize; i++)
                list.Add(new DecimalChromosome(minValue, maxValue, 4));

            var chromosome = new MultipleChromosome(list);
            return new Population(minSize: 9, maxSize: 150, adamChromosome: chromosome);
        }

        private static IFitness GetFitness() {
            return new FuncFitness((c) => {
                //if (!ResultMap.IsReady()) {
                //    lock (GA) {
                //        if (!ResultMap.IsReady())
                //            ResultMap.EvalResults(GA);
                //    }
                //}
                return ResultMap.GetResult(c);
            });
        }


        #region Genetic Algorithm Settings

        private static ISelection GetSelection() {
            //return new TournamentSelection(2, true); // Each round Select a chromosome with best Fitness value inside random group of 3 items. This chromosome will be removed from next round
            //return new TournamentSelection(2, false); // Each round Select a chromosome with best Fitness value inside random group of 3 items. This chromosome will be removed from next round
            return new EliteSelection();
        }

        private static ICrossover GetCrossover() {
            // here is the place for furher investigations
            //return new UniformCrossover(); // 50% of each parent
            return new ThreeParentCrossover();// Is good for us because generate 1 child for 3 parents, then take parents by Selection
			return new VotingRecombinationCrossover(3, 2);

            //return new CutAndSpliceCrossover();//The length of gene was changed
            //return new OrderedCrossover();// NEED TO BE ORDERED
            return new TwoPointCrossover(8, 23);
        }

        private static IMutation GetMutation() {
            //return new UniformMutation();
            return new FlipBitMutation();
            //FlipBitMutation - Takes the chosen genome and inverts the bits (i.e. if the genome bit is 1, it is changed to 0 and vice versa).
            //DisplacementMutation - In the displacement mutation operator, a substring is randomly selected from chromosome, is removed, then replaced at a randomly selected position. 
            //Insertion Mutation - In the insertion mutation operator, a gene is randomly selected from chromosome, is removed, then replaced at a randomly selected position. 
            //Partial Shuffle Mutation - In the partial shuffle mutation operator, we take a sequence S limited by two positions i and j randomly chosen, such that i&lt;j. The gene order in this sequence will be shuffled. Sequence will be shuffled until it becomes different than the starting order
            //Twors mutation - allows the exchange of position of two genes randomly chosen.
            //UniformMutation - This operator replaces the value of the chosen gene with a uniform random value selected between the user-specified upper and lower bounds for that gene. 
        }

        private static ITermination GetTermination() {
            return new OrTermination(
                new TimeEvolvingTermination(new TimeSpan(1,10,0)),
                new GenerationNumberTermination(300)
                );
            return new FitnessStagnationTermination(100);

            // And & OR terminator
            // Time Evolving Termination. - The genetic algorithm will be terminate when the evolving exceed the max time specified.
            // Generation number termination. - The genetic algorithm will be terminate when reach the expected generation number.
            // Fitness Threshold Termination - The genetic algorithm will be terminate when the best chromosome reach the expected fitness.
            // Fitness Stagnation Termination - The genetic algorithm will be terminate when the best chromosome's fitness has no change in the last generations specified.
        }
        #endregion

        #region Floating garbage

        private static IPopulation GetPopulation()
        {            
            var minValue = 0f;
            var maxValue = 90f;
            int totalBits = 10;
            int fractionDigits = 0;

            const int parametersSize = 3;

            double[] arrayOfMin = new double[parametersSize];
            double[] arrayOfMax = new double[parametersSize];
            int[] arrayOfBits = new int[parametersSize];
            int[] arrayOfFractionDigits = new int[parametersSize];

            for (int i = 0; i < parametersSize; i++)
            {
                arrayOfMin[i] = minValue;
                arrayOfMax[i] = maxValue;
                arrayOfBits[i] = totalBits;
                arrayOfFractionDigits[i] = fractionDigits;
            }

            var chromosome = new FloatingPointChromosome(arrayOfMin, arrayOfMax, arrayOfBits, arrayOfFractionDigits);
            throw new NotSupportedException();
            return new Population(minSize: 5, maxSize: 150, adamChromosome: chromosome);
        }
        #endregion
    }
}
