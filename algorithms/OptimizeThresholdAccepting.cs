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

        private ThresholdAccepting CreateAcceptor()
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
            return acceptor;
        }

        protected override void ProcessRecord()
        {
            ThresholdAccepting bestAcceptor = null;
            TimeSpan acceptanceTime;
            List<float> objectiveFunctionValues = new List<float>();
            for (int iteration = 0; iteration < this.BestOf; ++iteration)
            {
                ThresholdAccepting currentAcceptor = this.CreateAcceptor();
                acceptanceTime = currentAcceptor.Accept();
                objectiveFunctionValues.Add(currentAcceptor.BestObjectiveFunction);

                if ((bestAcceptor == null) || (currentAcceptor.BestObjectiveFunction < bestAcceptor.BestObjectiveFunction))
                {
                    bestAcceptor = currentAcceptor;
                }
            }

            this.WriteObject(bestAcceptor);
            if (this.BestOf > 1)
            {
                this.WriteObject(objectiveFunctionValues);
            }

            int movesAccepted = 0;
            int movesRejected = 0;
            float previousObjectiveFunction = bestAcceptor.ObjectiveFunctionByIteration[0];
            for (int index = 1; index < bestAcceptor.ObjectiveFunctionByIteration.Count; ++index)
            {
                float currentObjectiveFunction = bestAcceptor.ObjectiveFunctionByIteration[index];
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

            this.WriteVerbose("threshold = {0:0.00#} -> {1:0.00#}, harvest target = {2:0}", bestAcceptor.Thresholds.First(), bestAcceptor.Thresholds.Last(), bestAcceptor.TargetHarvestPerPeriod);
            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0} moves, {1} accepted ({2:0%}), {3} rejected ({4:0%})", totalMoves, movesAccepted, (float)movesAccepted / (float)totalMoves, movesRejected, (float)movesRejected / (float)totalMoves);
            this.WriteVerbose("Best objective function {0}.", bestAcceptor.BestObjectiveFunction);
            this.WriteVerbose("Ending objective function {0}.", bestAcceptor.ObjectiveFunctionByIteration.Last());
            float iterationsPerSecond = (float)bestAcceptor.ObjectiveFunctionByIteration.Count / (float)acceptanceTime.TotalSeconds;
            this.WriteVerbose("{0} iterations in {1:s\\.fff}s ({2:0.00} Miterations/s).", bestAcceptor.ObjectiveFunctionByIteration.Count, acceptanceTime, 1E-6F * iterationsPerSecond);
        }
    }
}
