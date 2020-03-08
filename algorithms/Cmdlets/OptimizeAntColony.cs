using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "AntColony")]
    public class OptimizeAntColony : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> Ants { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> Iterations { get; set; }

        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> MinimumPeriodVisibilityFactor { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> PheremoneEvaporationRate { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> PheremoneProportion { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> ReservedPopulationProportion { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> TrailTranspositionProbability { get; set; }

        protected override Heuristic CreateHeuristic()
        {
            AntColony colony = new AntColony(this.Units);
            if (this.Ants.HasValue)
            {
                colony.Ants = this.Ants.Value;
            }
            if (this.Iterations.HasValue)
            {
                colony.Iterations = this.Iterations.Value;
            }
            //if (this.MinimumPeriodVisibilityFactor.HasValue)
            //{
            //    colony.MinimumPeriodVisibilityFactor = this.MinimumPeriodVisibilityFactor.Value;
            //}
            if (this.PheremoneEvaporationRate.HasValue)
            {
                colony.PheremoneEvaporationRate = this.PheremoneEvaporationRate.Value;
            }
            if (this.PheremoneProportion.HasValue)
            {
                colony.PheremoneProportion = this.PheremoneProportion.Value;
            }
            if (this.ReservedPopulationProportion.HasValue)
            {
                colony.ReservedPopulationProportion = this.ReservedPopulationProportion.Value;
            }
            if (this.TrailTranspositionProbability.HasValue)
            {
                colony.TrailTranspositionProbability = this.TrailTranspositionProbability.Value;
            }
            return colony;
        }
    }
}
