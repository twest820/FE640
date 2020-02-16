using System.IO;
using System.Management.Automation;
using System.Text;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Units")]
    public class WriteUnits : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public HarvestUnits Units { get; set; }

        protected override void ProcessRecord()
        {
            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            writer.WriteLine("unit,harvest period,adjacent W,adjacent S,adjacent E,adjacent N");
            StringBuilder line = new StringBuilder();
            for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
            {
                line.Clear();
                line.Append(unitIndex);
                line.Append(",");
                line.Append(this.Units.HarvestSchedule[unitIndex]);
                line.Append(",");
                line.Append(this.Units.AdjacencyByUnit[unitIndex, 0]);
                line.Append(",");
                line.Append(this.Units.AdjacencyByUnit[unitIndex, 1]);
                line.Append(",");
                line.Append(this.Units.AdjacencyByUnit[unitIndex, 2]);
                line.Append(",");
                line.Append(this.Units.AdjacencyByUnit[unitIndex, 3]);
                writer.WriteLine(line);
            }
        }
    }
}
