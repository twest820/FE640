using FE640.Heuristics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    public abstract class OptimizeCmdlet : Cmdlet
    {
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int BestOf { get; set; }
        [Parameter]
        [ValidateRange(0.0, 1.0)]
        public List<double> HarvestProbabilityByPeriod { get; set; }
        [Parameter]
        public SwitchParameter LoopHarvestPeriods { get; set; }
        [Parameter]
        public SwitchParameter UniformHarvestProbability { get; set; }
        [Parameter]
        [ValidateRange(0.0, Double.MaxValue)]
        public Nullable<double> TargetHarvestPerPeriod { get; set; }
        [Parameter]
        public double[] TargetHarvestWeights { get; set; }
        [Parameter(Mandatory = true)]
        public HarvestUnits Units { get; set; }

        public OptimizeCmdlet()
        {
            this.BestOf = 1;
            this.HarvestProbabilityByPeriod = null;
            this.LoopHarvestPeriods = false;
            this.UniformHarvestProbability = false;
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
            Stopwatch timeSinceLastProgress = new Stopwatch();
            timeSinceLastProgress.Start();
            for (int iteration = 0; iteration < this.BestOf; ++iteration)
            {
                if (this.HarvestProbabilityByPeriod != null)
                {
                    this.Units.SetRandomSchedule(this.HarvestProbabilityByPeriod);
                }
                else if (this.LoopHarvestPeriods)
                {
                    this.Units.SetLoopSchedule(iteration + 1);
                }
                else if (this.UniformHarvestProbability)
                {
                    this.Units.SetRandomSchedule();
                }

                Heuristic currentHeuristic = this.CreateHeuristic();
                this.ConfigureHeuristic(currentHeuristic);
                totalRunTime += currentHeuristic.Run();
                objectiveFunctionValues.Add(currentHeuristic.BestObjectiveFunction);
                totalIterations += currentHeuristic.ObjectiveFunctionByIteration.Count;

                if ((bestHeuristic == null) || (currentHeuristic.BestObjectiveFunction < bestHeuristic.BestObjectiveFunction))
                {
                    bestHeuristic = currentHeuristic;
                }

                if (this.Stopping)
                {
                    break;
                }

                if (timeSinceLastProgress.Elapsed.TotalSeconds > 30.0)
                {
                    this.WriteProgress(new ProgressRecord(0, currentHeuristic.GetType().Name, String.Format("run {0}", iteration))
                    {
                        PercentComplete = (int)(100.0F * (float)iteration / (float)this.BestOf)
                    });
                    timeSinceLastProgress.Restart();
                }
            }
            timeSinceLastProgress.Stop();

            this.WriteObject(bestHeuristic);
            if (this.BestOf > 1)
            {
                this.WriteObject(objectiveFunctionValues);
            }

            this.WriteHeuristicRun(bestHeuristic, objectiveFunctionValues, totalIterations, totalRunTime);
        }

        private void WriteHeuristicRun(Heuristic heuristic, List<double> objectiveFuctionValues, int iterations, TimeSpan runTime)
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
            double minimumHarvest = Double.MaxValue;
            double sum = 0.0;
            double sumOfSquares = 0.0;
            for (int periodIndex = 1; periodIndex < heuristic.BestHarvestByPeriod.Length; ++periodIndex)
            {
                double harvest = heuristic.BestHarvestByPeriod[periodIndex];
                maximumHarvest = Math.Max(harvest, maximumHarvest);
                sum += harvest;
                sumOfSquares += harvest * harvest;
                minimumHarvest = Math.Min(harvest, minimumHarvest);
            }
            double periods = (double)(heuristic.BestHarvestByPeriod.Length - 1);
            double meanHarvest = sum / periods;
            double variance = sumOfSquares / periods - meanHarvest * meanHarvest;
            double standardDeviation = Math.Sqrt(variance);
            double flowEvenness = Math.Max(maximumHarvest - meanHarvest, meanHarvest - minimumHarvest) / meanHarvest;

            int totalMoves = movesAccepted + movesRejected;
            this.WriteVerbose("{0} moves, {1} changing ({2:0%}), {3} unchanging ({4:0%})", totalMoves, movesAccepted, (double)movesAccepted / (double)totalMoves, movesRejected, (double)movesRejected / (double)totalMoves);
            this.WriteVerbose("objective: best {0:0.00#}M, mean {1:0.00#}M, ending {2:0.00#}M.", 1E-6 * heuristic.BestObjectiveFunction, 1E-6 * objectiveFuctionValues.Average(), 1E-6 * heuristic.ObjectiveFunctionByIteration.Last());
            this.WriteVerbose("flow: {0:0.0#}k mean, {1:0.000} σ, {2:0.000}% even, {3:0.0#}-{4:0.0#}k = range {5:0.0}.", 1E-3 * meanHarvest, standardDeviation, 1E2 * flowEvenness, 1E-3 * minimumHarvest, 1E-3 * maximumHarvest, maximumHarvest - minimumHarvest);
            if (this.Units.HasAdjacency)
            {
                this.WriteVerbose("opening: {0:0.0} allowed, {1:0.0} maximum reported", this.Units.MaximumOpeningSize, heuristic.MaximumOpeningSize);
            }

            double iterationsPerSecond = (double)iterations / (double)runTime.TotalSeconds;
            double iterationsPerSecondMultiplier = iterationsPerSecond > 1E6 ? 1E-6 : 1E-3;
            string iterationsPerSecondScale = iterationsPerSecond > 1E6 ? "M" : "k";
            this.WriteVerbose("{0} iterations in {1:0.000}s ({2:0.00} {3}iterations/s).", iterations, runTime.TotalSeconds, iterationsPerSecondMultiplier * iterationsPerSecond, iterationsPerSecondScale);
        }

        protected void WriteVerbose(string format, params object[] args)
        {
            base.WriteVerbose(String.Format(format, args));
        }
    }
}
