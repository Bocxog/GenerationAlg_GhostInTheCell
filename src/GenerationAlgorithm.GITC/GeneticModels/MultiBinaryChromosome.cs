using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Extensions.Multiple;

namespace GenerationAlgorithm.GITC {
    public class MultiBinaryChromosome : MultipleChromosome, IBinaryChromosome {
        public MultiBinaryChromosome(IList<IChromosome> chromosomes) : base(chromosomes) { }
        public void FlipGene(int index) {
            var value = (bool)GetGene(index).Value;
            ReplaceGene(index, new Gene(value ? 1 : 0));
        }

        public override IChromosome CreateNew() {
            return new MultiBinaryChromosome(Chromosomes.Select(c => c.CreateNew()).ToList());
        }
    }
}
