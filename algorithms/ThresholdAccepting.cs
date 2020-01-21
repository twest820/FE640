using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640
{
    public class ThresholdAccepting : Heuristic
    {
        public int IterationsPerThreshold { get; set; }
        public List<float> Thresholds { get; private set; }

        public ThresholdAccepting(HarvestUnits units)
            : base(units)
        {
            this.IterationsPerThreshold = 5 * units.Count;
            this.Thresholds = new List<float>() { 1.1F, 1.08F, 1.05F, 1.03F, 1.01F, 1.0F };

            this.ObjectiveFunctionByIteration = new List<float>(this.Thresholds.Count * this.IterationsPerThreshold)
            {
                this.BestObjectiveFunction
            };

            // example code initializes parcels to random harvest period
        }

        // very similar code to SimulatedAnnealing.Anneal()
        // Differences are all in move acceptance.
        public TimeSpan Accept()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float currentObjectiveFunction = this.BestObjectiveFunction;
            float harvestPeriodScalingFactor = ((float)this.CurrentHarvestByPeriod.Length - 1.01F) / (float)byte.MaxValue;
            int movesSinceBestObjectiveImproved = 0;
            float unitIndexScalingFactor = ((float)this.Units.Count - 0.01F) / (float)UInt16.MaxValue;

            foreach (float threshold in this.Thresholds)
            {
                for (int iteration = 0; iteration < this.IterationsPerThreshold; ++iteration)
                {
                    int unitIndex = (int)(unitIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                    int currentHarvestPeriod = this.CurrentHarvestPeriods[unitIndex];
                    int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat()) + 1;
                    while (candidateHarvestPeriod == currentHarvestPeriod)
                    {
                        candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat()) + 1;
                    }
                    Debug.Assert(candidateHarvestPeriod > 0);

                    float candidateHarvest = this.CurrentHarvestByPeriod[candidateHarvestPeriod];
                    float candidateYield = this.Units.YieldByPeriod[unitIndex, candidateHarvestPeriod];
                    float currentHarvest = this.CurrentHarvestByPeriod[currentHarvestPeriod];
                    float currentYield = this.Units.YieldByPeriod[unitIndex, currentHarvestPeriod];

                    // default to move from uncut to cut case
                    float candidateDeviations = (this.TargetHarvestPerPeriod - candidateHarvest - candidateYield) *
                                                (this.TargetHarvestPerPeriod - candidateHarvest - candidateYield);
                    float currentDeviations = (this.TargetHarvestPerPeriod - candidateHarvest) *
                                              (this.TargetHarvestPerPeriod - candidateHarvest);
                    // if this is a move between periods then include objective function terms for the unit's current harvest period
                    if (currentHarvestPeriod > 0)
                    {
                        candidateDeviations += (this.TargetHarvestPerPeriod - currentHarvest + currentYield) *
                                               (this.TargetHarvestPerPeriod - currentHarvest + currentYield);
                        currentDeviations += (this.TargetHarvestPerPeriod - currentHarvest) *
                                             (this.TargetHarvestPerPeriod - currentHarvest);
                    }
                    float candidateObjectiveFunctionChange = candidateDeviations - currentDeviations;
                    float candidateObjectiveFunction = currentObjectiveFunction + candidateObjectiveFunctionChange;

                    if (candidateObjectiveFunction < threshold * currentObjectiveFunction)
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
                            movesSinceBestObjectiveImproved = 0;
                        }
                    }
                    this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
