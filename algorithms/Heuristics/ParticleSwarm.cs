using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640.Heuristics
{
    public class ParticleSwarm : Heuristic
    {
        public double CognitiveConstant { get; set; }
        public double Inertia { get; set; }
        public int Particles { get; set; }
        public double SocialConstant { get; set; }
        public int TimeSteps { get; set; }

        public ParticleSwarm(HarvestUnits units)
            : base(units)
        {
            // defaults from Poli R, Kennedy J, Blackwell T. 2007. Particle swarm optimization: An overview. Swarm Intelligence 1:33-57.
            this.CognitiveConstant = 1.49618;
            this.Inertia = 0.7298;
            this.Particles = 30;
            this.SocialConstant = 1.49618;
            this.TimeSteps = 300;

            this.ObjectiveFunctionByIteration = new List<double>(this.TimeSteps);
        }

        public override TimeSpan Run()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // initialize population of particles and objective functions
            int periods = this.Units.HarvestPeriods + 1;
            double[] bestObjectiveFunctionByParticle = new double[this.Particles];
            double[] bestPosition = new double[this.Units.Count];
            double[][] bestPositionByParticle = new double[this.Particles][];
            double[][] harvestVolumesByParticle = new double[this.Particles][];
            double[][] positionByParticle = new double[this.Particles][];
            int[][] scheduleByParticle = new int[this.Particles][];
            double[,] velocityByParticle = new double[this.Particles, this.Units.Count];

            int bestParticleIndex = -1;
            double initialHarvestPeriodScalingFactor = ((double)this.CurrentHarvestByPeriod.Length - 1.5 - Constant.RoundToZeroTolerance) / (double)byte.MaxValue;
            double velocityScalingFactor = 1.0 / (double)byte.MaxValue;
            this.BestObjectiveFunction = double.MaxValue;
            for (int particleIndex = 0; particleIndex < this.Particles; ++particleIndex)
            {
                double[] bestPositionOfParticle = new double[this.Units.Count];
                bestPositionByParticle[particleIndex] = bestPositionOfParticle;
                int[] schedule = new int[this.Units.Count];
                scheduleByParticle[particleIndex] = schedule;
                double[] harvestVolumes = new double[periods];
                harvestVolumesByParticle[particleIndex] = harvestVolumes;
                double[] position = new double[this.Units.Count];
                positionByParticle[particleIndex] = position;

                // give particle random schedule and velocity
                for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
                {
                    position[unitIndex] = initialHarvestPeriodScalingFactor * this.GetPseudorandomByteAsDouble() + 1.0F;
                    velocityByParticle[particleIndex, unitIndex] = velocityScalingFactor * this.GetPseudorandomByteAsDouble();

                    bestPositionOfParticle[unitIndex] = position[unitIndex];
                    schedule[unitIndex] = (int)Math.Round(position[unitIndex]);
                }

                // initialize particle's objective function
                this.GetHarvestVolumes(schedule, harvestVolumes);
                double particleObjectiveFunction = this.GetObjectiveFunction(harvestVolumes);

                bestObjectiveFunctionByParticle[particleIndex] = particleObjectiveFunction;
                if (particleObjectiveFunction < this.BestObjectiveFunction)
                {
                    this.BestObjectiveFunction = particleObjectiveFunction;
                    bestParticleIndex = particleIndex;
                }
            }

            double[] harvestVolumesFromUnitSchedule = new double[periods];
            this.GetHarvestVolumes(this.Units.HarvestSchedule, harvestVolumesFromUnitSchedule);
            double objectiveFunctionFromUnitSchedule = this.GetObjectiveFunction(harvestVolumesFromUnitSchedule);
            if (objectiveFunctionFromUnitSchedule < this.BestObjectiveFunction)
            {
                this.BestObjectiveFunction = objectiveFunctionFromUnitSchedule;
                bestParticleIndex = 0;
                int[] bestSchedule = scheduleByParticle[bestParticleIndex];
                Array.Copy(this.Units.HarvestSchedule, 0, bestSchedule, 0, this.Units.Count);
                Array.Copy(harvestVolumesFromUnitSchedule, 0, harvestVolumesByParticle[bestParticleIndex], 0, periods);

                double[] bestPositionForParticle = positionByParticle[bestParticleIndex];
                for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
                {
                    bestPositionForParticle[unitIndex] = (double)bestSchedule[unitIndex];
                }
            }

            Array.Copy(harvestVolumesByParticle[bestParticleIndex], 0, this.BestHarvestByPeriod, 0, periods);
            Array.Copy(positionByParticle[bestParticleIndex], 0, bestPosition, 0, this.Units.Count);
            Array.Copy(scheduleByParticle[bestParticleIndex], 0, this.BestHarvestPeriods, 0, this.Units.Count);
            this.ObjectiveFunctionByIteration.Add(this.BestObjectiveFunction);

            //' adjust cut periods for each particle (solution)
            //w = 0.75    ' inertia weight      see discussion in references for parameter values of inertia and
            //c1 = 1.5    ' global weight       and accelerators in section 2.4 of Reference 03_PSO_overview
            //c2 = 1.5     ' local weight
            double cognitiveScalingFactor = this.CognitiveConstant / (double)byte.MaxValue;
            double maxPosition = (double)this.Units.HarvestPeriods + 0.5 - Constant.RoundToZeroTolerance;
            double socialScalingFactor = this.SocialConstant / (double)byte.MaxValue;
            for (int timeStep = 0; timeStep < this.TimeSteps; ++timeStep)
            {
                for (int particleIndex = 0; particleIndex < this.Particles; ++particleIndex)
                {
                    double[] bestPositionOfParticle = bestPositionByParticle[particleIndex];
                    double[] position = positionByParticle[particleIndex];
                    int[] schedule = scheduleByParticle[particleIndex];
                    for (int unitIndex = 0; unitIndex < this.Units.Count; ++unitIndex)
                    {
                        double cognition = cognitiveScalingFactor * this.GetPseudorandomByteAsDouble();
                        double cognitiveDistance = bestPositionOfParticle[unitIndex] - position[unitIndex];
                        double social = socialScalingFactor * this.GetPseudorandomByteAsDouble();
                        double socialDistance = bestPosition[unitIndex] - position[unitIndex];
                        double velocity = velocityByParticle[particleIndex, unitIndex];
                        velocityByParticle[particleIndex, unitIndex] = this.Inertia * velocity + cognition * cognitiveDistance + social * socialDistance;
                        position[unitIndex] += velocityByParticle[particleIndex, unitIndex];
                        if (position[unitIndex] < 0.5)
                        {
                            velocityByParticle[particleIndex, unitIndex] = -velocityByParticle[particleIndex, unitIndex];
                            position[unitIndex] = 0.5;
                        }
                        else if (position[unitIndex] > maxPosition)
                        {
                            velocityByParticle[particleIndex, unitIndex] = -velocityByParticle[particleIndex, unitIndex];
                            position[unitIndex] = maxPosition;
                        }
                        schedule[unitIndex] = (int)Math.Round(position[unitIndex]);

                        //    ' for example to make integer using sigmoid function to map velocity into 0-1 range
                        //   ' x(i, j) = Int(x(i, j))                      ' round down
                        //   ' Prob = 1 / (1 + Exp(-v(i, j)))              ' map the velocity into 0 - 1 range
                        //   ' If Rnd < Prob Then x(i, j) = x(i, j) + 1    ' see if we can slip a random number under it

                        //    If x(i, j) > 3 Then x(i, j) = 3   ' clamp down on range
                        //    If x(i, j) < 1 Then x(i, j) = 1   '   "         "
                    }

                    double[] harvestVolumes = harvestVolumesByParticle[particleIndex];
                    Array.Clear(harvestVolumes, 0, harvestVolumes.Length);
                    this.GetHarvestVolumes(schedule, harvestVolumes);
                    double particleObjectiveFunction = this.GetObjectiveFunction(harvestVolumes);
                    if (particleObjectiveFunction < bestObjectiveFunctionByParticle[particleIndex])
                    {
                        bestObjectiveFunctionByParticle[particleIndex] = particleObjectiveFunction;
                        Array.Copy(position, 0, bestPositionByParticle[particleIndex], 0, bestPositionByParticle[particleIndex].Length);
                    }
                    if (particleObjectiveFunction < this.BestObjectiveFunction)
                    {
                        this.BestObjectiveFunction = particleObjectiveFunction;
                        Array.Copy(position, 0, bestPosition, 0, position.Length);
                        Array.Copy(harvestVolumes, 0, this.BestHarvestByPeriod, 0, harvestVolumes.Length);
                        Array.Copy(schedule, 0, this.BestHarvestPeriods, 0, schedule.Length);
                    }
                }

                this.ObjectiveFunctionByIteration.Add(this.BestObjectiveFunction);
            }

            Array.Copy(this.BestHarvestByPeriod, 0, this.CurrentHarvestByPeriod, 0, this.BestHarvestByPeriod.Length);
            Array.Copy(this.BestHarvestPeriods, 0, this.CurrentHarvestPeriods, 0, this.BestHarvestPeriods.Length);

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
