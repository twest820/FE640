using System;
using System.Collections.Generic;

namespace FE640
{
    public class Heuristic
    {
        private readonly Random pseudorandom;
        private readonly byte[] pseudorandomBytes;
        private int pseudorandomByteIndex;
        private float targetHarvestPerPeriod;

        protected HarvestUnits Units { get; private set; }

        public float BestObjectiveFunction { get; protected set; }
        public float[] BestHarvestByPeriod { get; protected set; }
        public int[] BestHarvestPeriods { get; protected set; }
        public float[] CurrentHarvestByPeriod { get; protected set; }
        public int[] CurrentHarvestPeriods { get; protected set; }
        public List<float> ObjectiveFunctionByIteration { get; protected set; }

        protected Heuristic(HarvestUnits units)
        {
            this.pseudorandom = new Random();
            this.pseudorandomBytes = new byte[1024];
            pseudorandom.NextBytes(pseudorandomBytes);
            this.pseudorandomByteIndex = 0;

            this.BestHarvestPeriods = new int[units.Count];
            Array.Copy(units.HarvestPeriods, 0, this.BestHarvestPeriods, 0, units.Count);
            this.CurrentHarvestPeriods = new int[units.Count];
            Array.Copy(units.HarvestPeriods, 0, this.CurrentHarvestPeriods, 0, units.Count);
            this.Units = units;

            // units default to harvest period 0, which is treated as no cut
            // Cut periods are therefore 1...n and require array allocation accordingly. This simplifies the inner annealing loop
            // as no special logic is needed for handling a special no cut value such as -1.
            int periods = this.Units.YieldByPeriod.GetLength(1) - 1;
            this.BestHarvestByPeriod = new float[periods + 1];
            this.CurrentHarvestByPeriod = new float[periods + 1];

            this.targetHarvestPerPeriod = this.GetDefaultTargetHarvestPerPeriod();
            this.RecalculateHarvestVolumes();
            this.BestObjectiveFunction = this.RecalculateObjectiveFunction();
        }

        public float TargetHarvestPerPeriod
        {
            get
            {
                return this.targetHarvestPerPeriod;
            }
            set
            {
                this.targetHarvestPerPeriod = value;
                this.BestObjectiveFunction = this.RecalculateObjectiveFunction();
                this.ObjectiveFunctionByIteration[0] = this.BestObjectiveFunction;
            }
        }

        protected float GetDefaultTargetHarvestPerPeriod()
        {
            int periods = this.Units.YieldByPeriod.GetLength(1) - 1;
            float maximumYield = 0.0F;
            for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
            {
                maximumYield += this.Units.YieldByPeriod[unitIndex, periods];
            }

            return maximumYield / (float)periods;
        }

        protected float GetPseudorandomByteAsFloat()
        {
            float byteAsFloat = this.pseudorandomBytes[this.pseudorandomByteIndex];
            ++this.pseudorandomByteIndex;

            // if the last available byte was used, ensure more bytes are available
            if (this.pseudorandomByteIndex >= this.pseudorandomBytes.Length)
            {
                this.pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            return byteAsFloat;
        }

        protected float GetTwoPseudorandomBytesAsFloat()
        {
            // ensure two bytes are available
            if (this.pseudorandomByteIndex > this.pseudorandomBytes.Length - 2)
            {
                this.pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            // get bytes
            float bytesAsFloat = (float)BitConverter.ToUInt16(this.pseudorandomBytes, this.pseudorandomByteIndex);
            this.pseudorandomByteIndex += 2;

            // if the last available byte was used, ensure more bytes are available
            if (this.pseudorandomByteIndex > this.pseudorandomBytes.Length - 1)
            {
                this.pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            return bytesAsFloat;
        }

        public void RecalculateHarvestVolumes()
        {
            // recalculate harvest volumes
            Array.Clear(this.CurrentHarvestByPeriod, 0, this.CurrentHarvestByPeriod.Length);
            for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
            {
                int periodIndex = this.CurrentHarvestPeriods[unitIndex];
                if (periodIndex > -1)
                {
                    this.CurrentHarvestByPeriod[periodIndex] += this.Units.YieldByPeriod[unitIndex, periodIndex];
                }
            }

            Array.Copy(this.CurrentHarvestByPeriod, 0, this.BestHarvestByPeriod, 0, this.CurrentHarvestByPeriod.Length);
        }

        public float RecalculateObjectiveFunction()
        {
            // find objective function value
            float objectiveFunction = 0.0F;
            for (int periodIndex = 1; periodIndex < this.CurrentHarvestByPeriod.Length; ++periodIndex)
            {
                float harvest = this.CurrentHarvestByPeriod[periodIndex];
                float differenceFromTarget = this.TargetHarvestPerPeriod - harvest;
                objectiveFunction += differenceFromTarget * differenceFromTarget;
            }
            return objectiveFunction;
        }
    }
}
