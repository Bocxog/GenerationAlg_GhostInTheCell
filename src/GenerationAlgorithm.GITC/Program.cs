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
using Serilog;

namespace GenerationAlgorithm.GITC {
    class Program {
        static List<IChromosome> bestChromosomes = new List<IChromosome>();
        static GeneticAlgorithm GA = null;

        static FightExecuter FightExecuter = new FightExecuter();
        static ResultDictionary ResultMap = new ResultDictionary(FightExecuter);

        static void Main(string[] args) {
            //TODO: multiple log destinations for each type & lvl
            // why no any mutation
            // what's problem in selection
            // remove cache from fitness

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Properties.Settings.Default.LogsPath, rollOnFileSizeLimit: true, fileSizeLimitBytes: 5*1024*1024)
                .CreateLogger();
            try {
                var population = GetPopulation();
                var fitness = GetFitness();
                var selection = GetSelection();
                var crossover = GetCrossover();
                var mutation = GetMutation();


                var ga = GA = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
                ga.Termination = GetTermination();

                Log.Information("Generation: The strongest algorithm GITC.");

                var latestFitness = 0.0;

                ga.GenerationRan += (sender, e) => {
                    ResultMap.Reset();
                    var bestChromosome = ga.Population.CurrentGeneration.BestChromosome as FloatingPointChromosome;
                    //var bestChromosome = ga.BestChromosome as FloatingPointChromosome;
                    var bestFitness = bestChromosome.Fitness.Value;

                    bestChromosomes.Add(bestChromosome);

                    //if (bestFitness != latestFitness) {
                    latestFitness = bestFitness;
                    var phenotype = bestChromosome.ToFloatingPoints();

                    Log.Information(
                        "Generation {0,2}: ({1}) = {2}. Chromosome: {BestChromosome}",
                        ga.GenerationsNumber,
                        string.Join(":", phenotype),
                        bestFitness,
                        ga.BestChromosome
                    );

                    foreach (var chromosome in ga.Population.CurrentGeneration.Chromosomes)
                        chromosome.Fitness = null;
                    //}
                };

                ga.Start();
            }
            catch(Exception e) {
                Log.Error(e, "Exception is occured while");
            }
            
            Console.WriteLine("Generation is ended.");
            Log.Information("Generation is ended.");
            Console.ReadKey();
        }

        private static IPopulation GetPopulation() {
            var minValue = 0f;
            var maxValue = 90f;
            int totalBits = 10;
            int fractionDigits = 0;

            const int parametersSize = 3;

            double[] arrayOfMin = new double[parametersSize];
            double[] arrayOfMax = new double[parametersSize];
            int[] arrayOfBits = new int[parametersSize];
            int[] arrayOfFractionDigits = new int[parametersSize];

            for(int i = 0; i < parametersSize; i++) {
                arrayOfMin[i]            = minValue;
                arrayOfMax[i]            = maxValue;
                arrayOfBits[i]           = totalBits;
                arrayOfFractionDigits[i] = fractionDigits;
            }


            var chromosome = new FloatingPointChromosome(arrayOfMin, arrayOfMax, arrayOfBits, arrayOfFractionDigits);

            return new Population(minSize: 5, maxSize: 150, adamChromosome: chromosome);
        }

        private static IFitness GetFitness() {
            return new FuncFitness((c) => {
                if (!ResultMap.IsReady()) {
                    lock (GA) {
                        if (!ResultMap.IsReady())
                            ResultMap.EvalResults(GA);
                    }
                }
                return ResultMap.GetResult(c);
                //var fc = c as FloatingPointChromosome;
                //return bestChromosomes.AsParallel().Sum(x => {
                //    var fightResult = GetResult(fc, x);

                //    switch (fightResult) {
                //        case FightResult.Win: return 3;
                //        case FightResult.Draw: return 1;
                //        case FightResult.Lose:
                //        default:
                //            return 0;
                //            }
                //});
            });
        }


        #region Genetic Algorithm Settings

        private static ISelection GetSelection() {
            //return new TournamentSelection(2, true); // Each round Select a chromosome with best Fitness value inside random group of 3 items. This chromosome will be removed from next round
            return new TournamentSelection(2, false); // Each round Select a chromosome with best Fitness value inside random group of 3 items. This chromosome will be removed from next round
            return new EliteSelection();
        }

        private static ICrossover GetCrossover() {
            // here is the place for furher investigations
            //return new UniformCrossover(); // 50% of each parent
            return new OrderedCrossover(); //random select 2 points of intersect
            return new TwoPointCrossover();
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
                new TimeEvolvingTermination(new TimeSpan(5,0,0)),
                new GenerationNumberTermination(3)
                );
            return new FitnessStagnationTermination(100);

            // And & OR terminator
            // Time Evolving Termination. - The genetic algorithm will be terminate when the evolving exceed the max time specified.
            // Generation number termination. - The genetic algorithm will be terminate when reach the expected generation number.
            // Fitness Threshold Termination - The genetic algorithm will be terminate when the best chromosome reach the expected fitness.
            // Fitness Stagnation Termination - The genetic algorithm will be terminate when the best chromosome's fitness has no change in the last generations specified.
        }
        #endregion
    }
}
