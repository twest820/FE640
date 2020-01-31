using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "RecordTravel")]
    public class OptimizeRecordTravel : OptimizeCmdlet
    {
        [Parameter]
        public Nullable<double> Deviation { get; set; }
        [Parameter]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeRecordTravel()
        {
            this.Deviation = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic()
        {
            RecordToRecordTravel recordTravel = new RecordToRecordTravel(this.Units);
            if (this.Deviation.HasValue)
            {
                recordTravel.Deviation = this.Deviation.Value;
            }
            if (this.StopAfter.HasValue)
            {
                recordTravel.StopAfter = this.StopAfter.Value;
            }
            return recordTravel;
        }
    }
}
