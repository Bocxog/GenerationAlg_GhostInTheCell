using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace GenerationAlgorithm.GITC {
    public class DecimalChromosome : IntegerChromosome
    {
        public short Decimals { get; set; }
        public readonly int Shift;
        public readonly int m_minValue;
        public readonly int m_maxValue;
        public readonly short m_decimals;
        public int DecimalTens { get; set; }
        public DecimalChromosome(int min, int max, short decimals) : base(0, (max - min) * (int)Math.Pow(10, decimals))
        {
            Decimals = decimals;
            DecimalTens = (int)Math.Pow(10, decimals);
            Shift = min * DecimalTens;
        }

        public override IChromosome CreateNew()
        {
            return new DecimalChromosome(m_minValue, m_maxValue, m_decimals);
        }


        public decimal ToDecimal()
        {
            var intValue = this.ToInteger() + Shift;
            return intValue / (decimal)DecimalTens;
        }
    }
}
