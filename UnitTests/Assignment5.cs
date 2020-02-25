using FE640.Heuristics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FE640.Test
{
    [TestClass]
    [DeploymentItem("FE640_set5_20.xlsx")]
    public class Assignment5
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ParticleSwarm()
        {
            HarvestUnits units = new HarvestUnits("FE640_set5_20.xlsx", 100);

            // do evolution
            ParticleSwarm swarm = new ParticleSwarm(units)
            {
                TargetHarvestPerPeriod = 0.6 * 211500.0 / 5.0,
                TargetHarvestWeights = new double[] { 0.0, 2.86, 2.20, 1.69, 1.30, 1.0 },
                TimeSteps = 100
            };
            swarm.Run();

            // check self reporting from heuristic
            swarm.RecalculateHarvestVolumes();
            double endObjectiveFunction = swarm.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = swarm.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            Assert.IsTrue(objectiveFunctionRatio > 0.99999);
            Assert.IsTrue(objectiveFunctionRatio < 1.00001);

            this.TestContext.WriteLine("objective: best {0:0}, end {1:0}, end ratio {2:0.00000}", swarm.BestObjectiveFunction, swarm.ObjectiveFunctionByIteration.Last(), objectiveFunctionRatio);
        }
    }
}
