using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Units")]
    public class GetUnits : Cmdlet
    {
        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> Units { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string UnitXlsx { get; set; }

        protected override void ProcessRecord()
        {
            HarvestUnits units;
            if (this.Units.HasValue)
            {
                units = new HarvestUnits(this.UnitXlsx, this.Units.Value);
            }
            else
            {
                units = new HarvestUnits(this.UnitXlsx);
            }
            this.WriteObject(units);
        }
    }
}
