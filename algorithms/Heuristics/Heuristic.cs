using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FE640.Heuristics
{
    public abstract class Heuristic : RandomNumberConsumer
    {
        private double targetHarvestPerPeriod;
        private double[] targetHarvestWeights;

        protected HarvestUnits Units { get; private set; }

        public double BestObjectiveFunction { get; protected set; }
        public double[] BestHarvestByPeriod { get; protected set; }
        public int[] BestHarvestPeriods { get; protected set; }
        public double[] CurrentHarvestByPeriod { get; protected set; }
        public int[] CurrentHarvestPeriods { get; protected set; }
        public float MaximumOpeningSize { get; protected set; }
        public List<double> ObjectiveFunctionByIteration { get; protected set; }

        protected Heuristic(HarvestUnits units)
        {
            this.MaximumOpeningSize = 0.0F;

            this.BestHarvestPeriods = new int[units.Count];
            Array.Copy(units.HarvestSchedule, 0, this.BestHarvestPeriods, 0, units.Count);
            this.CurrentHarvestPeriods = new int[units.Count];
            Array.Copy(units.HarvestSchedule, 0, this.CurrentHarvestPeriods, 0, units.Count);
            this.Units = units;

            // units default to harvest period 0, which is treated as no cut
            // Cut periods are therefore 1...n and require array allocation accordingly. This simplifies the inner annealing loop
            // as no special logic is needed for handling a special no cut value such as -1.
            int periods = this.Units.YieldByPeriod.GetLength(1) - 1;
            this.BestHarvestByPeriod = new double[periods + 1];
            this.CurrentHarvestByPeriod = new double[periods + 1];
            this.targetHarvestWeights = Enumerable.Repeat(1.0, periods + 1).ToArray();

            this.targetHarvestPerPeriod = this.GetDefaultTargetHarvestPerPeriod();
            this.RecalculateHarvestVolumes();
            this.BestObjectiveFunction = this.RecalculateObjectiveFunction();
        }

        public double TargetHarvestPerPeriod
        {
            get
            {
                return this.targetHarvestPerPeriod;
            }
            set
            {
                this.targetHarvestPerPeriod = value;
                this.BestObjectiveFunction = this.RecalculateObjectiveFunction();
                if (this.ObjectiveFunctionByIteration.Count > 0)
                {
                    this.ObjectiveFunctionByIteration[0] = this.BestObjectiveFunction;
                }
            }
        }

        public double[] TargetHarvestWeights 
        { 
            get
            {
                return this.targetHarvestWeights;
            }
            set
            {
                this.targetHarvestWeights = value;
                this.BestObjectiveFunction = this.RecalculateObjectiveFunction();
                if (this.ObjectiveFunctionByIteration.Count > 0)
                {
                    this.ObjectiveFunctionByIteration[0] = this.BestObjectiveFunction;
                }
            }
        }

        protected int AcceptMove(int unitIndex, int candidateHarvestPeriod)
        {
            int currentHarvestPeriod = this.CurrentHarvestPeriods[unitIndex];
            double candidateYield = this.Units.YieldByPeriod[unitIndex, candidateHarvestPeriod];
            double currentYield = this.Units.YieldByPeriod[unitIndex, currentHarvestPeriod];

            this.CurrentHarvestPeriods[unitIndex] = candidateHarvestPeriod;
            this.CurrentHarvestByPeriod[candidateHarvestPeriod] += candidateYield;
            this.CurrentHarvestByPeriod[currentHarvestPeriod] -= currentYield;
            Debug.Assert(this.CurrentHarvestByPeriod[candidateHarvestPeriod] >= 0.0F);
            Debug.Assert(this.CurrentHarvestByPeriod[currentHarvestPeriod] >= 0.0F);

            return currentHarvestPeriod;
        }

        protected double GetCandidateObjectiveFunction(int unitIndex, int candidateHarvestPeriod, double currentObjectiveFunction)
        {
            int currentHarvestPeriod = this.CurrentHarvestPeriods[unitIndex];

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
            return candidateObjectiveFunction;
        }

        protected double GetDefaultTargetHarvestPerPeriod()
        {
            int periods = this.Units.YieldByPeriod.GetLength(1) - 1;
            double maximumYield = 0.0F;
            for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
            {
                maximumYield += this.Units.YieldByPeriod[unitIndex, periods];
            }

            return 0.6 * maximumYield / (double)periods;
        }

        protected void GetHarvestVolumes(int[] harvestPeriods, double[] harvestVolumeByPeriod)
        {
            for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
            {
                int periodIndex = harvestPeriods[unitIndex];
                if (periodIndex > 0)
                {
                    harvestVolumeByPeriod[periodIndex] += this.Units.YieldByPeriod[unitIndex, periodIndex];
                }
            }
        }

        protected double GetObjectiveFunction(double[] harvestVolumes)
        {
            // find objective function value
            double objectiveFunction = 0.0F;
            for (int periodIndex = 1; periodIndex < harvestVolumes.Length; ++periodIndex)
            {
                double harvest = harvestVolumes[periodIndex];
                double differenceFromTarget = this.TargetHarvestPerPeriod - harvest;
                double weight = this.TargetHarvestWeights[periodIndex];
                objectiveFunction += weight * differenceFromTarget * differenceFromTarget;
            }
            return objectiveFunction;
        }

        public void RecalculateHarvestVolumes()
        {
            Array.Clear(this.CurrentHarvestByPeriod, 0, this.CurrentHarvestByPeriod.Length);
            this.GetHarvestVolumes(this.CurrentHarvestPeriods, this.CurrentHarvestByPeriod);
        }

        public double RecalculateObjectiveFunction()
        {
            return this.GetObjectiveFunction(this.CurrentHarvestByPeriod);
        }

        public abstract TimeSpan Run();

        protected void UpdateBestSolution(int unitIndex, int previousHarvestPeriod, int currentHarvestPeriod, int movesSinceBestObjectiveImproved)
        {
            if (movesSinceBestObjectiveImproved == 1)
            {
                // incremental update of best solution
                this.BestHarvestPeriods[unitIndex] = currentHarvestPeriod;
                this.BestHarvestByPeriod[previousHarvestPeriod] = this.CurrentHarvestByPeriod[previousHarvestPeriod];
                this.BestHarvestByPeriod[currentHarvestPeriod] = this.CurrentHarvestByPeriod[currentHarvestPeriod];
            }
            else
            {
                // copy current solution since history of moves between it and last best solution is unknown
                Array.Copy(this.CurrentHarvestByPeriod, 0, this.BestHarvestByPeriod, 0, this.CurrentHarvestByPeriod.Length);
                Array.Copy(this.CurrentHarvestPeriods, 0, this.BestHarvestPeriods, 0, this.CurrentHarvestPeriods.Length);
            }
        }
    }
}
