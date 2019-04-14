using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Extensions.Multiple;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerationAlgorithm.GITC
{
    public static class ChromosomeExtension
    {
        public static string GetTransferedString(this IChromosome chromosome) {
            if (!(chromosome is MultipleChromosome multipleChromosome))
            {
                throw new NotImplementedException("Chromosome type is not supported: " + chromosome.GetType());
            }

            var phenotype = multipleChromosome.Chromosomes.Select(x => (x as DecimalChromosome).ToDecimal());
            return string.Join("|", phenotype.Select(x => x.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
