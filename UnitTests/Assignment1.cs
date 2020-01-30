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
            units.SetRandomSchedule();
            SimulatedAnnealing annealer = new SimulatedAnnealing(units)
            {
                TargetHarvestPerPeriod = 440000.0,
                TargetHarvestWeights = new double[] { 0.0, 1.4, 1.3, 1.2, 1.1, 1.0 }
            };
            annealer.Anneal();

            annealer.RecalculateHarvestVolumes();
            double endObjectiveFunction = annealer.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = annealer.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            Assert.IsTrue(objectiveFunctionRatio > 0.99999);
            Assert.IsTrue(objectiveFunctionRatio < 1.00001);
        }

        [TestMethod]
        public void ThresholdAccepting()
        {
            HarvestUnits units = new HarvestUnits("FE640_set1_20.xlsx");
            units.SetRandomSchedule();
            ThresholdAccepting acceptor = new ThresholdAccepting(units)
            {
                TargetHarvestPerPeriod = 440000.0,
                TargetHarvestWeights = new double[] { 0.0, 1.4, 1.3, 1.2, 1.1, 1.0 }
            };
            acceptor.Accept();

            acceptor.RecalculateHarvestVolumes();
            double endObjectiveFunction = acceptor.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = acceptor.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            Assert.IsTrue(objectiveFunctionRatio > 0.99999);
            Assert.IsTrue(objectiveFunctionRatio < 1.00001);
        }
    }
}
