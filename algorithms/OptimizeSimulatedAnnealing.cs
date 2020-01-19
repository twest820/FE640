using System;
using System.Linq;
using System.Management.Automation;

namespace FE640
{
    [Cmdlet(VerbsCommon.Optimize, "SimulatedAnnealing")]
    public class OptimizeSimulatedAnnealing : OptimizeCmdlet
    {
        [Parameter]
        public Nullable<float> Alpha { get; set; }
        [Parameter]
        public Nullable<float> FinalTemperature { get; set; }
        [Parameter]
        public Nullable<float> InitialTemperature { get; set; }
        [Parameter]
        public Nullable<int> IterationsPerTemperature { get; set; }

        public OptimizeSimulatedAnnealing()
        {
            this.Alpha = null;
            this.FinalTemperature = null;
            this.InitialTemperature = null;
            this.IterationsPerTemperature = null;
        }

        protected override void ProcessRecord()
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
            TimeSpan annealingTime = annealer.Anneal();

            this.WriteObject(annealer);

            int movesAccepted = 0;
            int movesRejected = 0;
            float previousObjectiveFunction = annealer.ObjectiveFunctionByIteration[0];
            for (int index = 1; index < annealer.ObjectiveFunctionByIteration.Count; ++index)
            {
                float currentObjectiveFunction = annealer.ObjectiveFunctionByIteration[index];
                if (currentObjectiveFunction != previousObjectiveFunction)
                {
                    ++movesAccepted;
                }
                else
                {
                    ++movesRejected;
                }
                previousObjectiveFunction = currentObjectiveFunction;
            }

            this.WriteVerbose("T = {0:0} -> {1:0}, α = {2:0.0000}, harvest target = {3:0}", annealer.InitialTemperature, annealer.FinalTemperature, annealer.Alpha, annealer.TargetHarvestPerPeriod);
            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0} moves, {1} accepted ({2:0%}), {3} rejected ({4:0%})", totalMoves, movesAccepted, (float)movesAccepted / (float)totalMoves, movesRejected, (float)movesRejected / (float)totalMoves);
            this.WriteVerbose("Best objective function {0}.", annealer.BestObjectiveFunction);
            this.WriteVerbose("Ending objective function {0}.", annealer.ObjectiveFunctionByIteration.Last());
            float iterationsPerSecond = (float)annealer.ObjectiveFunctionByIteration.Count / (float)annealingTime.TotalSeconds;
            this.WriteVerbose("{0} iterations in {1:s\\.fff}s ({2:0.00} Miterations/s).", annealer.ObjectiveFunctionByIteration.Count, annealingTime, 1E-6F * iterationsPerSecond);
        }
    }
}
