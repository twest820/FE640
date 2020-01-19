using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FE640.Test
{
    [TestClass]
    [DeploymentItem("FE640_set1_20.xlsx")]
    public class Assignment1
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SimulatedAnnealing()
        {
            HarvestUnits units = new HarvestUnits("FE640_set1_20.xlsx");
            SimulatedAnnealing annealer = new SimulatedAnnealing(units);
            annealer.Anneal();

            annealer.RecalculateHarvestVolumes();
            float endObjectiveFunction = annealer.ObjectiveFunctionByIteration.Last();
            float recalculatedObjectiveFunction = annealer.RecalculateObjectiveFunction();
            float objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            Assert.IsTrue(objectiveFunctionRatio > 0.999F);
            Assert.IsTrue(objectiveFunctionRatio < 1.001F);
        }
    }
}
