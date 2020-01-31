using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Linq;

namespace FE640
{
    public class HarvestUnits
    {
        public int[] HarvestPeriods;
        public float[,] YieldByPeriod;

        public HarvestUnits(string xlsxPath)
        {
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
                        period = 0;
                        reader.Read();
                    }
                    else if (reader.LocalName == Constant.OpenXml.Dimension)
                    {
                        string reference = reader.Attributes[0].Value;
                        int periods = reference[3] - 'A';
                        int units = Int32.Parse(reference.Substring(4)) - 1;
                        this.HarvestPeriods = new int[units];
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
            get { return this.HarvestPeriods.Length; }
        }

        public void SetRandomSchedule()
        {
            Random random = new Random();
            for (int unitIndex = 0; unitIndex < this.Count; ++unitIndex)
            {
                this.HarvestPeriods[unitIndex] = random.Next(1, this.YieldByPeriod.GetLength(1));
            }
        }
    }
}
