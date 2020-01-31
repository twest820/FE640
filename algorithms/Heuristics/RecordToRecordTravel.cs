using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640.Heuristics
{
    public class RecordToRecordTravel : Heuristic
    {
        public double Deviation { get; set; }
        public int StopAfter { get; set; }

        public RecordToRecordTravel(HarvestUnits units)
            : base(units)
        {
            double defaultIterations = 100000.0;
            this.Deviation = 10000.0;
            this.StopAfter = 10000;

            this.ObjectiveFunctionByIteration = new List<double>((int)defaultIterations)
            {
                this.BestObjectiveFunction
            };
        }

        public override TimeSpan Run()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            double maximumAcceptableObjectiveFunction = this.BestObjectiveFunction + this.Deviation;
            double currentObjectiveFunction = this.BestObjectiveFunction;
            double harvestPeriodScalingFactor = ((double)this.CurrentHarvestByPeriod.Length - 1.01F) / (double)byte.MaxValue;
            int iterationsSinceBestObjectiveImproved = 0;
            int movesSinceBestObjectiveImproved = 0;
            double unitIndexScalingFactor = ((double)this.Units.Count - 0.01F) / (double)UInt16.MaxValue;

            while (iterationsSinceBestObjectiveImproved < this.StopAfter)
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

                if (candidateObjectiveFunction < maximumAcceptableObjectiveFunction)
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
                        maximumAcceptableObjectiveFunction = this.BestObjectiveFunction + this.Deviation;
                        movesSinceBestObjectiveImproved = 0;
                    }
                }

                this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
