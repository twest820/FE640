using System;
using System.Collections.Generic;
using System.Linq;

namespace FE640.Heuristics
{
    public abstract class Heuristic
    {
        private readonly Random pseudorandom;
        private readonly byte[] pseudorandomBytes;
        private int pseudorandomByteIndex;
        private double targetHarvestPerPeriod;
        private double[] targetHarvestWeights;

        protected HarvestUnits Units { get; private set; }

        public double BestObjectiveFunction { get; protected set; }
        public double[] BestHarvestByPeriod { get; protected set; }
        public int[] BestHarvestPeriods { get; protected set; }
        public double[] CurrentHarvestByPeriod { get; protected set; }
        public int[] CurrentHarvestPeriods { get; protected set; }
        public List<double> ObjectiveFunctionByIteration { get; protected set; }

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
            this.BestHarvestByPeriod = new double[periods + 1];
            this.CurrentHarvestByPeriod = new double[periods + 1];
            this.targetHarvestWeights = Enumerable.Repeat(1.0, periods + 1).ToArray();

            this.targetHarvestPerPeriod = this.GetDefaultTargetHarvestPerPeriod();
            this.RecalculateHarvestVolumes();
            this.BestObjectiveFunction = this.RecalculateObjectiveFunction();
        }

        public double TargetHarvestPerPeriod
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

        public double[] TargetHarvestWeights 
        { 
            get
            {
                return this.targetHarvestWeights;
            }
            set
            {
                this.targetHarvestWeights = value;
                this.BestObjectiveFunction = this.RecalculateObjectiveFunction();
                this.ObjectiveFunctionByIteration[0] = this.BestObjectiveFunction;
            }
        }

        protected double GetDefaultTargetHarvestPerPeriod()
        {
            int periods = this.Units.YieldByPeriod.GetLength(1) - 1;
            double maximumYield = 0.0F;
            for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
            {
                maximumYield += this.Units.YieldByPeriod[unitIndex, periods];
            }

            return 0.6 * maximumYield / (double)periods;
        }

        protected double GetPseudorandomByteAsFloat()
        {
            double byteAsFloat = this.pseudorandomBytes[this.pseudorandomByteIndex];
            ++this.pseudorandomByteIndex;

            // if the last available byte was used, ensure more bytes are available
            if (this.pseudorandomByteIndex >= this.pseudorandomBytes.Length)
            {
                this.pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            return byteAsFloat;
        }

        protected double GetTwoPseudorandomBytesAsFloat()
        {
            // ensure two bytes are available
            if (this.pseudorandomByteIndex > this.pseudorandomBytes.Length - 2)
            {
                this.pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            // get bytes
            double bytesAsFloat = (double)BitConverter.ToUInt16(this.pseudorandomBytes, this.pseudorandomByteIndex);
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

        public double RecalculateObjectiveFunction()
        {
            // find objective function value
            double objectiveFunction = 0.0F;
            for (int periodIndex = 1; periodIndex < this.CurrentHarvestByPeriod.Length; ++periodIndex)
            {
                double harvest = this.CurrentHarvestByPeriod[periodIndex];
                double differenceFromTarget = this.TargetHarvestPerPeriod - harvest;
                double weight = this.TargetHarvestWeights[periodIndex];
                objectiveFunction += weight * differenceFromTarget * differenceFromTarget;
            }
            return objectiveFunction;
        }

        public abstract TimeSpan Run();
    }
}
