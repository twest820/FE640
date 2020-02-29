using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Harmony")]
    public class OptimizeHarmony : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> Generations { get; set; }

        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> MaximumBandwidth { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> MaximumPitchAdjustmentRate { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> MemoryRate { get; set; }

        [Parameter]
        [ValidateRange(1, Int16.MaxValue)]
        public Nullable<int> MemorySize { get; set; }

        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> MinimumBandwidth { get; set; }

        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public Nullable<double> MinimumPitchAdjustmentRate { get; set; }

        protected override Heuristic CreateHeuristic()
        {
            HarmonySearch harmony = new HarmonySearch(this.Units);
            if (this.Generations.HasValue)
            {
                harmony.Generations = this.Generations.Value;
            }
            if (this.MaximumBandwidth.HasValue)
            {
                harmony.MaximumBandwidth = this.MaximumBandwidth.Value;
            }
            if (this.MaximumPitchAdjustmentRate.HasValue)
            {
                harmony.MaximumPitchAdjustmentRate = this.MaximumPitchAdjustmentRate.Value;
            }
            if (this.MemoryRate.HasValue)
            {
                harmony.MemoryRate = this.MemoryRate.Value;
            }
            if (this.MemorySize.HasValue)
            {
                harmony.MemorySize = this.MemorySize.Value;
            }
            if (this.MinimumBandwidth.HasValue)
            {
                harmony.MinimumBandwidth = this.MinimumBandwidth.Value;
            }
            if (this.MinimumPitchAdjustmentRate.HasValue)
            {
                harmony.MinimumPitchAdjustmentRate = this.MinimumPitchAdjustmentRate.Value;
            }
            return harmony;
        }
    }
}
