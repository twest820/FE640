using System;
using System.Collections.Generic;
using System.Text;

namespace FE640
{
    public class OpeningSizes
    {
        public float[] MaximumOpeningSizeByPeriod { get; private set; }
        public int[] MaximumOpeningUnitByPeriod { get; private set; }

        public OpeningSizes(int periods)
        {
            this.MaximumOpeningSizeByPeriod = new float[periods + 1];
            this.MaximumOpeningUnitByPeriod = new int[periods + 1];
        }

        public void Max(int unitIndex, int harvestPeriod, float openingSize)
        {
            if (harvestPeriod > this.MaximumOpeningSizeByPeriod.Length - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(harvestPeriod));
            }

            if (this.MaximumOpeningSizeByPeriod[harvestPeriod] < openingSize)
            {
                this.MaximumOpeningSizeByPeriod[harvestPeriod] = openingSize;
                this.MaximumOpeningUnitByPeriod[harvestPeriod] = unitIndex;
            }
        }

        public override string ToString()
        {
            StringBuilder csvTable = new StringBuilder();
            for (int periodIndex = 0; periodIndex < this.MaximumOpeningSizeByPeriod.Length; ++periodIndex)
            {
                csvTable.AppendLine(this.MaximumOpeningUnitByPeriod[periodIndex].ToString() + "," + this.MaximumOpeningSizeByPeriod[periodIndex].ToString());
            }
            return csvTable.ToString();
        }
    }
}
