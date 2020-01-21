using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace FE640
{
    [Cmdlet(VerbsCommon.Optimize, "ThresholdAccepting")]
    public class OptimizeThresholdAccepting : OptimizeCmdlet
    {
        [Parameter]
        public Nullable<int> IterationsPerThreshold { get; set; }
        [Parameter]
        public List<float> Thresholds { get; set; }

        public OptimizeThresholdAccepting()
        {
            this.IterationsPerThreshold = null;
            this.Thresholds = null;
        }

        protected override void ProcessRecord()
        {
            ThresholdAccepting acceptor = new ThresholdAccepting(this.Units);
            if (this.IterationsPerThreshold.HasValue)
            {
                acceptor.IterationsPerThreshold = this.IterationsPerThreshold.Value;
            }
            if (this.TargetHarvestPerPeriod.HasValue)
            {
                acceptor.TargetHarvestPerPeriod = this.TargetHarvestPerPeriod.Value;
            }
            if (this.Thresholds != null)
            {
                acceptor.Thresholds.Clear();
                acceptor.Thresholds.AddRange(this.Thresholds);
            }
            TimeSpan annealingTime = acceptor.Accept();

            this.WriteObject(acceptor);

            int movesAccepted = 0;
            int movesRejected = 0;
            float previousObjectiveFunction = acceptor.ObjectiveFunctionByIteration[0];
            for (int index = 1; index < acceptor.ObjectiveFunctionByIteration.Count; ++index)
            {
                float currentObjectiveFunction = acceptor.ObjectiveFunctionByIteration[index];
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

            this.WriteVerbose("threshold = {0:0.00#} -> {1:0.00#}, harvest target = {2:0}", acceptor.Thresholds.First(), acceptor.Thresholds.Last(), acceptor.TargetHarvestPerPeriod);
            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0} moves, {1} accepted ({2:0%}), {3} rejected ({4:0%})", totalMoves, movesAccepted, (float)movesAccepted / (float)totalMoves, movesRejected, (float)movesRejected / (float)totalMoves);
            this.WriteVerbose("Best objective function {0}.", acceptor.BestObjectiveFunction);
            this.WriteVerbose("Ending objective function {0}.", acceptor.ObjectiveFunctionByIteration.Last());
            float iterationsPerSecond = (float)acceptor.ObjectiveFunctionByIteration.Count / (float)annealingTime.TotalSeconds;
            this.WriteVerbose("{0} iterations in {1:s\\.fff}s ({2:0.00} Miterations/s).", acceptor.ObjectiveFunctionByIteration.Count, annealingTime, 1E-6F * iterationsPerSecond);
        }
    }
}
