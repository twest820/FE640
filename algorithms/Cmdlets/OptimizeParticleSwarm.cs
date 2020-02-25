using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "ParticleSwarm")]
    public class OptimizeParticleSwarm : OptimizeCmdlet
    {
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> Inertia { get; set; }
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> CognitiveConstant { get; set; }
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> Particles { get; set; }
        [Parameter]
        [ValidateRange(0.0, double.MaxValue)]
        public Nullable<double> SocialConstant { get; set; }
        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public Nullable<int> TimeSteps { get; set; }

        public OptimizeParticleSwarm()
        {
            this.Inertia = null;
            this.CognitiveConstant = null;
            this.Particles = null;
            this.SocialConstant = null;
            this.TimeSteps = null;
        }

        protected override Heuristic CreateHeuristic()
        {
            ParticleSwarm swarm = new ParticleSwarm(this.Units);
            if (this.Inertia.HasValue)
            {
                swarm.Inertia = this.Inertia.Value;
            }
            if (this.Particles.HasValue)
            {
                swarm.Particles = this.Particles.Value;
            }
            if (this.TimeSteps.HasValue)
            {
                swarm.TimeSteps = this.TimeSteps.Value;
            }
            return swarm;
        }
    }
}
