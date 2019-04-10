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
            protected readonly double[] values;
            protected readonly string formattedValue;
            public AlgorithmConfig(IChromosome chromosome) {
                if (!(chromosome is FloatingPointChromosome floatPoints)) {
                    throw new NotImplementedException("Chromosome type is not supported: " + chromosome.GetType());
                }

                var floatValues = floatPoints.ToFloatingPoints();
                values = new double[floatValues.Length];
                for (int i = 0; i < floatValues.Length; i++)
                    values[i] = floatValues[i];

                formattedValue = string.Join("|", values.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            }

            public override string ToString() {
                return formattedValue;
                //return base.ToString();
            }
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
