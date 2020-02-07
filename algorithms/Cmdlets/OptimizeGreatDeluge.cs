using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "GreatDeluge")]
    public class OptimizeGreatDeluge : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> InitialWaterLevelMultiplier { get; set; }
        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> RainRate { get; set; }
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeGreatDeluge()
        {
            this.InitialWaterLevelMultiplier = null;
            this.RainRate = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic()
        {
            GreatDeluge deluge = new GreatDeluge(this.Units);
            if (this.InitialWaterLevelMultiplier.HasValue)
            {
                deluge.InitialWaterLevelMultiplier = this.InitialWaterLevelMultiplier.Value;
            }
            if (this.RainRate.HasValue)
            {
                deluge.RainRate = this.RainRate.Value;
            }
            if (this.StopAfter.HasValue)
            {
                deluge.StopAfter = this.StopAfter.Value;
            }
            return deluge;
        }
    }
}
