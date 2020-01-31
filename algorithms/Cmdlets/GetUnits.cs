using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Units")]
    public class GetUnits : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public string UnitXlsx { get; set; }

        protected override void ProcessRecord()
        {
            this.WriteObject(new HarvestUnits(this.UnitXlsx));
        }
    }
}
