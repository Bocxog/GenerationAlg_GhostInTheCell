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
        protected int[,] Result;
        protected int Size;
        private FightExecuter FightExecuter;

        public ResultDictionary(FightExecuter fightExecuter) {
            Chromosomes = new Dictionary<int, IChromosome>();
            this.FightExecuter = fightExecuter;
        }

        public bool IsReady() {
            return Result != null;
        }

        public void EvalResults(GeneticAlgorithm ga) {
            Size = ga.Population.CurrentGeneration.Chromosomes.Count;
            Result = new int[Size, Size];

            var index = 0;
            foreach(var chromosome in ga.Population.CurrentGeneration.Chromosomes) {
                //Indexes[chromosome] = index;
                Chromosomes[index] = chromosome;
                index++;
            }
            for (int i = 0; i < Size; i++) {
                for (int j = i + 1; j < Size; j++) {
                    var fightResult = GetFightResult(Chromosomes[i], Chromosomes[j]);
                    Result[i, j] = fightResult;
                    Result[j, i] = -fightResult;
                }
            }
        }

        private int GetFightResult(IChromosome chromosome1, IChromosome chromosome2) {
            return FightExecuter.GetCompetitionResult(chromosome1, chromosome2);
        }

        public double GetResult(IChromosome chromosome) {
            //var index = Indexes[chromosome];
            var index = Chromosomes.First(x => x.Value == chromosome).Key;

            return Enumerable.Range(0, Size).Sum(i => Result[index, i]);
        }

        internal void Reset() {
            Chromosomes = new Dictionary<int, IChromosome>();
            Result = null;
        }
    }
}
