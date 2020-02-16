using FE640.Heuristics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FE640.Test
{
    [TestClass]
    [DeploymentItem("FE640_set4_20.xlsx")]
    public class Assignment4
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GeneticAlgorithm()
        {
            HarvestUnits units = new HarvestUnits("FE640_set4_20.xlsx", 100);

            // do evolution
            GeneticAlgorithm genetic = new GeneticAlgorithm(units)
            {
                TargetHarvestPerPeriod = 0.6 * 211500.0 / 5.0,
                TargetHarvestWeights = new double[] { 0.0, 2.86, 2.20, 1.69, 1.30, 1.0 }
            };
            genetic.Run();

            // check self reporting from heuristic
            genetic.RecalculateHarvestVolumes();
            double endObjectiveFunction = genetic.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = genetic.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            this.TestContext.WriteLine("objective: best {0:0}, end {1:0}, end ratio {2:0.00000}", genetic.BestObjectiveFunction, genetic.ObjectiveFunctionByIteration.Last(), objectiveFunctionRatio);
        }
    }
}
