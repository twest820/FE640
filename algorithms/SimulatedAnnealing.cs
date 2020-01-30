using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640
{
    public class SimulatedAnnealing : Heuristic
    {
        public double Alpha { get; set; }
        public double FinalTemperature { get; set; }
        public double InitialTemperature { get; set; }
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
            double temperatureSteps = (double)(defaultIterations / this.IterationsPerTemperature);
            this.Alpha = 1.0F / (double)Math.Pow(this.InitialTemperature / this.FinalTemperature, 1.0F / temperatureSteps);

            this.ObjectiveFunctionByIteration = new List<double>(defaultIterations)
            {
                this.BestObjectiveFunction
            };
        }

        public TimeSpan Anneal()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            double acceptanceProbabilityScalingFactor = 1.0F / (double)byte.MaxValue;
            double currentObjectiveFunction = this.BestObjectiveFunction;
            double harvestPeriodScalingFactor = ((double)this.CurrentHarvestByPeriod.Length - 1.01F) / (double)byte.MaxValue;
            int movesSinceBestObjectiveImproved = 0;
            double temperature = this.InitialTemperature;
            double unitIndexScalingFactor = ((double)this.Units.Count - 0.01F) / (double)UInt16.MaxValue;

            for (double currentTemperature = this.InitialTemperature; currentTemperature > this.FinalTemperature; currentTemperature *= this.Alpha)
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
                    // TODO: if needed, support zero crossing of objective function

                    bool acceptMove = candidateObjectiveFunctionChange < 0.0F;
                    if (acceptMove == false)
                    {
                        double exponent = 0.001F * candidateObjectiveFunctionChange / temperature; // why 0.001?
                        if (exponent < 9.0F)
                        {
                            // exponent is small enough not to round acceptance probabilities down to zero
                            // 1/e^9 is an acceptance probability of 0.012%, or 1 in 8095 moves.
                            double acceptanceProbability = 1.0F / (double)Math.Exp(exponent);
                            double moveProbability = acceptanceProbabilityScalingFactor * this.GetPseudorandomByteAsFloat();
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
