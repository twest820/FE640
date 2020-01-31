﻿using FE640.Heuristics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    public abstract class OptimizeCmdlet : Cmdlet
    {
        [Parameter]
        public int BestOf { get; set; }
        [Parameter]
        public Nullable<float> TargetHarvestPerPeriod { get; set; }
        [Parameter]
        public double[] TargetHarvestWeights { get; set; }
        [Parameter(Mandatory = true)]
        public HarvestUnits Units { get; set; }

        public OptimizeCmdlet()
        {
            this.BestOf = 1;
            this.TargetHarvestPerPeriod = null;
            this.TargetHarvestWeights = null;
        }

        private void ConfigureHeuristic(Heuristic heuristic)
        {
            if (this.TargetHarvestPerPeriod.HasValue)
            {
                heuristic.TargetHarvestPerPeriod = this.TargetHarvestPerPeriod.Value;
            }
            if (this.TargetHarvestWeights != null)
            {
                heuristic.TargetHarvestWeights = this.TargetHarvestWeights;
            }
        }

        protected abstract Heuristic CreateHeuristic();

        protected override void ProcessRecord()
        {
            Heuristic bestHeuristic = null;
            int totalIterations = 0;
            TimeSpan totalRunTime = TimeSpan.Zero;
            List<double> objectiveFunctionValues = new List<double>();
            for (int iteration = 0; iteration < this.BestOf; ++iteration)
            {
                Heuristic currentHeuristic = this.CreateHeuristic();
                this.ConfigureHeuristic(currentHeuristic);
                totalRunTime += currentHeuristic.Run();
                totalIterations += currentHeuristic.ObjectiveFunctionByIteration.Count;
                objectiveFunctionValues.Add(currentHeuristic.BestObjectiveFunction);

                if ((bestHeuristic == null) || (currentHeuristic.BestObjectiveFunction < bestHeuristic.BestObjectiveFunction))
                {
                    bestHeuristic = currentHeuristic;
                }
            }

            this.WriteObject(bestHeuristic);
            if (this.BestOf > 1)
            {
                this.WriteObject(objectiveFunctionValues);
            }

            this.WriteHeuristicRun(bestHeuristic, totalIterations, totalRunTime);
        }

        private void WriteHeuristicRun(Heuristic heuristic, int iterations, TimeSpan runTime)
        {
            int movesAccepted = 0;
            int movesRejected = 0;
            double previousObjectiveFunction = heuristic.ObjectiveFunctionByIteration[0];
            for (int index = 1; index < heuristic.ObjectiveFunctionByIteration.Count; ++index)
            {
                double currentObjectiveFunction = heuristic.ObjectiveFunctionByIteration[index];
                if (currentObjectiveFunction != previousObjectiveFunction)
                {
                    ++movesAccepted;
                }
                else
                {
                    ++movesRejected;
                }
                previousObjectiveFunction = currentObjectiveFunction;
            }

            double maximumHarvest = Double.MinValue;
            double meanHarvest = 0.0F;
            double minimumHarvest = Double.MaxValue;
            for (int periodIndex = 1; periodIndex < heuristic.BestHarvestByPeriod.Length; ++periodIndex)
            {
                double harvest = heuristic.BestHarvestByPeriod[periodIndex];
                maximumHarvest = Math.Max(harvest, maximumHarvest);
                meanHarvest += harvest;
                minimumHarvest = Math.Min(harvest, minimumHarvest);
            }
            double periods = (double)(heuristic.BestHarvestByPeriod.Length - 1);
            meanHarvest /= periods;
            double flowEvenness = Math.Max(maximumHarvest - meanHarvest, meanHarvest - minimumHarvest) / meanHarvest;

            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0} moves, {1} accepted ({2:0%}), {3} rejected ({4:0%})", totalMoves, movesAccepted, (double)movesAccepted / (double)totalMoves, movesRejected, (double)movesRejected / (double)totalMoves);
            this.WriteVerbose("objective: best {0:0.00#}M, ending {1:0.00#}M.", 1E-6 * heuristic.BestObjectiveFunction, 1E-6 * heuristic.ObjectiveFunctionByIteration.Last());
            this.WriteVerbose("flow: {0:0.0}k mean, {1:0.000}% even, {2:0.0}-{3:0.0}k = range {4:0}.", 1E-3 * meanHarvest, 1E2 * flowEvenness, 1E-3 * minimumHarvest, 1E-3 * maximumHarvest, maximumHarvest - minimumHarvest);
            double iterationsPerSecond = (double)iterations / (double)runTime.TotalSeconds;
            this.WriteVerbose("{0} iterations in {1:s\\.fff}s ({2:0.00} Miterations/s).", iterations, runTime, 1E-6 * iterationsPerSecond);
        }

        protected void WriteVerbose(string format, params object[] args)
        {
            base.WriteVerbose(String.Format(format, args));
        }
    }
}
