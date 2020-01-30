using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace FE640
{
    [Cmdlet(VerbsCommon.Optimize, "SimulatedAnnealing")]
    public class OptimizeSimulatedAnnealing : OptimizeCmdlet
    {
        [Parameter]
        public Nullable<double> Alpha { get; set; }
        [Parameter]
        public Nullable<double> FinalTemperature { get; set; }
        [Parameter]
        public Nullable<double> InitialTemperature { get; set; }
        [Parameter]
        public Nullable<int> IterationsPerTemperature { get; set; }

        public OptimizeSimulatedAnnealing()
        {
            this.Alpha = null;
            this.FinalTemperature = null;
            this.InitialTemperature = null;
            this.IterationsPerTemperature = null;
        }

        private SimulatedAnnealing CreateAnnealer()
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

        protected override void ProcessRecord()
        {
            SimulatedAnnealing bestAnnealer = null;
            List<double> objectiveFunctionValues = new List<double>();
            TimeSpan annealingTime;
            for (int iteration = 0; iteration < this.BestOf; ++iteration)
            {
                SimulatedAnnealing currentAnnealer = this.CreateAnnealer();
                annealingTime = currentAnnealer.Anneal();
                objectiveFunctionValues.Add(currentAnnealer.BestObjectiveFunction);

                if ((bestAnnealer == null) || (currentAnnealer.BestObjectiveFunction < bestAnnealer.BestObjectiveFunction))
                {
                    bestAnnealer = currentAnnealer;
                }
            }

            this.WriteObject(bestAnnealer);
            if (this.BestOf > 1)
            {
                this.WriteObject(objectiveFunctionValues);
            }

            int movesAccepted = 0;
            int movesRejected = 0;
            double previousObjectiveFunction = bestAnnealer.ObjectiveFunctionByIteration[0];
            for (int index = 1; index < bestAnnealer.ObjectiveFunctionByIteration.Count; ++index)
            {
                double currentObjectiveFunction = bestAnnealer.ObjectiveFunctionByIteration[index];
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

            this.WriteVerbose("T = {0:0} -> {1:0}, α = {2:0.0000}, harvest target = {3:0}", bestAnnealer.InitialTemperature, bestAnnealer.FinalTemperature, bestAnnealer.Alpha, bestAnnealer.TargetHarvestPerPeriod);
            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0} moves, {1} accepted ({2:0%}), {3} rejected ({4:0%})", totalMoves, movesAccepted, (double)movesAccepted / (double)totalMoves, movesRejected, (double)movesRejected / (double)totalMoves);
            this.WriteVerbose("Best objective function {0}.", bestAnnealer.BestObjectiveFunction);
            this.WriteVerbose("Ending objective function {0}.", bestAnnealer.ObjectiveFunctionByIteration.Last());
            double iterationsPerSecond = (double)bestAnnealer.ObjectiveFunctionByIteration.Count / (double)annealingTime.TotalSeconds;
            this.WriteVerbose("{0} iterations in {1:s\\.fff}s ({2:0.00} Miterations/s).", bestAnnealer.ObjectiveFunctionByIteration.Count, annealingTime, 1E-6F * iterationsPerSecond);
        }
    }
}
