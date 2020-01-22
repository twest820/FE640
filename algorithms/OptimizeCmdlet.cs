using System;
using System.Management.Automation;

namespace FE640
{
    public class OptimizeCmdlet : Cmdlet
    {
        [Parameter]
        public int BestOf { get; set; }
        [Parameter]
        public Nullable<float> TargetHarvestPerPeriod { get; set; }
        [Parameter(Mandatory = true)]
        public HarvestUnits Units { get; set; }

        public OptimizeCmdlet()
        {
            this.BestOf = 1;
            this.TargetHarvestPerPeriod = null;
        }

        protected void WriteVerbose(string format, params object[] args)
        {
            base.WriteVerbose(String.Format(format, args));
        }
    }
}
