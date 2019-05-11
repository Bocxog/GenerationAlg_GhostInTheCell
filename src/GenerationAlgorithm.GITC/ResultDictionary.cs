using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerationAlgorithm.GITC;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;

namespace GenerationAlgorithm {
    public class ResultDictionary {
        //protected Dictionary<IChromosome, int> Indexes;
        protected Dictionary<int, IChromosome> Chromosomes;
        protected IChromosome BestChromosome;
        protected int[,] Result;
        protected int Size;
        private FightExecuter FightExecuter;

        public ResultDictionary(FightExecuter fightExecuter) {
            Chromosomes = new Dictionary<int, IChromosome>();
            BestChromosome = null;
            this.FightExecuter = fightExecuter;
        }

        public bool IsReady() {
            return Result != null;
        }

        public IChromosome CheckBest(IChromosome chromosome)
        {
            if (BestChromosome == null)
            {
                BestChromosome = chromosome;
            }
            else
            {
                if (GetFightResult(BestChromosome, chromosome, 5) < 0)
                    BestChromosome = chromosome;
            }

            return BestChromosome;
        }

        public void EvalResults(GeneticAlgorithm ga) {
            Size = ga.Population.CurrentGeneration.Chromosomes.Count;

            Result = new int[Size, Size + 1];

                var index = 0;
            foreach(var chromosome in ga.Population.CurrentGeneration.Chromosomes) {
                //Indexes[chromosome] = index;
                Chromosomes[index] = chromosome;
                index++;
            }
            for (int i = 0; i < Size; i++) {
                for (int j = i + 1; j < Size; j++) {
                    var fightResult = GetFightResult(Chromosomes[i], Chromosomes[j], 1);
                    Result[i, j] = fightResult;
                    Result[j, i] = -fightResult;
                }
                if (Program.mostBestChromosome != null)
                {
                    var fightResult = GetFightResult(Chromosomes[i], Program.mostBestChromosome, 2);
                    Result[i, Size] = fightResult * 2;
                }
            }
        }

        public int GetFightResult(IChromosome chromosome1, IChromosome chromosome2, int count) {
            return FightExecuter.GetCompetitionResult(chromosome1, chromosome2, count);
        }

        public double GetResult(IChromosome chromosome) {
            //var index = Indexes[chromosome];
            var index = Chromosomes.First(x => x.Value == chromosome).Key;

            return Enumerable.Range(0, Size + 1).Sum(i => Result[index, i]);
        }

        internal void Reset() {
            Chromosomes = new Dictionary<int, IChromosome>();
            Result = null;
        }
    }
}
