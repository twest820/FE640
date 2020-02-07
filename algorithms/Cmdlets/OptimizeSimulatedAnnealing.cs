using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "SimulatedAnnealing")]
    public class OptimizeSimulatedAnnealing : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> Alpha { get; set; }

        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> FinalTemperature { get; set; }
        
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> InitialTemperature { get; set; }
        
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> IterationsPerTemperature { get; set; }

        public OptimizeSimulatedAnnealing()
        {
            this.Alpha = null;
            this.FinalTemperature = null;
            this.InitialTemperature = null;
            this.IterationsPerTemperature = null;
        }

        protected override Heuristic CreateHeuristic()
        {
            SimulatedAnnealing annealer = new SimulatedAnnealing(this.Units);
            if (this.Alpha.HasValue)
            {
                annealer.Alpha = this.Alpha.Value;
            }
            if (this.FinalTemperature.HasValue)
            {
                annealer.FinalTemperature = this.FinalTemperature.Value;
            }
            if (this.InitialTemperature.HasValue)
            {
                annealer.InitialTemperature = this.InitialTemperature.Value;
            }
            if (this.IterationsPerTemperature.HasValue)
            {
                annealer.IterationsPerTemperature = this.IterationsPerTemperature.Value;
            }
            if (this.TargetHarvestPerPeriod.HasValue)
            {
                annealer.TargetHarvestPerPeriod = this.TargetHarvestPerPeriod.Value;
            }
            if (this.TargetHarvestWeights != null)
            {
                annealer.TargetHarvestWeights = this.TargetHarvestWeights;
            }
            return annealer;
        }
    }
}
