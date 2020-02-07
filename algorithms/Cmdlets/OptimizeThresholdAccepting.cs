using FE640.Heuristics;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "ThresholdAccepting")]
    public class OptimizeThresholdAccepting : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> IterationsPerThreshold { get; set; }

        [Parameter]
        public List<double> Thresholds { get; set; }

        public OptimizeThresholdAccepting()
        {
            this.IterationsPerThreshold = null;
            this.Thresholds = null;
        }

        protected override Heuristic CreateHeuristic()
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
            if (this.TargetHarvestWeights != null)
            {
                acceptor.TargetHarvestWeights = this.TargetHarvestWeights;
            }
            if (this.Thresholds != null)
            {
                acceptor.Thresholds.Clear();
                acceptor.Thresholds.AddRange(this.Thresholds);
            }
            return acceptor;
        }
    }
}
