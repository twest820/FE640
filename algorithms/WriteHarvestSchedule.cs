using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace FE640
{
    [Cmdlet(VerbsCommunications.Write, "HarvestSchedule")]
    public class WriteHarvestSchedule : Cmdlet
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
            int maxUnit = 0;
            for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
            {
                line.AppendFormat(CultureInfo.InvariantCulture, ",SA{0}", heuristicIndex);

                Heuristic heuristic = this.Heuristics[heuristicIndex];
                maxUnit = Math.Max(maxUnit, heuristic.BestHarvestPeriods.Length);
            }
            writer.WriteLine(line);

            for (int unitIndex = 0; unitIndex < maxUnit; ++unitIndex)
            {
                line.Clear();
                line.Append(unitIndex);

                for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
                {
                    Heuristic heuristic = this.Heuristics[heuristicIndex];
                    if (heuristic.ObjectiveFunctionByIteration.Count > unitIndex)
                    {
                        float objectiveFunction = heuristic.BestHarvestPeriods[unitIndex];
                        line.Append(",");
                        line.Append(objectiveFunction.ToString(CultureInfo.InvariantCulture));
                    }
                }

                writer.WriteLine(line);
            }
        }
    }
}
