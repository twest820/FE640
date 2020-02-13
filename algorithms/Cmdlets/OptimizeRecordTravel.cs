using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "RecordTravel")]
    public class OptimizeRecordTravel : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> Deviation { get; set; }
        
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> InfeasibilityPenalty { get; set; }

        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public int MaximumInfeasibleUnits { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeRecordTravel()
        {
            this.Deviation = null;
            this.MaximumInfeasibleUnits = 0;
            this.InfeasibilityPenalty = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic()
        {
            RecordToRecordTravel recordTravel = new RecordToRecordTravel(this.Units)
            {
                MaximumInfeasibleUnits = this.MaximumInfeasibleUnits
            };
            if (this.Deviation.HasValue)
            {
                recordTravel.Deviation = this.Deviation.Value;
            }
            if (this.InfeasibilityPenalty.HasValue)
            {
                recordTravel.InfeasibilityPenalty = this.InfeasibilityPenalty.Value;
            }
            if (this.StopAfter.HasValue)
            {
                recordTravel.StopAfter = this.StopAfter.Value;
            }
            return recordTravel;
        }
    }
}
