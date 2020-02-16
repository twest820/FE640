using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FE640.Heuristics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FE640
{
    public class HarvestUnits
    {
        public int[,] AdjacencyByUnit { get; private set; }
        public int GreenUpInPeriods { get; private set; }
        public int[] HarvestSchedule;
        public bool HasAdjacency { get; private set; }
        public float MaximumOpeningSize { get; private set; }
        public float[,] YieldByPeriod;
        public float UnitSize { get; private set; }

        public HarvestUnits(string xlsxPath)
            : this(xlsxPath, Int32.MaxValue)
        {
        }

        /// <summary>
        /// Reads the first n data rows from the units tab of the specified .xlsx file.
        /// </summary>
        public HarvestUnits(string xlsxPath, int maximumDataRow)
        {
            this.GreenUpInPeriods = 3;
            this.HasAdjacency = false;
            this.MaximumOpeningSize = 120.0F; // ac or ha
            this.UnitSize = 30.0F; // ac or ha
            // other fields initialized when worksheet dimension has been read

            using SpreadsheetDocument unitXlsx = SpreadsheetDocument.Open(xlsxPath, false);
            string sheetID = unitXlsx.WorkbookPart.Workbook.Sheets.Elements<Sheet>().First(sheet => String.Equals(sheet.Name, "units", StringComparison.Ordinal)).Id;
            WorksheetPart worksheet = (WorksheetPart)unitXlsx.WorkbookPart.GetPartById(sheetID);

            using OpenXmlReader reader = OpenXmlReader.Create(worksheet);
            reader.Read();
            int period = -1;
            int row = -1;
            while (reader.EOF == false)
            {
                if (reader.IsStartElement)
                {
                    if (reader.LocalName == Constant.OpenXml.CellValue)
                    {
                        if (row > 0)
                        {
                            if (period > 0)
                            {
                                this.YieldByPeriod[row - 1, period] = float.Parse(reader.GetText());
                            }
                        }
                        ++period;
                        reader.Read();
                    }
                    else if (reader.LocalName == Constant.OpenXml.Row)
                    {
                        ++row;
                        if (row > maximumDataRow)
                        {
                            // for now, assume one header row
                            return;
                        }

                        period = 0;
                        reader.Read();
                    }
                    else if (reader.LocalName == Constant.OpenXml.Dimension)
                    {
                        string reference = reader.Attributes[0].Value;
                        int periods = reference[3] - 'A';
                        int units = Math.Min(maximumDataRow, Int32.Parse(reference.Substring(4)) - 1);
                        this.AdjacencyByUnit = new int[units, 4];
                        this.HarvestSchedule = new int[units];
                        this.YieldByPeriod = new float[units, periods + 1];
                        reader.Read();
                    }
                    else
                    {
                        reader.Read();
                    }
                }
                else
                {
                    reader.Read();
                }
            }
        }

        public int Count
        {
            get { return this.HarvestSchedule.Length; }
        }

        public int HarvestPeriods
        {
            get { return this.YieldByPeriod.GetLength(1) - 1; }
        }

        public OpeningSizes GetMaximumOpeningSizesByPeriod()
        {
            OpeningSizes openingSizes = new OpeningSizes(this.HarvestPeriods);
            for (int unitIndex = 0; unitIndex < this.Count; ++unitIndex)
            {
                int harvestPeriod = this.HarvestSchedule[unitIndex];
                if (harvestPeriod < 1)
                {
                    // uncut units aren't openings
                    continue;
                }

                int maxOpeningPeriod = Math.Min(harvestPeriod + this.GreenUpInPeriods, this.HarvestPeriods + 1);
                for (int openingPeriod = harvestPeriod; openingPeriod < maxOpeningPeriod; ++openingPeriod)
                {
                    float openingSize = this.GetOpeningSize(unitIndex, openingPeriod);
                    openingSizes.Max(unitIndex, openingPeriod, openingSize);
                }
            }
            return openingSizes;
        }

        public float GetOpeningSize(int unitIndex, int openingPeriod)
        {
            if (openingPeriod < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(openingPeriod));
            }
            int harvestPeriod = this.HarvestSchedule[unitIndex];
            if (harvestPeriod == 0)
            {
                // unit isn't scheduled for harvest
                return 0.0F;
            }
            if (openingPeriod < harvestPeriod)
            {
                // unit hasn't (yet) been harvested
                return 0.0F;
            }
            int greenUpPeriod = harvestPeriod + this.GreenUpInPeriods;
            if (harvestPeriod > greenUpPeriod)
            {
                // unit is free to grow and no longer contributes to openings
                return 0.0F;
            }

            bool[] openingStatusEvaluated = new bool[this.Count];
            openingStatusEvaluated[unitIndex] = true;
            float openingSize = this.UnitSize;
            openingSize += this.GetAdjacentOpeningSize(unitIndex, openingPeriod, openingStatusEvaluated);
            return openingSize;
        }

        private float GetAdjacentOpeningSize(int unitIndex, int openingPeriod, bool[] openingStatusEvaluated)
        {
            int neighbors = this.AdjacencyByUnit.GetLength(1);
            float openingSize = 0.0F;
            for (int adjacencyIndex = 0; adjacencyIndex < neighbors; ++adjacencyIndex)
            {
                int adjacentUnitIndex = this.AdjacencyByUnit[unitIndex, adjacencyIndex];
                if (adjacentUnitIndex < 0)
                {
                    break;
                }
                if (openingStatusEvaluated[adjacentUnitIndex])
                {
                    continue;
                }

                int adjacentUnitHarvestPeriod = this.HarvestSchedule[adjacentUnitIndex];
                openingStatusEvaluated[adjacentUnitIndex] = true;
                if (adjacentUnitHarvestPeriod < 1)
                {
                    // neighboring unit isn't scheduled for harvest
                    continue;
                }
                if ((adjacentUnitHarvestPeriod <= openingPeriod) && (adjacentUnitHarvestPeriod + this.GreenUpInPeriods >= openingPeriod))
                {
                    // add this neighboring unit to the opening
                    openingSize += this.UnitSize;
                    // recurse to units adjacent to this neighboring unit
                    openingSize += this.GetAdjacentOpeningSize(adjacentUnitIndex, openingPeriod, openingStatusEvaluated);
                }
            }

            return openingSize;
        }

        public void SetBestSchedule(Heuristic heuristic)
        {
            if (heuristic.BestHarvestPeriods.Length != this.HarvestSchedule.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(heuristic));
            }

            Array.Copy(heuristic.BestHarvestPeriods, 0, this.HarvestSchedule, 0, this.HarvestSchedule.Length);
        }

        public void SetCurrentSchedule(Heuristic heuristic)
        {
            if (heuristic.CurrentHarvestPeriods.Length != this.HarvestSchedule.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(heuristic));
            }

            Array.Copy(heuristic.CurrentHarvestPeriods, 0, this.HarvestSchedule, 0, this.HarvestSchedule.Length);
        }

        public void SetLoopSchedule(int loopRate)
        {
            if (loopRate < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(loopRate));
            }

            for (int unitIndex = 0; unitIndex < this.Count; ++unitIndex)
            {
                this.HarvestSchedule[unitIndex] = 1 + (unitIndex / loopRate) % this.HarvestPeriods;
            }
        }

        public void SetRandomSchedule()
        {
            Random random = new Random();
            for (int unitIndex = 0; unitIndex < this.Count; ++unitIndex)
            {
                this.HarvestSchedule[unitIndex] = random.Next(1, this.YieldByPeriod.GetLength(1));
            }
        }

        public void SetRandomSchedule(IList<double> harvestProbabilityByPeriod)
        {
            if ((harvestProbabilityByPeriod.Count != this.HarvestPeriods) || (harvestProbabilityByPeriod.Sum() != 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(harvestProbabilityByPeriod));
            }

            Random random = new Random();
            for (int unitIndex = 0; unitIndex < this.Count; ++unitIndex)
            {
                double probability = random.NextDouble();
                double cumulativeProbability = 0.0;
                int harvestPeriod = 1;
                for (int periodIndex = 0; periodIndex < harvestProbabilityByPeriod.Count; ++periodIndex)
                {
                    cumulativeProbability += harvestProbabilityByPeriod[periodIndex];
                    if (probability <= cumulativeProbability)
                    {
                        break;
                    }
                    ++harvestPeriod;
                }
                this.HarvestSchedule[unitIndex] = harvestPeriod;
            }
        }

        /// <summary>
        /// Sets adjacency index for units in a rectangular grid of dimension unitsPerRow x Math.Ceiling(this.Count / unitsPerRow). Last row may be partial.
        /// </summary>
        /// <param name="unitsPerRow">Number of units per row.</param>
        public void SetRectangularAdjacency(int unitsPerRow)
        {
            int columnIndex = 0;
            int rowIndex = 0;
            for (int unitIndex = 0; unitIndex < this.Count; ++unitIndex)
            {
                int[] adjacentUnits = new int[4] { -1, -1, -1, -1 };
                int adjacentUnitCount = 0;
                // unit to left/west
                if (columnIndex > 0)
                {
                    adjacentUnits[adjacentUnitCount] = unitIndex - 1;
                    ++adjacentUnitCount;
                }
                // unit below/to south
                if (rowIndex > 0)
                {
                    adjacentUnits[adjacentUnitCount] = unitIndex - unitsPerRow;
                    ++adjacentUnitCount;
                }
                // unit to right/east
                if ((columnIndex + 1 < unitsPerRow) && (unitIndex < this.Count - 1))
                {
                    adjacentUnits[adjacentUnitCount] = unitIndex + 1;
                    ++adjacentUnitCount;
                }
                // unit above/to north
                int candidateNorthIndex = unitIndex + unitsPerRow;
                if (candidateNorthIndex < this.Count)
                {
                    adjacentUnits[adjacentUnitCount] = candidateNorthIndex;
                }

                this.AdjacencyByUnit[unitIndex, 0] = adjacentUnits[0];
                this.AdjacencyByUnit[unitIndex, 1] = adjacentUnits[1];
                this.AdjacencyByUnit[unitIndex, 2] = adjacentUnits[2];
                this.AdjacencyByUnit[unitIndex, 3] = adjacentUnits[3];

                ++columnIndex;
                if (columnIndex >= unitsPerRow)
                {
                    columnIndex = 0;
                    ++rowIndex;
                }
            }

            this.HasAdjacency = true;
        }
    }
}
