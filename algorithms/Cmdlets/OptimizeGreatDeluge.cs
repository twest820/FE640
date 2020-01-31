using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "GreatDeluge")]
    public class OptimizeGreatDeluge : OptimizeCmdlet
    {
        [Parameter]
        public Nullable<double> InitialWaterLevel { get; set; }
        [Parameter]
        public Nullable<double> RainRate { get; set; }
        [Parameter]
        public Nullable<int> StopAfter { get; set; }

        public OptimizeGreatDeluge()
        {
            this.InitialWaterLevel = null;
            this.RainRate = null;
            this.StopAfter = null;
        }

        protected override Heuristic CreateHeuristic()
        {
            GreatDeluge deluge = new GreatDeluge(this.Units);
            if (this.InitialWaterLevel.HasValue)
            {
                deluge.InitialWaterLevel = this.InitialWaterLevel.Value;
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
