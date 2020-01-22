using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640
{
    public class SimulatedAnnealing : Heuristic
    {
        public float Alpha { get; set; }
        public float FinalTemperature { get; set; }
        public float InitialTemperature { get; set; }
        public int IterationsPerTemperature { get; set; }

        public SimulatedAnnealing(HarvestUnits units)
            :  base(units)
        {
            this.FinalTemperature = 100.0F;
            this.InitialTemperature = 10000.0F;
            this.IterationsPerTemperature = 10;

            // typical i5-4200U interation velocities are around 4 Miterations/s for debug and 7.3 Miterations/s for retail
            // Default to 1M iterations as a reasonable runtime for unit testing.
            int defaultIterations = 1000 * 1000;
            float temperatureSteps = (float)(defaultIterations / this.IterationsPerTemperature);
            this.Alpha = 1.0F / (float)Math.Pow(this.InitialTemperature / this.FinalTemperature, 1.0F / temperatureSteps);

            this.ObjectiveFunctionByIteration = new List<float>(defaultIterations)
            {
                this.BestObjectiveFunction
            };
        }

        public TimeSpan Anneal()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float acceptanceProbabilityScalingFactor = 1.0F / (float)byte.MaxValue;
            float currentObjectiveFunction = this.BestObjectiveFunction;
            float harvestPeriodScalingFactor = ((float)this.CurrentHarvestByPeriod.Length - 1.01F) / (float)byte.MaxValue;
            int movesSinceBestObjectiveImproved = 0;
            float temperature = this.InitialTemperature;
            float unitIndexScalingFactor = ((float)this.Units.Count - 0.01F) / (float)UInt16.MaxValue;

            for (float currentTemperature = this.InitialTemperature; currentTemperature > this.FinalTemperature; currentTemperature *= this.Alpha)
            {
                for (int iterationAtTemperature = 0; iterationAtTemperature < this.IterationsPerTemperature; ++iterationAtTemperature)
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
                    // TODO: if needed, support zero crossing of objective function

                    bool acceptMove = candidateObjectiveFunctionChange < 0.0F;
                    if (acceptMove == false)
                    {
                        float exponent = 0.001F * candidateObjectiveFunctionChange / temperature; // why 0.001?
                        if (exponent < 9.0F)
                        {
                            // exponent is small enough not to round acceptance probabilities down to zero
                            // 1/e^9 is an acceptance probability of 0.012%, or 1 in 8095 moves.
                            float acceptanceProbability = 1.0F / (float)Math.Exp(exponent);
                            float moveProbability = acceptanceProbabilityScalingFactor * this.GetPseudorandomByteAsFloat();
                            if (moveProbability < acceptanceProbability)
                            {
                                acceptMove = true;
                            }
                        }
                    }

                    if (acceptMove)
                    {
                        this.CurrentHarvestPeriods[unitIndex] = candidateHarvestPeriod;
                        this.CurrentHarvestByPeriod[candidateHarvestPeriod] += candidateYield;
                        this.CurrentHarvestByPeriod[currentHarvestPeriod] -= currentYield;
                        currentObjectiveFunction += candidateObjectiveFunctionChange;
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
