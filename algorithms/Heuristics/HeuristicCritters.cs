using System;
using System.Diagnostics;

namespace FE640.Heuristics
{
    internal class HeuristicCritters : RandomNumberConsumer
    {
        private readonly double[] fitnessCumulativeDistributionFunction;
        private readonly double reservedPopulationProportion;

        public double[] FitnessProbabilityDensity { get; private set; }
        public double[] IndividualFitness { get; private set; }
        public double[][] HarvestVolumesByPeriod { get; private set; }
        public int[][] HarvestSchedules { get; private set; }

        public HeuristicCritters(int populationSize, int harvestUnits, int harvestPeriods, double reservedPopulationProportion)
        {
            this.fitnessCumulativeDistributionFunction = new double[populationSize];
            this.FitnessProbabilityDensity = new double[populationSize];
            this.IndividualFitness = new double[populationSize];
            this.HarvestVolumesByPeriod = new double[populationSize][];
            this.HarvestSchedules = new int[populationSize][];
            this.reservedPopulationProportion = reservedPopulationProportion;

            double harvestPeriodScalingFactor = ((double)harvestPeriods - Constant.RoundToZeroTolerance) / (double)byte.MaxValue;
            for (int individualIndex = 0; individualIndex < populationSize; ++individualIndex)
            {
                int[] schedule = new int[harvestUnits];
                for (int unitIndex = 0; unitIndex < harvestUnits; ++unitIndex)
                {
                    schedule[unitIndex] = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
                }
                double[] harvestByPeriod = new double[harvestPeriods + 1];
                this.HarvestVolumesByPeriod[individualIndex] = harvestByPeriod;
                this.HarvestSchedules[individualIndex] = schedule;
            }
        }

        public HeuristicCritters(HeuristicCritters other)
            : this(other.Size, other.HarvestUnits, other.HarvestPeriods, other.reservedPopulationProportion)
        {
            Array.Copy(other.fitnessCumulativeDistributionFunction, 0, this.fitnessCumulativeDistributionFunction, 0, this.Size);
            Array.Copy(other.IndividualFitness, 0, this.IndividualFitness, 0, this.Size);
            for (int individualIndex = 0; individualIndex < other.Size; ++individualIndex)
            {
                Array.Copy(other.HarvestSchedules[individualIndex], 0, this.HarvestSchedules[individualIndex], 0, this.HarvestUnits);
                Array.Copy(other.HarvestVolumesByPeriod[individualIndex], 0, this.HarvestVolumesByPeriod[individualIndex], 0, this.HarvestPeriods + 1);
            }
        }

        public int HarvestPeriods
        {
            get { return this.HarvestVolumesByPeriod[0].Length - 1; }
        }

        public int HarvestUnits
        {
            get { return this.HarvestSchedules[0].Length; }
        }

        public int Size
        {
            get { return this.IndividualFitness.Length; }
        }

        public void FindParents(out int firstParentIndex, out int secondParentIndex)
        {
            // find first parent
            // TODO: check significance of quantization effects from use of two random bytes
            double parentScalingFactor = 1.0 / (double)UInt16.MaxValue;
            double firstParentCumlativeProbability = parentScalingFactor * this.GetTwoPseudorandomBytesAsDouble();
            firstParentIndex = this.Size - 1;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                if (firstParentCumlativeProbability < fitnessCumulativeDistributionFunction[individualIndex])
                {
                    firstParentIndex = individualIndex;
                    break;
                }
            }

            // find second parent
            // TODO: check significance of allowing selfing
            // TOOD: investigate selection pressure effect of choosing second parent randomly
            double secondParentCumlativeProbability = parentScalingFactor * this.GetTwoPseudorandomBytesAsDouble();
            secondParentIndex = this.Size - 1;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                if (secondParentCumlativeProbability < fitnessCumulativeDistributionFunction[individualIndex])
                {
                    secondParentIndex = individualIndex;
                    break;
                }
            }
        }

        public void RandomizeHarvestSchedules()
        {
            double harvestPeriodScalingFactor = ((double)this.HarvestPeriods - Constant.RoundToZeroTolerance) / (double)byte.MaxValue;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                int[] schedule = this.HarvestSchedules[individualIndex];
                for (int unitIndex = 0; unitIndex < this.HarvestUnits; ++unitIndex)
                {
                    schedule[unitIndex] = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
                }
            }
        }

        public void RecalculateFitnessDistribution()
        {
            // find cumulative distribution function (CDF) proportionallyt representing individuals in population by fitness
            // Since individuals with the lowest fitness values should be the represented, their probability is found using
            //    (maxFitness - individualFitness) / (n * maxFitness - totalFitness)
            // as this provides the desired likelihood and, properly scaled, produces a CDF totalling 1.0 when accumulated with the guaranteed
            // minimum fitnesses.
            //
            // The reserved proportion is allocated equally across all individuals and guarantees some minimum presence of low fitness individuals.
            double maximumFitness = double.MinValue;
            double totalFitness = 0.0;
            for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
            {
                double individualFitness = this.IndividualFitness[individualIndex];
                maximumFitness = Math.Max(maximumFitness, individualFitness);
                totalFitness += individualFitness;
            }

            double guaranteedProportion = this.reservedPopulationProportion / this.Size;
            double fitnessProportion = 1.0 - this.reservedPopulationProportion;
            double totalDifferencesFromMaximum = this.Size * maximumFitness - totalFitness;
            if (totalDifferencesFromMaximum <= 0.0)
            {
                // it's possible for all individuals in the population to become clones of a single genotype, in which case the total difference
                // can be exactly zero and the loop below is either poorly condtioned or produces NaNs
                Debug.Assert(Math.Abs(totalDifferencesFromMaximum / totalFitness) < 1E-9);
                for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
                {
                    this.fitnessCumulativeDistributionFunction[individualIndex] = guaranteedProportion;
                }
                return;
            }

            this.fitnessCumulativeDistributionFunction[0] = guaranteedProportion + fitnessProportion * (maximumFitness - this.IndividualFitness[0]) / totalDifferencesFromMaximum;
            for (int individualIndex = 1; individualIndex < this.Size; ++individualIndex)
            {
                double individualProbability = guaranteedProportion + fitnessProportion * (maximumFitness - this.IndividualFitness[individualIndex]) / totalDifferencesFromMaximum;
                this.FitnessProbabilityDensity[individualIndex] = individualProbability;
                this.fitnessCumulativeDistributionFunction[individualIndex] = this.fitnessCumulativeDistributionFunction[individualIndex - 1];
                this.fitnessCumulativeDistributionFunction[individualIndex] += individualProbability;

                Debug.Assert(this.fitnessCumulativeDistributionFunction[individualIndex] > this.fitnessCumulativeDistributionFunction[individualIndex - 1]);
                Debug.Assert(this.fitnessCumulativeDistributionFunction[individualIndex] <= 1.0000001);
            }
        }
    }
}
