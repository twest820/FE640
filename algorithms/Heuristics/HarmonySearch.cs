using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640.Heuristics
{
    /// <summary>
    /// Improved harmony search from Mahdavi M, Fesanghary M, Damangir E. 2007. An improved harmony search algorithm for solving optimization problems. 
    /// Applied Mathematics and Computation 188: 1567–1579.
    /// </summary>
    public class HarmonySearch : Heuristic
    {
        public int Generations { get; set; }
        public double MaximumBandwidth { get; set; }
        public double MaximumPitchAdjustmentRate { get; set; }
        public double MemoryRate { get; set; }
        public int MemorySize { get; set; }
        public double MinimumBandwidth { get; set; }
        public double MinimumPitchAdjustmentRate { get; set; }

        public HarmonySearch(HarvestUnits units)
            : base(units)
        {
            this.Generations = 100 * units.Count;
            this.MaximumBandwidth = 1.9;
            this.MaximumPitchAdjustmentRate = 1.0 / (double)units.Count;
            this.MemoryRate = 0.98;
            this.MemorySize = 50;
            this.MinimumBandwidth = 1.9;
            this.MinimumPitchAdjustmentRate = 0.1 / (double)units.Count;

            this.ObjectiveFunctionByIteration = new List<double>(this.Generations);
        }

        public override TimeSpan Run()
        {
            if (this.MaximumBandwidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MaximumBandwidth));
            }
            if (this.Generations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Generations));
            }
            if ((this.MaximumPitchAdjustmentRate > 1.0) || (this.MaximumPitchAdjustmentRate < this.MinimumPitchAdjustmentRate))
            {
                throw new ArgumentOutOfRangeException(nameof(this.MaximumPitchAdjustmentRate));
            }
            if ((this.MemoryRate < 0.0) || (this.MemoryRate > 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.MemoryRate));
            }
            if ((this.MemorySize < 1) || (this.MemorySize > UInt16.MaxValue))
            {
                throw new ArgumentOutOfRangeException(nameof(this.MemorySize));
            }
            if ((this.MinimumPitchAdjustmentRate < 0.0) || (this.MinimumPitchAdjustmentRate > this.MaximumPitchAdjustmentRate))
            {
                throw new ArgumentOutOfRangeException(nameof(this.MinimumPitchAdjustmentRate));
            }
            if (this.Units.HasAdjacency)
            {
                throw new NotSupportedException();
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // initialize harmonies
            int[][] harmonyScheduleMemory = new int[this.MemorySize][];
            double[] harmonyObjectiveFunctionMemory = new double[this.MemorySize];
            double[] harvestVolumes = new double[this.BestHarvestByPeriod.Length];
            double harvestPeriodScalingFactor = ((double)this.CurrentHarvestByPeriod.Length - 1.0 - Constant.RoundToZeroTolerance) / (double)byte.MaxValue;

            int worstHarmonyMemoryIndex = -1;
            this.BestObjectiveFunction = double.MaxValue;
            double worstObjectiveFunction = double.MinValue;
            for (int memoryIndex = 0; memoryIndex < this.MemorySize; ++memoryIndex)
            {
                // set random harvest schedule
                int[] harmonySchedule = new int[this.Units.Count];
                for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
                {
                    harmonySchedule[unitIndex] = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
                }
                harmonyScheduleMemory[memoryIndex] = harmonySchedule;

                // evaluate harmony
                Array.Clear(harvestVolumes, 0, harvestVolumes.Length);
                this.GetHarvestVolumes(harmonySchedule, harvestVolumes);
                double harmonyObjectiveFunction = this.GetObjectiveFunction(harvestVolumes);
                harmonyObjectiveFunctionMemory[memoryIndex] = harmonyObjectiveFunction;

                if (harmonyObjectiveFunction < this.BestObjectiveFunction)
                {
                    this.BestObjectiveFunction = harmonyObjectiveFunction;
                    Array.Copy(harmonySchedule, 0, this.BestHarvestPeriods, 0, this.Units.Count);
                    Array.Copy(harvestVolumes, 0, this.BestHarvestByPeriod, 0, this.BestHarvestByPeriod.Length);
                }
                if (harmonyObjectiveFunction > worstObjectiveFunction)
                {
                    worstHarmonyMemoryIndex = memoryIndex;
                    worstObjectiveFunction = harmonyObjectiveFunction;
                }
            }
            // if needed, include solution from units
            this.ObjectiveFunctionByIteration.Add(this.BestObjectiveFunction);

            // do search
            double bandwidth = this.MaximumBandwidth;
            double bandwidthIncrement = Math.Log(this.MinimumBandwidth / this.MaximumBandwidth) / (double)this.Generations;
            double memoryScalingFactor = ((double)this.MemorySize - Constant.RoundToZeroTolerance) / (double)UInt16.MaxValue;
            double pitchAdjustmentIncrement = (this.MaximumPitchAdjustmentRate - this.MinimumPitchAdjustmentRate) / (double)this.Generations;
            double pitchAdjustmentRate = this.MinimumPitchAdjustmentRate;
            // double unitScalingFactor = ((double)this.Units.Count - Constant.RoundToZeroTolerance) / (double)UInt16.MaxValue;
            double unityScalingFactor = (1.0 - Constant.RoundToZeroTolerance) / (double)byte.MaxValue;
            int[] candidateHarmonySchedule = new int[this.Units.Count];
            for (int generation = 0; generation < this.Generations; ++generation)
            {
                // construct candidate harmony
                for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
                {
                    bool consultMemory = unityScalingFactor * this.GetPseudorandomByteAsDouble() < this.MemoryRate;
                    if (consultMemory)
                    {
                        int memoryIndex = (int)(memoryScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                        candidateHarmonySchedule[unitIndex] = harmonyScheduleMemory[memoryIndex][unitIndex];
                        bool adjustPitch = unityScalingFactor * this.GetPseudorandomByteAsDouble() < pitchAdjustmentRate;
                        if (adjustPitch)
                        {
                            // gobal harmony search: substitution interpretation
                            // candidateHarmonySchedule[unitIndex] = this.BestHarvestPeriods[unitIndex];
                            // gobal harmony search: draw from best interpretation
                            // int bestSolutionUnitIndex = (int)(unitScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                            // candidateHarmonySchedule[unitIndex] = this.BestHarvestPeriods[bestSolutionUnitIndex];

                            // improved harmony search
                            int pitchChange = (int)(bandwidth * unityScalingFactor * this.GetPseudorandomByteAsDouble());
                            if (pitchChange != 0)
                            {
                                bool increasePitch = unityScalingFactor * this.GetPseudorandomByteAsDouble() > 0.5;
                                if (increasePitch)
                                {
                                    candidateHarmonySchedule[unitIndex] += pitchChange;
                                    if (candidateHarmonySchedule[unitIndex] > this.Units.HarvestPeriods)
                                    {
                                        candidateHarmonySchedule[unitIndex] = this.Units.HarvestPeriods;
                                    }
                                }
                                else
                                {
                                    candidateHarmonySchedule[unitIndex] -= pitchChange;
                                    if (candidateHarmonySchedule[unitIndex] < 1)
                                    {
                                        candidateHarmonySchedule[unitIndex] = 1;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        candidateHarmonySchedule[unitIndex] = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble());
                    }
                }

                // evaluate harmony
                Array.Clear(harvestVolumes, 0, harvestVolumes.Length);
                this.GetHarvestVolumes(candidateHarmonySchedule, harvestVolumes);
                double candidateHarmonyObjectiveFunction = this.GetObjectiveFunction(harvestVolumes);

                if (candidateHarmonyObjectiveFunction < this.BestObjectiveFunction)
                {
                    // update best harmony
                    // Do this before accepting the harmony as the code below complicates access to the schedule just obtained.
                    this.BestObjectiveFunction = candidateHarmonyObjectiveFunction;
                    Array.Copy(candidateHarmonySchedule, 0, this.BestHarvestPeriods, 0, this.Units.Count);
                    Array.Copy(harvestVolumes, 0, this.BestHarvestByPeriod, 0, this.BestHarvestByPeriod.Length);
                }
                if (candidateHarmonyObjectiveFunction < worstObjectiveFunction)
                {
                    // accept harmony
                    harmonyObjectiveFunctionMemory[worstHarmonyMemoryIndex] = candidateHarmonyObjectiveFunction;
                    int[] harmonyScheduleSwap = harmonyScheduleMemory[worstHarmonyMemoryIndex];
                    harmonyScheduleMemory[worstHarmonyMemoryIndex] = candidateHarmonySchedule;
                    candidateHarmonySchedule = harmonyScheduleSwap;

                    worstObjectiveFunction = double.MinValue;
                    for (int memoryIndex = 0; memoryIndex < this.MemorySize; ++memoryIndex)
                    {
                        if (worstObjectiveFunction < harmonyObjectiveFunctionMemory[memoryIndex])
                        {
                            worstObjectiveFunction = harmonyObjectiveFunctionMemory[memoryIndex];
                            worstHarmonyMemoryIndex = memoryIndex;
                        }
                    }
                }

                // update loop variables
                bandwidth *= bandwidthIncrement;
                pitchAdjustmentRate += pitchAdjustmentIncrement;

                this.ObjectiveFunctionByIteration.Add(this.BestObjectiveFunction);
            }

            Array.Copy(this.BestHarvestByPeriod, 0, this.CurrentHarvestByPeriod, 0, this.BestHarvestByPeriod.Length);
            Array.Copy(this.BestHarvestPeriods, 0, this.CurrentHarvestPeriods, 0, this.BestHarvestPeriods.Length);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
