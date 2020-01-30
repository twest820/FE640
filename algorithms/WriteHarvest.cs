using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace FE640
{
    [Cmdlet(VerbsCommunications.Write, "Harvest")]
    public class WriteHarvest : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public string CsvFile;
        [Parameter(Mandatory = true)]
        public List<Heuristic> Heuristics { get; set; }

        protected override void ProcessRecord()
        {
            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            StringBuilder line = new StringBuilder("unit");
            int maxPeriod = 0;
            for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
            {
                line.AppendFormat(CultureInfo.InvariantCulture, ",SA{0}", heuristicIndex);

                Heuristic heuristic = this.Heuristics[heuristicIndex];
                maxPeriod = Math.Max(maxPeriod, heuristic.BestHarvestByPeriod.Length);
            }
            writer.WriteLine(line);

            for (int period = 0; period < maxPeriod; ++period)
            {
                line.Clear();
                line.Append(period);

                for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
                {
                    Heuristic heuristic = this.Heuristics[heuristicIndex];
                    if (heuristic.ObjectiveFunctionByIteration.Count > period)
                    {
                        double objectiveFunction = heuristic.BestHarvestByPeriod[period];
                        line.Append(",");
                        line.Append(objectiveFunction.ToString(CultureInfo.InvariantCulture));
                    }
                }

                writer.WriteLine(line);
            }
        }
    }
}
