using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Genetic")]
    public class OptimizeGenetic : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> EndStandardDeviation { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> MaximumGenerations { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> MutationProbability { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> PopulationSize { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> ReservedPopulationProportion { get; set; }

        protected override Heuristic CreateHeuristic()
        {
            GeneticAlgorithm genetic = new GeneticAlgorithm(this.Units);
            if (this.EndStandardDeviation.HasValue)
            {
                genetic.EndStandardDeviation = this.EndStandardDeviation.Value;
            }
            if (this.MaximumGenerations.HasValue)
            {
                genetic.MaximumGenerations = this.MaximumGenerations.Value;
            }
            if (this.MutationProbability.HasValue)
            {
                genetic.MutationProbability = this.MutationProbability.Value;
            }
            if (this.PopulationSize.HasValue)
            {
                genetic.PopulationSize = this.PopulationSize.Value;
            }
            if (this.ReservedPopulationProportion.HasValue)
            {
                genetic.ReservedPopulationProportion = this.ReservedPopulationProportion.Value;
            }
            return genetic;
        }
    }
}
