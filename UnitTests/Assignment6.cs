using FE640.Heuristics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FE640.Test
{
    [TestClass]
    [DeploymentItem("FE640_set6_20.xlsx")]
    public class Assignment6
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void AntColony()
        {
            HarvestUnits units = new HarvestUnits("FE640_set6_20.xlsx", 100);

            // do evolution
            AntColony colony = new AntColony(units)
            {
                TargetHarvestPerPeriod = 25.0 * 1000.0,
                TargetHarvestWeights = new double[] { 0.0, 2.86, 2.20, 1.69, 1.30, 1.0 },
            };
            colony.Run();

            // check self reporting from heuristic
            colony.RecalculateHarvestVolumes();
            double endObjectiveFunction = colony.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = colony.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            Assert.IsTrue(objectiveFunctionRatio > 0.99999);
            Assert.IsTrue(objectiveFunctionRatio < 1.00001);

            this.TestContext.WriteLine("objective: best {0:0}, end {1:0}, end ratio {2:0.00000}", colony.BestObjectiveFunction, colony.ObjectiveFunctionByIteration.Last(), objectiveFunctionRatio);
        }
    }
}
