﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640.Heuristics
{
    public class GreatDeluge : Heuristic
    {
        public double InitialWaterLevel { get; set; }
        public double RainRate { get; set; }
        public int StopAfter { get; set; }

        public GreatDeluge(HarvestUnits units)
            : base(units)
        {
            this.InitialWaterLevel = 1.2 * this.BestObjectiveFunction;
            this.RainRate = 0.9999;
            this.StopAfter = 100000;

            this.ObjectiveFunctionByIteration = new List<double>(1000 * 1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override TimeSpan Run()
        {
            if ((this.RainRate <= 0.0) || (this.RainRate >= 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.RainRate));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            double currentObjectiveFunction = this.BestObjectiveFunction;
            double harvestPeriodScalingFactor = ((double)this.CurrentHarvestByPeriod.Length - 1.01F) / (double)byte.MaxValue;
            int iterationsSinceBestObjectiveImproved = 0;
            int movesSinceBestObjectiveImproved = 0;
            double unitIndexScalingFactor = ((double)this.Units.Count - 0.01F) / (double)UInt16.MaxValue;

            for (double waterLevel = this.InitialWaterLevel; waterLevel > 1.0; waterLevel *= this.RainRate)
            {
                int unitIndex = (int)(unitIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                int currentHarvestPeriod = this.CurrentHarvestPeriods[unitIndex];
                int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat()) + 1;
                while (candidateHarvestPeriod == currentHarvestPeriod)
                {
                    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat()) + 1;
                }
                Debug.Assert(candidateHarvestPeriod > 0);

                double candidateHarvest = this.CurrentHarvestByPeriod[candidateHarvestPeriod];
                double candidateYield = this.Units.YieldByPeriod[unitIndex, candidateHarvestPeriod];
                double currentHarvest = this.CurrentHarvestByPeriod[currentHarvestPeriod];
                double currentYield = this.Units.YieldByPeriod[unitIndex, currentHarvestPeriod];

                // default to move from uncut to cut case
                double candidateWeight = this.TargetHarvestWeights[candidateHarvestPeriod];
                double candidateDeviations = candidateWeight * (this.TargetHarvestPerPeriod - candidateHarvest - candidateYield) *
                                                                (this.TargetHarvestPerPeriod - candidateHarvest - candidateYield);
                double currentDeviations = candidateWeight * (this.TargetHarvestPerPeriod - candidateHarvest) *
                                                                (this.TargetHarvestPerPeriod - candidateHarvest);
                // if this is a move between periods then include objective function terms for the unit's current harvest period
                if (currentHarvestPeriod > 0)
                {
                    double currentWeight = this.TargetHarvestWeights[currentHarvestPeriod];
                    candidateDeviations += currentWeight * (this.TargetHarvestPerPeriod - currentHarvest + currentYield) *
                                                           (this.TargetHarvestPerPeriod - currentHarvest + currentYield);
                    currentDeviations += currentWeight * (this.TargetHarvestPerPeriod - currentHarvest) *
                                                         (this.TargetHarvestPerPeriod - currentHarvest);
                }
                double candidateObjectiveFunctionChange = candidateDeviations - currentDeviations;
                double candidateObjectiveFunction = currentObjectiveFunction + candidateObjectiveFunctionChange;
                if (candidateObjectiveFunction < 0.0F)
                {
                    candidateObjectiveFunction = -candidateObjectiveFunction;
                }
                ++iterationsSinceBestObjectiveImproved;

                if (candidateObjectiveFunction < waterLevel)
                {
                    this.CurrentHarvestPeriods[unitIndex] = candidateHarvestPeriod;
                    this.CurrentHarvestByPeriod[candidateHarvestPeriod] += candidateYield;
                    this.CurrentHarvestByPeriod[currentHarvestPeriod] -= currentYield;
                    currentObjectiveFunction = candidateObjectiveFunction;
                    ++movesSinceBestObjectiveImproved;
                    Debug.Assert(this.CurrentHarvestByPeriod[candidateHarvestPeriod] >= 0.0F);
                    Debug.Assert(this.CurrentHarvestByPeriod[currentHarvestPeriod] >= 0.0F);
                    Debug.Assert(currentObjectiveFunction >= 0.0F);

                    if (currentObjectiveFunction < this.BestObjectiveFunction)
                    {
                        if (movesSinceBestObjectiveImproved == 1)
                        {
                            // incremental update of best solution
                            this.BestHarvestPeriods[unitIndex] = candidateHarvestPeriod;
                            this.BestHarvestByPeriod[candidateHarvestPeriod] = this.CurrentHarvestByPeriod[candidateHarvestPeriod];
                            this.BestHarvestByPeriod[currentHarvestPeriod] = this.CurrentHarvestByPeriod[currentHarvestPeriod];
                        }
                        else
                        {
                            // copy current solution since history of moves between it and last best solution is unknown
                            Array.Copy(this.CurrentHarvestByPeriod, 0, this.BestHarvestByPeriod, 0, this.CurrentHarvestByPeriod.Length);
                            Array.Copy(this.CurrentHarvestPeriods, 0, this.BestHarvestPeriods, 0, this.CurrentHarvestPeriods.Length);
                        }

                        this.BestObjectiveFunction = currentObjectiveFunction;
                        iterationsSinceBestObjectiveImproved = 0;
                        movesSinceBestObjectiveImproved = 0;
                    }
                }

                this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
                if (iterationsSinceBestObjectiveImproved > this.StopAfter)
                {
                    break;
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
