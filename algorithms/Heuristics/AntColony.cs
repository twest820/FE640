using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640.Heuristics
{
    public class AntColony : Heuristic
    {
        public int Ants { get; set; }
        public int Iterations { get; set; }
        // public double MinimumPeriodVisibilityFactor { get; set; }
        public double PheremoneEvaporationRate { get; set; }
        public double PheremoneProportion { get; set; }
        public double ReservedPopulationProportion { get; set; }
        public double TrailTranspositionProbability { get; set; }

        public AntColony(HarvestUnits units)
            : base (units)
        {
            this.Ants = 20;
            this.Iterations = units.Count;
            // this.MinimumPeriodVisibilityFactor = 1.0;
            this.PheremoneEvaporationRate = 0.5;
            this.PheremoneProportion = 1.0 - 1.0 / (double)units.Count;
            this.ReservedPopulationProportion = 0.1;
            this.TrailTranspositionProbability = 1.0 / (double)units.Count;

            this.ObjectiveFunctionByIteration = new List<double>(this.Iterations);
        }

        public override TimeSpan Run()
        {
            if (this.Ants < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Ants));
            }
            if (this.Iterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Iterations));
            }
            if ((this.PheremoneEvaporationRate < 0.0) || (this.PheremoneEvaporationRate > 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.PheremoneEvaporationRate));
            }
            if ((this.PheremoneProportion < 0.0) || (this.PheremoneProportion > 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.PheremoneProportion));
            }
            if ((this.ReservedPopulationProportion < 0.0) || (this.ReservedPopulationProportion > 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.ReservedPopulationProportion));
            }
            if (this.Units.HasAdjacency)
            {
                throw new NotSupportedException();
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            HeuristicCritters ants = new HeuristicCritters(this.Ants, this.Units.Count, this.Units.HarvestPeriods, this.ReservedPopulationProportion);
            int maximumPeriodIndex = this.Units.HarvestPeriods + 1;
            double[] periodPheromoneVisibility = new double[maximumPeriodIndex];
            double pheromoneEvaporativeMultiplier = 1.0 - this.PheremoneEvaporationRate;
            double[,] pheromoneLevelsByUnitAndPeriod = new double[this.Units.Count, maximumPeriodIndex];
            double nMinusOneUnitScalingFactor = ((double)this.Units.Count - 1.0 - Constant.RoundToZeroTolerance) / (double)UInt16.MaxValue;
            double unityScalingFactor = 1.0 / (double)byte.MaxValue;
            int positionIncrement = 1;
            for (int iteration = 0; iteration < this.Iterations; ++iteration)
            {
                // send ants across harvest schedule
                // To reduce potential for bias, all ants turn around and proceed back through the schedule in the opposite direction on each iteration.
                int bestAntIndex = -1;
                double bestAntObjectiveFunction = double.MaxValue;
                for (int antIndex = 0; antIndex < this.Ants; ++antIndex)
                {
                    int[] antHarvestSchedule = ants.HarvestSchedules[antIndex];
                    double[] antHarvestVolumeByPeriod = ants.HarvestVolumesByPeriod[antIndex];
                    for (int periodIndex = 1; periodIndex < maximumPeriodIndex; ++periodIndex)
                    {
                        antHarvestVolumeByPeriod[periodIndex] = 0.0;
                    }
                    // int antMinimumHarvestPeriod = 1;

                    int antPositionIndex = positionIncrement == 1 ? 0 : this.Units.Count - 1;
                    for (int unitIndex = 0; unitIndex < this.Units.Count; antPositionIndex += positionIncrement, ++unitIndex)
                    {
                        double periodPheromoneVisibilitySum = 0.0;
                        for (int periodIndex = 1; periodIndex < maximumPeriodIndex; ++periodIndex)
                        {
                            double periodVisibility = this.TargetHarvestWeights[periodIndex];
                            //if (periodVisibility == antMinimumHarvestPeriod)
                            //{
                            //    periodVisibility *= this.MinimumPeriodVisibilityFactor;
                            //}
                            periodPheromoneVisibility[periodIndex] = this.PheremoneProportion * pheromoneLevelsByUnitAndPeriod[antPositionIndex, periodIndex] + (1.0 - this.PheremoneProportion) * periodVisibility;
                            periodPheromoneVisibilitySum += periodPheromoneVisibility[periodIndex];
                        }

                        double periodSelectionProbability = periodPheromoneVisibilitySum * unityScalingFactor * this.GetPseudorandomByteAsDouble();
                        double cumulativeProbability = 0.0;
                        int harvestPeriod = 0;
                        for (int periodIndex = 1; periodIndex < maximumPeriodIndex; ++periodIndex)
                        {
                            cumulativeProbability += periodPheromoneVisibility[periodIndex];
                            harvestPeriod = periodIndex;
                            if (periodSelectionProbability <= cumulativeProbability)
                            {
                                break;
                            }
                        }

                        // update ant's harvest schedule
                        // Giving extra weight to period with minimum harvest level produced no meaningful improvement.
                        double scheduledHarvestVolume = this.Units.YieldByPeriod[antPositionIndex, harvestPeriod];
                        antHarvestSchedule[antPositionIndex] = harvestPeriod;
                        antHarvestVolumeByPeriod[harvestPeriod] += scheduledHarvestVolume;
                        //if (harvestPeriod == antMinimumHarvestPeriod)
                        //{
                        //    double antMinimumHarvest = double.MaxValue;
                        //    for (int periodIndex = 1; periodIndex < maximumPeriodIndex; ++periodIndex)
                        //    {
                        //        if (antHarvestVolumeByPeriod[periodIndex] < antMinimumHarvest)
                        //        {
                        //            antMinimumHarvest = antHarvestVolumeByPeriod[periodIndex];
                        //            antMinimumHarvestPeriod = periodIndex;
                        //        }
                        //    }
                        //}
                    }

                    // recording transposition
                    double transpositionProbability = unityScalingFactor * this.GetPseudorandomByteAsDouble();
                    if (transpositionProbability < this.TrailTranspositionProbability)
                    {
                        int unit1index = (int)(nMinusOneUnitScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                        int unit2index = unit1index + 1;

                        int unit1scheduledHarvestPeriod = antHarvestSchedule[unit1index];
                        int unit2scheduledHarvestPeriod = antHarvestSchedule[unit2index];

                        double unit1scheduledVolume = this.Units.YieldByPeriod[unit1index, unit1scheduledHarvestPeriod];
                        double unit1transposedVolume = this.Units.YieldByPeriod[unit1index, unit2scheduledHarvestPeriod];
                        double unit2scheduledVolume = this.Units.YieldByPeriod[unit2index, unit2scheduledHarvestPeriod];
                        double unit2transposedVolume = this.Units.YieldByPeriod[unit2index, unit1scheduledHarvestPeriod];

                        antHarvestSchedule[unit1index] = unit2scheduledHarvestPeriod;
                        antHarvestSchedule[unit2index] = unit1scheduledHarvestPeriod;
                        antHarvestVolumeByPeriod[unit1scheduledHarvestPeriod] += unit2transposedVolume - unit1scheduledVolume;
                        antHarvestVolumeByPeriod[unit2scheduledHarvestPeriod] += unit1transposedVolume - unit2scheduledVolume;
                    }

                    // update ant's objective function and best ant for this iteration
                    double antObjectFunction = this.GetObjectiveFunction(antHarvestVolumeByPeriod);
                    ants.IndividualFitness[antIndex] = antObjectFunction;
                    if (antObjectFunction < bestAntObjectiveFunction)
                    {
                        bestAntIndex = antIndex;
                        bestAntObjectiveFunction = antObjectFunction;
                    }
                }

                // update best solution if it improved
                if (bestAntObjectiveFunction < this.BestObjectiveFunction)
                {
                    int[] bestAntHarvestSchedule = ants.HarvestSchedules[bestAntIndex];
                    double[] bestAntHarvestVolumeByPeriod = ants.HarvestVolumesByPeriod[bestAntIndex];

                    this.BestObjectiveFunction = bestAntObjectiveFunction;
                    Array.Copy(bestAntHarvestVolumeByPeriod, 0, this.BestHarvestByPeriod, 0, bestAntHarvestVolumeByPeriod.Length);
                    Array.Copy(bestAntHarvestSchedule, 0, this.BestHarvestPeriods, 0, bestAntHarvestSchedule.Length);
                }

                // evaporate pheromones form all units
                for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
                {
                    for (int periodIndex = 1; periodIndex < maximumPeriodIndex; ++periodIndex)
                    {
                        pheromoneLevelsByUnitAndPeriod[unitIndex, periodIndex] *= pheromoneEvaporativeMultiplier;
                    }
                }

                // add pheromones to units scheduled by ants
                // best ant only: max-min ant system
                for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
                {
                    int periodIndex = ants.HarvestSchedules[bestAntIndex][unitIndex];
                    pheromoneLevelsByUnitAndPeriod[unitIndex, periodIndex] += 1.0;
                }

                // proportional distribution: max-min ant system + ant colony system hybrid
                //ants.RecalculateFitnessDistribution();
                //for (int antIndex = 0; antIndex < this.Ants; ++antIndex)
                //{
                //    double antPheromoneLevel = ants.FitnessProbabilityDensity[antIndex];
                //    int[] antSchedule = ants.HarvestSchedules[antIndex];
                //    for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
                //    {
                //        int periodIndex = antSchedule[unitIndex];
                //        pheromoneLevelsByUnitAndPeriod[unitIndex, periodIndex] += antPheromoneLevel;
                //    }
                //}

                this.ObjectiveFunctionByIteration.Add(this.BestObjectiveFunction);
                positionIncrement *= -1;
            }

            Array.Copy(this.BestHarvestByPeriod, 0, this.CurrentHarvestByPeriod, 0, this.BestHarvestByPeriod.Length);
            Array.Copy(this.BestHarvestPeriods, 0, this.CurrentHarvestPeriods, 0, this.BestHarvestPeriods.Length);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
