using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640.Heuristics
{
    public class GeneticAlgorithm : Heuristic
    {
        public double EndStandardDeviation { get; set; }
        public int MaximumGenerations { get; set; }
        public double MutationProbability { get; set; }
        public int PopulationSize { get; set; }
        public double ReservedPopulationProportion { get; set; }

        public GeneticAlgorithm(HarvestUnits units)
            : base(units)
        {
            this.EndStandardDeviation = 1.0;
            this.MaximumGenerations = 50;
            this.MutationProbability = 0.005;
            this.PopulationSize = 50;
            this.ReservedPopulationProportion = 0.1;

            this.ObjectiveFunctionByIteration = new List<double>(this.MaximumGenerations);
        }

        private double GetVarianceAndMaybeUpdateBestSolution(GeneticPopulation generation)
        {
            int fittestIndividualIndex = -1;
            double lowestFitness = double.MaxValue;
            double sum = 0.0;
            double sumOfSquares = 0.0;
            for (int individualIndex = 1; individualIndex < generation.Size; ++individualIndex)
            {
                double individualFitness = generation.IndividualFitness[individualIndex];
                sum += individualFitness;
                sumOfSquares += individualFitness * individualFitness;
                if (individualFitness < lowestFitness)
                {
                    fittestIndividualIndex = individualIndex;
                    lowestFitness = individualFitness;
                }
            }
            this.ObjectiveFunctionByIteration.Add(lowestFitness);

            if (lowestFitness < this.BestObjectiveFunction)
            {
                this.BestObjectiveFunction = lowestFitness;
                this.BestHarvestPeriods = generation.HarvestSchedules[fittestIndividualIndex];
                this.BestHarvestByPeriod = generation.HarvestVolumesByPeriod[fittestIndividualIndex];
            }

            double n = (double)generation.Size;
            double meanHarvest = sum / n;
            double variance = sumOfSquares / n - meanHarvest * meanHarvest;
            return variance;
        }

        public override TimeSpan Run()
        {
            if (this.EndStandardDeviation <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.EndStandardDeviation));
            }
            if (this.MaximumGenerations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MaximumGenerations));
            }
            if (this.PopulationSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.PopulationSize));
            }
            if ((this.ReservedPopulationProportion < 0.0) || (this.ReservedPopulationProportion > 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.ReservedPopulationProportion));
            }
            if (this.Units.HasAdjacency)
            {
                throw new NotSupportedException();
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // begin with population of random harvest schedules
            // TODO: should incoming default schedule on this.Units be one of the individuals in the population?
            GeneticPopulation currentGeneration = new GeneticPopulation(this.PopulationSize, this.Units.Count, this.Units.HarvestPeriods, this.ReservedPopulationProportion);
            for (int individualIndex = 0; individualIndex < this.PopulationSize; ++individualIndex)
            {
                int[] schedule = currentGeneration.HarvestSchedules[individualIndex];
                double[] harvestByPeriod = currentGeneration.HarvestVolumesByPeriod[individualIndex];
                this.GetHarvestVolumes(schedule, harvestByPeriod);
                currentGeneration.IndividualFitness[individualIndex] = this.GetObjectiveFunction(harvestByPeriod);
            }

            // for each generation of size n, perform n fertile matings
            double endVariance = this.EndStandardDeviation * this.EndStandardDeviation;
            double unitScalingFactor = ((double)this.Units.Count - Constant.RoundToZeroTolerance) / (double)UInt16.MaxValue;
            double mutationScalingFactor = 1.0 / (double)UInt16.MaxValue;
            double variance = this.GetVarianceAndMaybeUpdateBestSolution(currentGeneration);
            GeneticPopulation nextGeneration = new GeneticPopulation(currentGeneration);
            for (int generationIndex = 1; (generationIndex < this.MaximumGenerations) && (variance > endVariance); ++generationIndex)
            {
                currentGeneration.RecalculateMatingDistributionFunction();
                for (int matingIndex = 0; matingIndex < currentGeneration.Size; ++matingIndex)
                {
                    // crossover parents' genetic material to create offsprings' genetic material
                    currentGeneration.FindParents(out int firstParentIndex, out int secondParentIndex);
                    int crossoverPosition = (int)(unitScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                    int[] firstParentHarvestSchedule = currentGeneration.HarvestSchedules[firstParentIndex];
                    int[] secondParentHarvestSchedule = currentGeneration.HarvestSchedules[secondParentIndex];
                    int[] firstChildHarvestSchedule = new int[this.Units.Count];
                    int[] secondChildHarvestSchedule = new int[this.Units.Count];
                    for (int unitIndex = 0; unitIndex < crossoverPosition; ++unitIndex)
                    {
                        firstChildHarvestSchedule[unitIndex] = firstParentHarvestSchedule[unitIndex];
                        secondChildHarvestSchedule[unitIndex] = secondParentHarvestSchedule[unitIndex];
                    }
                    for (int unitIndex = crossoverPosition; unitIndex < this.Units.Count; ++unitIndex)
                    {
                        firstChildHarvestSchedule[unitIndex] = secondParentHarvestSchedule[unitIndex];
                        secondChildHarvestSchedule[unitIndex] = firstParentHarvestSchedule[unitIndex];
                    }

                    // maybe perform mutations
                    // TODO: investigate effect of mutations other than 2-opt exchange
                    double firstProbability = mutationScalingFactor * this.GetTwoPseudorandomBytesAsDouble();
                    if (firstProbability < this.MutationProbability)
                    {
                        int firstUnitIndex = (int)(unitScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                        int secondUnitIndex = (int)(unitScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                        int harvestPeriod = firstChildHarvestSchedule[firstUnitIndex];
                        firstChildHarvestSchedule[firstUnitIndex] = firstChildHarvestSchedule[secondUnitIndex];
                        firstChildHarvestSchedule[secondUnitIndex] = harvestPeriod;
                    }
                    double secondProbability = mutationScalingFactor * this.GetTwoPseudorandomBytesAsDouble();
                    if (secondProbability < this.MutationProbability)
                    {
                        int firstUnitIndex = (int)(unitScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                        int secondUnitIndex = (int)(unitScalingFactor * this.GetTwoPseudorandomBytesAsDouble());
                        int harvestPeriod = secondParentHarvestSchedule[firstUnitIndex];
                        secondChildHarvestSchedule[firstUnitIndex] = secondChildHarvestSchedule[secondUnitIndex];
                        secondChildHarvestSchedule[secondUnitIndex] = harvestPeriod;
                    }

                    // evaluate fitness of offspring
                    double[] firstChildHarvestVolumeByPeriod = new double[currentGeneration.HarvestPeriods + 1];
                    this.GetHarvestVolumes(firstChildHarvestSchedule, firstChildHarvestVolumeByPeriod);
                    double firstChildFitness = this.GetObjectiveFunction(firstChildHarvestVolumeByPeriod);

                    double[] secondChildHarvestVolumeByPeriod = new double[currentGeneration.HarvestPeriods + 1];
                    this.GetHarvestVolumes(secondChildHarvestSchedule, secondChildHarvestVolumeByPeriod);
                    double secondChildFitness = this.GetObjectiveFunction(secondChildHarvestVolumeByPeriod);

                    // identify the fittest individual among the two parents and the two offspring and place it in the next generation
                    double firstParentFitness = currentGeneration.IndividualFitness[firstParentIndex];
                    double secondParentFitness = currentGeneration.IndividualFitness[secondParentIndex];

                    bool firstChildFittest = firstChildFitness < secondChildFitness;
                    double fittestChildFitness = firstChildFittest ? firstChildFitness : secondChildFitness;
                    bool firstParentFittest = firstParentFitness < secondParentFitness;
                    double fittestParentFitness = firstParentFittest ? firstParentFitness : secondParentFitness;

                    if (fittestChildFitness < fittestParentFitness)
                    {
                        // fittest individual is a child
                        nextGeneration.IndividualFitness[matingIndex] = fittestChildFitness;
                        if (firstChildFittest)
                        {
                            nextGeneration.HarvestSchedules[matingIndex] = firstChildHarvestSchedule;
                            nextGeneration.HarvestVolumesByPeriod[matingIndex] = firstChildHarvestVolumeByPeriod;
                        }
                        else
                        {
                            nextGeneration.HarvestSchedules[matingIndex] = secondChildHarvestSchedule;
                            nextGeneration.HarvestVolumesByPeriod[matingIndex] = secondChildHarvestVolumeByPeriod;
                        }
                    }
                    else
                    {
                        // fittest individual is a parent
                        nextGeneration.IndividualFitness[matingIndex] = fittestParentFitness;
                        if (firstParentFittest)
                        {
                            nextGeneration.HarvestSchedules[matingIndex] = firstParentHarvestSchedule;
                            nextGeneration.HarvestVolumesByPeriod[matingIndex] = currentGeneration.HarvestVolumesByPeriod[firstParentIndex];
                        }
                        else
                        {
                            nextGeneration.HarvestSchedules[matingIndex] = secondParentHarvestSchedule;
                            nextGeneration.HarvestVolumesByPeriod[matingIndex] = currentGeneration.HarvestVolumesByPeriod[secondParentIndex];
                        }
                    }
                }

                GeneticPopulation generationSwapPointer = currentGeneration;
                currentGeneration = nextGeneration;
                nextGeneration = generationSwapPointer;
                variance = this.GetVarianceAndMaybeUpdateBestSolution(currentGeneration);
            }

            Array.Copy(this.BestHarvestByPeriod, 0, this.CurrentHarvestByPeriod, 0, this.BestHarvestByPeriod.Length);
            Array.Copy(this.BestHarvestPeriods, 0, this.CurrentHarvestPeriods, 0, this.BestHarvestPeriods.Length);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
