using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FE640.Heuristics
{
    public class RecordToRecordTravel : Heuristic
    {
        public double Deviation { get; set; }
        public double InfeasibilityPenalty { get; set; }
        public int MaximumInfeasibleUnits { get; set; }
        public int StopAfter { get; set; }

        public RecordToRecordTravel(HarvestUnits units)
            : base(units)
        {
            this.Deviation = 1000.0 * 1000.0;
            this.InfeasibilityPenalty = 100.0 * 1000.0;
            this.MaximumInfeasibleUnits = 0;
            this.StopAfter = 25000;

            this.ObjectiveFunctionByIteration = new List<double>(1000 * 1000)
            {
                this.BestObjectiveFunction
            };
        }

        #if DEBUG
        private float GetLargestOpeningSize(int unitIndex, int candidateHarvestPeriod)
        {
            float largestOpeningSize = 0.0F;
            for (int openingPeriod = candidateHarvestPeriod; openingPeriod <= candidateHarvestPeriod + this.Units.GreenUpInPeriods; ++openingPeriod)
            {
                bool[] openingStatusEvaluated = new bool[this.Units.Count];
                openingStatusEvaluated[unitIndex] = true;
                float openingSizeForPeriod = this.Units.UnitSize;
                this.GetAdjacentOpeningSize(unitIndex, openingPeriod, openingStatusEvaluated, ref openingSizeForPeriod);
                largestOpeningSize = Math.Max(largestOpeningSize, openingSizeForPeriod);
            }
            return largestOpeningSize;
        }

        // opening is passed as a ref in order to allow early depth first search termination
        // This is 94% faster than returning the total adjacent opening size.
        private void GetAdjacentOpeningSize(int unitIndex, int openingPeriod, bool[] openingStatusEvaluated, ref float openingSize)
        {
            int maximumAdjacentUnits = this.Units.AdjacencyByUnit.GetLength(1);
            for (int adjacencyIndex = 0; adjacencyIndex < maximumAdjacentUnits; ++adjacencyIndex)
            {
                int adjacentUnitIndex = this.Units.AdjacencyByUnit[unitIndex, adjacencyIndex];
                if (adjacentUnitIndex < 0)
                {
                    break;
                }
                if (openingStatusEvaluated[adjacentUnitIndex])
                {
                    continue;
                }

                int adjacentUnitHarvestPeriod = this.CurrentHarvestPeriods[adjacentUnitIndex];
                openingStatusEvaluated[adjacentUnitIndex] = true;
                if ((adjacentUnitHarvestPeriod > 0) && (adjacentUnitHarvestPeriod <= openingPeriod) && (adjacentUnitHarvestPeriod + this.Units.GreenUpInPeriods >= openingPeriod))
                {
                    openingSize += this.Units.UnitSize;
                    if (openingSize > this.Units.MaximumOpeningSize)
                    {
                        break;
                    }
                    this.GetAdjacentOpeningSize(adjacentUnitIndex, openingPeriod, openingStatusEvaluated, ref openingSize);
                }
            }
        }
        #endif

        public override TimeSpan Run()
        {
            if (this.Deviation <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Deviation));
            }
            if (this.StopAfter <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.StopAfter));
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (this.Units.HasAdjacency)
            {
                if (this.MaximumInfeasibleUnits > 0)
                {
                    this.RunWithInfeasibleAdjacency();
                }
                else
                {
                    this.RunWithAdjacency();
                }
            }
            else
            {
                this.RunNonSpatial();
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        private void RunNonSpatial()
        {
            double maximumAcceptableObjectiveFunction = this.BestObjectiveFunction + this.Deviation;
            double currentObjectiveFunction = this.BestObjectiveFunction;
            double harvestPeriodScalingFactor = ((double)this.CurrentHarvestByPeriod.Length - 1.0 - Constant.RoundToZeroTolerance) / (double)byte.MaxValue;
            int iterationsSinceBestObjectiveImproved = 0;
            int movesSinceBestObjectiveImproved = 0;
            double unitIndexScalingFactor = ((double)this.Units.Count - Constant.RoundToZeroTolerance) / (double)UInt16.MaxValue;

            while (iterationsSinceBestObjectiveImproved < this.StopAfter)
            {
                int unitIndex = (int)(unitIndexScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                int currentHarvestPeriod = this.CurrentHarvestPeriods[unitIndex];
                int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
                while (candidateHarvestPeriod == currentHarvestPeriod)
                {
                    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
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

                    // see remarks in RunWithAdjacency()
                    double objectiveFunctionRatio = currentObjectiveFunction / this.BestObjectiveFunction;
                    if (objectiveFunctionRatio < Constant.MinimumObjectiveRatioRequiredForImprovement)
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
        }

        private void RunWithAdjacency()
        {
            double maximumAcceptableObjectiveFunction = this.BestObjectiveFunction + this.Deviation;
            double currentObjectiveFunction = this.BestObjectiveFunction;
            double harvestPeriodScalingFactor = ((double)this.CurrentHarvestByPeriod.Length - 1.0 - Constant.RoundToZeroTolerance) / (double)byte.MaxValue;
            int iterationsSinceBestObjectiveImproved = 0;
            int movesSinceBestObjectiveImproved = 0;
            int maximumAdjacentUnits = this.Units.AdjacencyByUnit.GetLength(1);
            int periods = this.Units.HarvestPeriods;
            bool[] openingStatusEvaluated = new bool[this.Units.Count];
            double unitIndexScalingFactor = ((double)this.Units.Count - Constant.RoundToZeroTolerance) / (double)UInt16.MaxValue;

            while (iterationsSinceBestObjectiveImproved < this.StopAfter)
            {
                int unitIndex = (int)(unitIndexScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                int currentHarvestPeriod = this.CurrentHarvestPeriods[unitIndex];
                int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
                while (candidateHarvestPeriod == currentHarvestPeriod)
                {
                    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
                }

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
                    // check if harvesting this unit results in an opening smaller than the maximum allowable opening size
                    // This is a depth first search with loop unrolling to avoid function call overhead in recursion.
                    // TODO: profile breadth versus depth first search?
                    float candidateOpeningSize = 0.0F;
                    int maximumPeriod = Math.Min(candidateHarvestPeriod + this.Units.GreenUpInPeriods, periods);
                    for (int openingPeriod = candidateHarvestPeriod; openingPeriod <= maximumPeriod; ++openingPeriod)
                    {
                        int minimumUnitIndex = unitIndex;
                        int maximumUnitIndex = unitIndex;

                        float openingSize = this.Units.UnitSize;
                        openingStatusEvaluated[unitIndex] = true;
                        // recursive depth first search: only ~2% slower
                        //this.GetAdjacentOpeningSize(unitIndex, openingPeriod, openingStatusEvaluated, ref openingSize);
                        // loop unrolled depth first search
                        for (int adjacencyIndexDepth1 = 0; adjacencyIndexDepth1 < maximumAdjacentUnits; ++adjacencyIndexDepth1)
                        {
                            int adjacentUnitIndexDepth1 = this.Units.AdjacencyByUnit[unitIndex, adjacencyIndexDepth1];
                            if (adjacentUnitIndexDepth1 < 0)
                            {
                                break;
                            }
                            if (openingStatusEvaluated[adjacentUnitIndexDepth1])
                            {
                                continue;
                            }

                            int adjacentUnitHarvestPeriodDepth1 = this.CurrentHarvestPeriods[adjacentUnitIndexDepth1];
                            openingStatusEvaluated[adjacentUnitIndexDepth1] = true;
                            minimumUnitIndex = Math.Min(minimumUnitIndex, adjacentUnitIndexDepth1);
                            maximumUnitIndex = Math.Max(maximumUnitIndex, adjacentUnitIndexDepth1);
                            if ((adjacentUnitHarvestPeriodDepth1 > 0) && (adjacentUnitHarvestPeriodDepth1 <= openingPeriod) && (adjacentUnitHarvestPeriodDepth1 + this.Units.GreenUpInPeriods >= openingPeriod))
                            {
                                openingSize += this.Units.UnitSize;
                                if (openingSize > this.Units.MaximumOpeningSize)
                                {
                                    break;
                                }
                                for (int adjacencyIndexDepth2 = 0; adjacencyIndexDepth2 < maximumAdjacentUnits; ++adjacencyIndexDepth2)
                                {
                                    int adjacentUnitIndexDepth2 = this.Units.AdjacencyByUnit[adjacentUnitIndexDepth1, adjacencyIndexDepth2];
                                    if (adjacentUnitIndexDepth2 < 0)
                                    {
                                        break;
                                    }
                                    if (openingStatusEvaluated[adjacentUnitIndexDepth2])
                                    {
                                        continue;
                                    }

                                    int adjacentUnitHarvestPeriodDepth2 = this.CurrentHarvestPeriods[adjacentUnitIndexDepth2];
                                    openingStatusEvaluated[adjacentUnitIndexDepth2] = true;
                                    minimumUnitIndex = Math.Min(minimumUnitIndex, adjacentUnitIndexDepth2);
                                    maximumUnitIndex = Math.Max(maximumUnitIndex, adjacentUnitIndexDepth2);
                                    if ((adjacentUnitHarvestPeriodDepth2 > 0) && (adjacentUnitHarvestPeriodDepth2 <= openingPeriod) && (adjacentUnitHarvestPeriodDepth2 + this.Units.GreenUpInPeriods >= openingPeriod))
                                    {
                                        openingSize += this.Units.UnitSize;
                                        if (openingSize > this.Units.MaximumOpeningSize)
                                        {
                                            break;
                                        }
                                        for (int adjacencyIndexDepth3 = 0; adjacencyIndexDepth3 < maximumAdjacentUnits; ++adjacencyIndexDepth3)
                                        {
                                            int adjacentUnitIndexDepth3 = this.Units.AdjacencyByUnit[adjacentUnitIndexDepth2, adjacencyIndexDepth3];
                                            if (adjacentUnitIndexDepth3 < 0)
                                            {
                                                break;
                                            }
                                            if (openingStatusEvaluated[adjacentUnitIndexDepth3])
                                            {
                                                continue;
                                            }

                                            int adjacentUnitHarvestPeriodDepth3 = this.CurrentHarvestPeriods[adjacentUnitIndexDepth3];
                                            openingStatusEvaluated[adjacentUnitIndexDepth3] = true;
                                            minimumUnitIndex = Math.Min(minimumUnitIndex, adjacentUnitIndexDepth3);
                                            maximumUnitIndex = Math.Max(maximumUnitIndex, adjacentUnitIndexDepth3);
                                            if ((adjacentUnitHarvestPeriodDepth3 > 0) && (adjacentUnitHarvestPeriodDepth3 <= openingPeriod) && (adjacentUnitHarvestPeriodDepth3 + this.Units.GreenUpInPeriods >= openingPeriod))
                                            {
                                                openingSize += this.Units.UnitSize;
                                                if (openingSize > this.Units.MaximumOpeningSize)
                                                {
                                                    break;
                                                }
                                                for (int adjacencyIndexDepth4 = 0; adjacencyIndexDepth4 < maximumAdjacentUnits; ++adjacencyIndexDepth4)
                                                {
                                                    int adjacentUnitIndexDepth4 = this.Units.AdjacencyByUnit[adjacentUnitIndexDepth3, adjacencyIndexDepth4];
                                                    if (adjacentUnitIndexDepth4 < 0)
                                                    {
                                                        break;
                                                    }
                                                    if (openingStatusEvaluated[adjacentUnitIndexDepth4])
                                                    {
                                                        continue;
                                                    }

                                                    int adjacentUnitHarvestPeriodDepth4 = this.CurrentHarvestPeriods[adjacentUnitIndexDepth4];
                                                    openingStatusEvaluated[adjacentUnitIndexDepth4] = true;
                                                    minimumUnitIndex = Math.Min(minimumUnitIndex, adjacentUnitIndexDepth4);
                                                    maximumUnitIndex = Math.Max(maximumUnitIndex, adjacentUnitIndexDepth4);
                                                    if ((adjacentUnitHarvestPeriodDepth4 > 0) && (adjacentUnitHarvestPeriodDepth4 <= openingPeriod) && (adjacentUnitHarvestPeriodDepth4 + this.Units.GreenUpInPeriods >= openingPeriod))
                                                    {
                                                        openingSize += this.Units.UnitSize;
                                                        if (openingSize > this.Units.MaximumOpeningSize)
                                                        {
                                                            break;
                                                        }
                                                        throw new NotImplementedException();
                                                    }
                                                }
                                                // ~5% slowdown from moving these end of loop checks into the for loop headers
                                                if (openingSize > this.Units.MaximumOpeningSize)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        if (openingSize > this.Units.MaximumOpeningSize)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (openingSize > this.Units.MaximumOpeningSize)
                                {
                                    break;
                                }
                            }
                        }

                        candidateOpeningSize = Math.Max(candidateOpeningSize, openingSize);
                        // no clear performance improvement for tracking minimum and maximum unit indices to minimizing clear cost at 100 units
                        // most likely no detriment either, though
                        Array.Clear(openingStatusEvaluated, minimumUnitIndex, maximumUnitIndex - minimumUnitIndex + 1);
                    }
                    #if DEBUG
                    //float candidateOpeningSize = this.GetOpeningSize(unitIndex, candidateHarvestPeriod);
                    //this.Units.SetCurrentSchedule(this);
                    //this.Units.HarvestPeriods[unitIndex] = candidateHarvestPeriod;
                    //float checkOpeningSize = 0.0F;
                    //for (int openingPeriod = candidateHarvestPeriod; openingPeriod <= candidateHarvestPeriod + this.Units.GreenUpInPeriods; ++openingPeriod)
                    //{
                    //    checkOpeningSize = Math.Max(checkOpeningSize, this.Units.GetOpeningSize(unitIndex, openingPeriod));
                    //}
                    float checkOpeningSize = this.GetLargestOpeningSize(unitIndex, candidateHarvestPeriod);
                    if (checkOpeningSize <= this.Units.MaximumOpeningSize)
                    {
                        Debug.Assert(candidateOpeningSize == checkOpeningSize);
                    }
                    else
                    {
                        // candidate opening size evaluation should halt at the first unit which exceeds the maximum opening size
                        Debug.Assert(candidateOpeningSize == this.Units.MaximumOpeningSize + this.Units.UnitSize);
                        Debug.Assert(candidateOpeningSize <= checkOpeningSize);
                    }
                    #endif
                    Debug.Assert(candidateOpeningSize >= this.Units.UnitSize);
                    Debug.Assert(candidateOpeningSize <= this.Units.UnitSize * this.Units.Count);
                    bool openingFeasible = candidateOpeningSize <= this.Units.MaximumOpeningSize ? true : false;

                    if (openingFeasible)
                    {
                        // accept move
                        this.CurrentHarvestPeriods[unitIndex] = candidateHarvestPeriod;
                        this.CurrentHarvestByPeriod[candidateHarvestPeriod] += candidateYield;
                        this.CurrentHarvestByPeriod[currentHarvestPeriod] -= currentYield;
                        this.MaximumOpeningSize = Math.Max(this.MaximumOpeningSize, candidateOpeningSize);
                        currentObjectiveFunction = candidateObjectiveFunction;
                        ++movesSinceBestObjectiveImproved;
                        Debug.Assert(this.CurrentHarvestByPeriod[candidateHarvestPeriod] >= 0.0F);
                        Debug.Assert(this.CurrentHarvestByPeriod[currentHarvestPeriod] >= 0.0F);
                        Debug.Assert(currentObjectiveFunction >= 0.0F);

                        #if DEBUG
                        // check incremental opening size calculation for move just accepted against a full recalculation
                        // Computationally expensive but useful in catching incremental errors.
                        this.Units.SetCurrentSchedule(this);
                        OpeningSizes openingSizes = this.Units.GetMaximumOpeningSizesByPeriod();
                        for (int planningPeriod = 0; planningPeriod < this.Units.HarvestPeriods + 1; ++planningPeriod)
                        {
                            float maxOpeningSize = openingSizes.MaximumOpeningSizeByPeriod[planningPeriod];
                            if (maxOpeningSize > this.Units.MaximumOpeningSize)
                            {
                                StringBuilder harvestPeriods = new StringBuilder();
                                foreach (int harvestPeriod in this.CurrentHarvestPeriods)
                                {
                                    harvestPeriods.AppendLine(harvestPeriod.ToString());
                                }

                                int testUnitIndex = 28;
                                int testHarvestPeriod = 5;
                                float testUnitOpening = this.Units.GetOpeningSize(testUnitIndex, testHarvestPeriod);

                                Debug.Assert(false);
                            }
                        }
                        #endif

                        // if a minima acts as a long period attractor the objective function will differ on revisit due to numerical precision
                        // In cases where roundoff results in the objective decreasing, a tolerance is required to avoid continual resets of
                        // iterationsSinceBestObjectiveImproved and allow termination.
                        double objectiveFunctionRatio = currentObjectiveFunction / this.BestObjectiveFunction;
                        if (objectiveFunctionRatio < Constant.MinimumObjectiveRatioRequiredForImprovement)
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
                }

                this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
            }
        }

        private void RunWithInfeasibleAdjacency()
        {
            double maximumAcceptableObjectiveFunction = this.BestObjectiveFunction + this.Deviation;
            double currentObjectiveFunction = this.BestObjectiveFunction;
            double harvestPeriodScalingFactor = ((double)this.CurrentHarvestByPeriod.Length - 1.0 - Constant.RoundToZeroTolerance) / (double)byte.MaxValue;
            bool[] hasInfeasibleOpening = new bool[this.Units.Count];
            int infeasibleUnitCount = 0;
            int iterationsSinceBestObjectiveImproved = 0;
            int movesSinceBestObjectiveImproved = 0;
            int maximumAdjacentUnits = this.Units.AdjacencyByUnit.GetLength(1);
            int periods = this.Units.HarvestPeriods;
            bool[] openingStatusEvaluated = new bool[this.Units.Count];
            double unitIndexScalingFactor = ((double)this.Units.Count - Constant.RoundToZeroTolerance) / (double)UInt16.MaxValue;

            while (iterationsSinceBestObjectiveImproved < this.StopAfter)
            {
                int unitIndex = (int)(unitIndexScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                int currentHarvestPeriod = this.CurrentHarvestPeriods[unitIndex];
                int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
                while (candidateHarvestPeriod == currentHarvestPeriod)
                {
                    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
                }

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
                    // check if harvesting this unit results in an opening smaller than the maximum allowable opening size
                    // This is a depth first search with loop unrolling to avoid function call overhead in recursion.
                    // TODO: profile breadth versus depth first search?
                    float candidateOpeningSize = 0.0F;
                    int maximumPeriod = Math.Min(candidateHarvestPeriod + this.Units.GreenUpInPeriods, periods);
                    for (int openingPeriod = candidateHarvestPeriod; openingPeriod <= maximumPeriod; ++openingPeriod)
                    {
                        int minimumUnitIndex = unitIndex;
                        int maximumUnitIndex = unitIndex;

                        float openingSize = this.Units.UnitSize;
                        openingStatusEvaluated[unitIndex] = true;
                        // recursive depth first search: only ~2% slower
                        //this.GetAdjacentOpeningSize(unitIndex, openingPeriod, openingStatusEvaluated, ref openingSize);
                        // loop unrolled depth first search
                        for (int adjacencyIndexDepth1 = 0; adjacencyIndexDepth1 < maximumAdjacentUnits; ++adjacencyIndexDepth1)
                        {
                            int adjacentUnitIndexDepth1 = this.Units.AdjacencyByUnit[unitIndex, adjacencyIndexDepth1];
                            if (adjacentUnitIndexDepth1 < 0)
                            {
                                break;
                            }
                            if (openingStatusEvaluated[adjacentUnitIndexDepth1])
                            {
                                continue;
                            }

                            int adjacentUnitHarvestPeriodDepth1 = this.CurrentHarvestPeriods[adjacentUnitIndexDepth1];
                            openingStatusEvaluated[adjacentUnitIndexDepth1] = true;
                            minimumUnitIndex = Math.Min(minimumUnitIndex, adjacentUnitIndexDepth1);
                            maximumUnitIndex = Math.Max(maximumUnitIndex, adjacentUnitIndexDepth1);
                            if ((adjacentUnitHarvestPeriodDepth1 > 0) && (adjacentUnitHarvestPeriodDepth1 <= openingPeriod) && (adjacentUnitHarvestPeriodDepth1 + this.Units.GreenUpInPeriods >= openingPeriod))
                            {
                                openingSize += this.Units.UnitSize;
                                if (openingSize > this.Units.MaximumOpeningSize)
                                {
                                    break;
                                }
                                for (int adjacencyIndexDepth2 = 0; adjacencyIndexDepth2 < maximumAdjacentUnits; ++adjacencyIndexDepth2)
                                {
                                    int adjacentUnitIndexDepth2 = this.Units.AdjacencyByUnit[adjacentUnitIndexDepth1, adjacencyIndexDepth2];
                                    if (adjacentUnitIndexDepth2 < 0)
                                    {
                                        break;
                                    }
                                    if (openingStatusEvaluated[adjacentUnitIndexDepth2])
                                    {
                                        continue;
                                    }

                                    int adjacentUnitHarvestPeriodDepth2 = this.CurrentHarvestPeriods[adjacentUnitIndexDepth2];
                                    openingStatusEvaluated[adjacentUnitIndexDepth2] = true;
                                    minimumUnitIndex = Math.Min(minimumUnitIndex, adjacentUnitIndexDepth2);
                                    maximumUnitIndex = Math.Max(maximumUnitIndex, adjacentUnitIndexDepth2);
                                    if ((adjacentUnitHarvestPeriodDepth2 > 0) && (adjacentUnitHarvestPeriodDepth2 <= openingPeriod) && (adjacentUnitHarvestPeriodDepth2 + this.Units.GreenUpInPeriods >= openingPeriod))
                                    {
                                        openingSize += this.Units.UnitSize;
                                        if (openingSize > this.Units.MaximumOpeningSize)
                                        {
                                            break;
                                        }
                                        for (int adjacencyIndexDepth3 = 0; adjacencyIndexDepth3 < maximumAdjacentUnits; ++adjacencyIndexDepth3)
                                        {
                                            int adjacentUnitIndexDepth3 = this.Units.AdjacencyByUnit[adjacentUnitIndexDepth2, adjacencyIndexDepth3];
                                            if (adjacentUnitIndexDepth3 < 0)
                                            {
                                                break;
                                            }
                                            if (openingStatusEvaluated[adjacentUnitIndexDepth3])
                                            {
                                                continue;
                                            }

                                            int adjacentUnitHarvestPeriodDepth3 = this.CurrentHarvestPeriods[adjacentUnitIndexDepth3];
                                            openingStatusEvaluated[adjacentUnitIndexDepth3] = true;
                                            minimumUnitIndex = Math.Min(minimumUnitIndex, adjacentUnitIndexDepth3);
                                            maximumUnitIndex = Math.Max(maximumUnitIndex, adjacentUnitIndexDepth3);
                                            if ((adjacentUnitHarvestPeriodDepth3 > 0) && (adjacentUnitHarvestPeriodDepth3 <= openingPeriod) && (adjacentUnitHarvestPeriodDepth3 + this.Units.GreenUpInPeriods >= openingPeriod))
                                            {
                                                openingSize += this.Units.UnitSize;
                                                if (openingSize > this.Units.MaximumOpeningSize)
                                                {
                                                    break;
                                                }
                                                for (int adjacencyIndexDepth4 = 0; adjacencyIndexDepth4 < maximumAdjacentUnits; ++adjacencyIndexDepth4)
                                                {
                                                    int adjacentUnitIndexDepth4 = this.Units.AdjacencyByUnit[adjacentUnitIndexDepth3, adjacencyIndexDepth4];
                                                    if (adjacentUnitIndexDepth4 < 0)
                                                    {
                                                        break;
                                                    }
                                                    if (openingStatusEvaluated[adjacentUnitIndexDepth4])
                                                    {
                                                        continue;
                                                    }

                                                    int adjacentUnitHarvestPeriodDepth4 = this.CurrentHarvestPeriods[adjacentUnitIndexDepth4];
                                                    openingStatusEvaluated[adjacentUnitIndexDepth4] = true;
                                                    minimumUnitIndex = Math.Min(minimumUnitIndex, adjacentUnitIndexDepth4);
                                                    maximumUnitIndex = Math.Max(maximumUnitIndex, adjacentUnitIndexDepth4);
                                                    if ((adjacentUnitHarvestPeriodDepth4 > 0) && (adjacentUnitHarvestPeriodDepth4 <= openingPeriod) && (adjacentUnitHarvestPeriodDepth4 + this.Units.GreenUpInPeriods >= openingPeriod))
                                                    {
                                                        openingSize += this.Units.UnitSize;
                                                        if (openingSize > this.Units.MaximumOpeningSize)
                                                        {
                                                            break;
                                                        }
                                                        throw new NotImplementedException();
                                                    }
                                                }
                                                // ~5% slowdown from moving these end of loop checks into the for loop headers
                                                if (openingSize > this.Units.MaximumOpeningSize)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        if (openingSize > this.Units.MaximumOpeningSize)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (openingSize > this.Units.MaximumOpeningSize)
                                {
                                    break;
                                }
                            }
                        }

                        candidateOpeningSize = Math.Max(candidateOpeningSize, openingSize);
                        // no clear performance improvement for tracking minimum and maximum unit indices to minimizing clear cost at 100 units
                        // most likely no detriment either, though
                        Array.Clear(openingStatusEvaluated, minimumUnitIndex, maximumUnitIndex - minimumUnitIndex + 1);
                    }
                    Debug.Assert(candidateOpeningSize >= this.Units.UnitSize);
                    Debug.Assert(candidateOpeningSize <= this.Units.UnitSize * this.Units.Count);
                    bool candidateInfeasible = candidateOpeningSize > this.Units.MaximumOpeningSize;
                    bool currentlyInfeasible = hasInfeasibleOpening[unitIndex];
                    int infeasibleUnitCountChange = 0;
                    if ((candidateInfeasible == true) && (currentlyInfeasible == false))
                    {
                        if (infeasibleUnitCount < this.MaximumInfeasibleUnits)
                        {
                            candidateObjectiveFunction += this.InfeasibilityPenalty;
                        }
                        else
                        {
                            candidateObjectiveFunction = Double.MaxValue;
                        }
                        infeasibleUnitCountChange = 1;
                    }
                    else if ((candidateInfeasible == false) && (currentlyInfeasible == true))
                    {
                        candidateObjectiveFunction -= this.InfeasibilityPenalty;
                        infeasibleUnitCountChange = -1;
                    }

                    if (candidateObjectiveFunction < maximumAcceptableObjectiveFunction)
                    {
                        // accept move
                        this.CurrentHarvestPeriods[unitIndex] = candidateHarvestPeriod;
                        this.CurrentHarvestByPeriod[candidateHarvestPeriod] += candidateYield;
                        this.CurrentHarvestByPeriod[currentHarvestPeriod] -= currentYield;
                        this.MaximumOpeningSize = Math.Max(this.MaximumOpeningSize, candidateOpeningSize);
                        currentObjectiveFunction = candidateObjectiveFunction;

                        infeasibleUnitCount += infeasibleUnitCountChange;
                        hasInfeasibleOpening[unitIndex] = candidateInfeasible;
                        ++movesSinceBestObjectiveImproved;
                        Debug.Assert(this.CurrentHarvestByPeriod[candidateHarvestPeriod] >= 0.0F);
                        Debug.Assert(this.CurrentHarvestByPeriod[currentHarvestPeriod] >= 0.0F);
                        Debug.Assert(currentObjectiveFunction >= 0.0F);

                        // if a minima acts as a long period attractor the objective function will differ on revisit due to numerical precision
                        // In cases where roundoff results in the objective decreasing, a tolerance is required to avoid continual resets of
                        // iterationsSinceBestObjectiveImproved and allow termination.
                        double objectiveFunctionRatio = currentObjectiveFunction / this.BestObjectiveFunction;
                        if (objectiveFunctionRatio < Constant.MinimumObjectiveRatioRequiredForImprovement)
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
                }

                this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
            }
        }
    }
}
