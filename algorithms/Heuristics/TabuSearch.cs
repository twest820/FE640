﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640.Heuristics
{
    public class TabuSearch : Heuristic
    {
        public int Iterations { get; set; }
        public int Tenure { get; set; }

        public TabuSearch(HarvestUnits units, int maximumUnitIndex)
            :  base(units, maximumUnitIndex)
        {
            this.Iterations = 2 * maximumUnitIndex;
            this.Tenure = 7;

            this.ObjectiveFunctionByIteration = new List<double>(1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override TimeSpan Run()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            double[,] candidateObjectiveFunctions = new double[this.MaximumUnitIndex, this.CurrentHarvestByPeriod.Length];
            int[,] remainingTabuTenures = new int[this.MaximumUnitIndex, this.CurrentHarvestByPeriod.Length];
            double currentObjectiveFunction = this.BestObjectiveFunction;
            int movesSinceBestObjectiveImproved = 0;
            for (int neighborhoodEvaluation = 0; neighborhoodEvaluation < this.Iterations; ++neighborhoodEvaluation)
            {
                // evaluate potential moves in neighborhood
                double bestCandidateObjectiveFunction = Double.MaxValue;
                int bestUnitIndex = -1;
                int bestHarvestPeriod = -1;
                double bestNonTabuCandidateObjectiveFunction = Double.MaxValue;
                int bestNonTabuUnitIndex = -1;
                int bestNonTabuHarvestPeriod = -1;
                for (int unitIndex = 0; unitIndex < this.MaximumUnitIndex; ++unitIndex)
                {
                    int currentHarvestPeriod = this.CurrentHarvestPeriods[unitIndex];
                    for (int periodIndex = 1; periodIndex < this.CurrentHarvestByPeriod.Length; ++periodIndex)
                    {
                        double candidateObjectiveFunction = Double.MaxValue;
                        if (periodIndex != currentHarvestPeriod)
                        {
                            candidateObjectiveFunction = this.GetCandidateObjectiveFunction(unitIndex, periodIndex, currentObjectiveFunction);
                        }
                        
                        if (candidateObjectiveFunction < bestCandidateObjectiveFunction)
                        {
                            bestCandidateObjectiveFunction = candidateObjectiveFunction;
                            bestUnitIndex = unitIndex;
                            bestHarvestPeriod = periodIndex;
                        }

                        int tabuTenure = remainingTabuTenures[unitIndex, periodIndex];
                        if ((tabuTenure == 0) && (candidateObjectiveFunction < bestNonTabuCandidateObjectiveFunction))
                        {
                            bestNonTabuCandidateObjectiveFunction = candidateObjectiveFunction;
                            bestNonTabuUnitIndex = unitIndex;
                            bestNonTabuHarvestPeriod = periodIndex;
                        }

                        // not needed, but potentially useful for debugging
                        candidateObjectiveFunctions[unitIndex, periodIndex] = candidateObjectiveFunction;
                        if (tabuTenure > 0)
                        {
                            remainingTabuTenures[unitIndex, periodIndex] = tabuTenure - 1;
                        }
                    }
                }
                Debug.Assert(bestCandidateObjectiveFunction >= 0.0F);

                // make best move and update tabu table
                // other possibilities: 1) make unit tabu, 2) stochastic tenure
                if (bestCandidateObjectiveFunction < this.BestObjectiveFunction)
                {
                    // always accept best candidate if it improves upon the best solution
                    this.BestObjectiveFunction = bestCandidateObjectiveFunction;
                    currentObjectiveFunction = bestCandidateObjectiveFunction;

                    int previousHarvestPeriod = this.AcceptMove(bestUnitIndex, bestHarvestPeriod);
                    remainingTabuTenures[bestUnitIndex, bestHarvestPeriod] = this.Tenure;
                    ++movesSinceBestObjectiveImproved;

                    this.UpdateBestSolution(bestUnitIndex, previousHarvestPeriod, bestHarvestPeriod, movesSinceBestObjectiveImproved);
                    movesSinceBestObjectiveImproved = 0;
                }
                else if (bestNonTabuUnitIndex != -1)
                {
                    // otherwise, accept the best non-tabu move when one exists
                    // Existence is nearly certain since (n units) * (n periods) >> tenure in most configurations.
                    currentObjectiveFunction = bestNonTabuCandidateObjectiveFunction;

                    this.AcceptMove(bestNonTabuUnitIndex, bestNonTabuHarvestPeriod);
                    remainingTabuTenures[bestNonTabuUnitIndex, bestNonTabuHarvestPeriod] = this.Tenure;
                    ++movesSinceBestObjectiveImproved;
                }

                this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
