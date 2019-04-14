using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using AlgorithmPowerChecker;
using Serilog;
using System.Globalization;

namespace GenerationAlgorithm.GITC {
    public class FightExecuter {
        class AlgorithmConfig {
            protected readonly string formattedValue;
            public AlgorithmConfig(IChromosome chromosome) {
                formattedValue = chromosome.GetTransferedString();
            }

            public override string ToString() => formattedValue;
        }


        internal int GetCompetitionResult(IChromosome chromosome1, IChromosome chromosome2, int count = 1) {
            var config1 = new AlgorithmConfig(chromosome1);
            var config2 = new AlgorithmConfig(chromosome2);

            var configString1 = config1.ToString();
            var configString2 = config2.ToString();

            var result = PowerChecker.GetPowerResult(configString1, configString2, count);
            //throw new NotImplementedException();
            Log.Information("Result: {@result}. Between: '{@chromosome1}' & '{@chromosome2}'", result, configString1, configString2);

            return result;
        }
    }
}
