using System;
using System.Diagnostics;

namespace FE640.Heuristics
{
    internal class GeneticPopulation : RandomNumberConsumer
    {
        private readonly double[] matingDistributionFunction;
        private readonly double reservedPopulationProportion;

        public double[] IndividualFitness { get; private set; }
        public double[][] HarvestVolumesByPeriod { get; private set; }
        public int[][] HarvestSchedules { get; private set; }

        public GeneticPopulation(int populationSize, int harvestUnits, int harvestPeriods, double reservedPopulationProportion)
        {
            this.matingDistributionFunction = new double[populationSize];
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

        public GeneticPopulation(GeneticPopulation other)
            : this(other.Size, other.HarvestUnits, other.HarvestPeriods, other.reservedPopulationProportion)
        {
            Array.Copy(other.matingDistributionFunction, 0, this.matingDistributionFunction, 0, this.Size);
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
                if (firstParentCumlativeProbability < matingDistributionFunction[individualIndex])
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
                if (secondParentCumlativeProbability < matingDistributionFunction[individualIndex])
                {
                    secondParentIndex = individualIndex;
                    break;
                }
            }
        }

        public void RecalculateMatingDistributionFunction()
        {
            // find cumulative distribution function (CDF) representing prevalence of individuals in population
            // Since individuals with the lowest fitness values should be most likely to mate their mating likelihood is found using
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
                Debug.Assert(totalDifferencesFromMaximum > -0.000001);
                for (int individualIndex = 0; individualIndex < this.Size; ++individualIndex)
                {
                    this.matingDistributionFunction[individualIndex] = guaranteedProportion;
                }
                return;
            }

            this.matingDistributionFunction[0] = guaranteedProportion + fitnessProportion * (maximumFitness - this.IndividualFitness[0]) / totalDifferencesFromMaximum;
            for (int individualIndex = 1; individualIndex < this.Size; ++individualIndex)
            {
                this.matingDistributionFunction[individualIndex] = matingDistributionFunction[individualIndex - 1];
                this.matingDistributionFunction[individualIndex] += guaranteedProportion + fitnessProportion * (maximumFitness - this.IndividualFitness[individualIndex]) / totalDifferencesFromMaximum;

                Debug.Assert(this.matingDistributionFunction[individualIndex] > this.matingDistributionFunction[individualIndex - 1]);
                Debug.Assert(this.matingDistributionFunction[individualIndex] <= 1.0000001);
            }
        }
    }
}
