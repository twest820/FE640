using FE640.Heuristics;
using System;
using System.Management.Automation;

namespace FE640.Cmdlets
{
    [Cmdlet(VerbsCommon.Optimize, "Tabu")]
    public class OptimizeTabu : OptimizeCmdlet
    {
        [Parameter]
        public Nullable<int> Iterations { get; set; }
        [Parameter]
        public int MaximumUnitIndex { get; set; }
        [Parameter]
        public Nullable<int> Tenure { get; set; }

        public OptimizeTabu()
        {
            this.MaximumUnitIndex = 100;
        }

        protected override Heuristic CreateHeuristic()
        {
            TabuSearch tabu = new TabuSearch(this.Units, this.MaximumUnitIndex);
            if (this.Iterations.HasValue)
            {
                tabu.Iterations = this.Iterations.Value;
            }
            if (this.Tenure.HasValue)
            {
                tabu.Tenure = this.Tenure.Value;
            }
            return tabu;
        }
    }
}
