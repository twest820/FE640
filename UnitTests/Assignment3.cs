using FE640.Heuristics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;

namespace FE640.Test
{
    [TestClass]
    [DeploymentItem("FE640_set3_20.xlsx")]
    public class Assignment3
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void FeasibleMovesOnly()
        {
            HarvestUnits units = new HarvestUnits("FE640_set3_20.xlsx", 100);
            units.SetRectangularAdjacency(10);
            // spot check of a unit on the east edge of the grid
            Assert.IsTrue(units.AdjacencyByUnit[19, 0] == 18);
            Assert.IsTrue(units.AdjacencyByUnit[19, 1] == 9);
            Assert.IsTrue(units.AdjacencyByUnit[19, 2] == 29);
            Assert.IsTrue(units.AdjacencyByUnit[19, 3] == -1);

            // do record to record travel
            RecordToRecordTravel recordTravel = new RecordToRecordTravel(units)
            {
                StopAfter = 10000,
                TargetHarvestPerPeriod = 8700.0,
                TargetHarvestWeights = new double[] { 0.0, 1.34, 1.30, 1.27, 1.23, 1.19, 1.16, 1.13, 1.09, 1.06, 1.03, 1.0 }
            };
            recordTravel.Run();

            // check self reporting from heuristic
            recordTravel.RecalculateHarvestVolumes();
            double endObjectiveFunction = recordTravel.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = recordTravel.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            Assert.IsTrue(objectiveFunctionRatio > 0.99999);
            Assert.IsTrue(objectiveFunctionRatio < 1.00001);

            Assert.IsTrue(recordTravel.MaximumOpeningSize >= units.UnitSize);
            Assert.IsTrue(recordTravel.MaximumOpeningSize <= units.MaximumOpeningSize);

            #if DEBUG
            // additional diagnostic objects (useful when broken in the debugger)
            StringBuilder harvestPeriods = new StringBuilder();
            foreach (int harvestPeriod in recordTravel.BestHarvestPeriods)
            {
                harvestPeriods.AppendLine(harvestPeriod.ToString());
            }

            int testUnitIndex = 28;
            int testHarvestPeriod = 5;
            float testUnitOpening = units.GetOpeningSize(testUnitIndex, testHarvestPeriod);
            #endif

            // apply best schedule found to harvest units and do full check of opening sizes
            // This valdiates the heuristic's many incremental opening size calculations against a computationally expensive
            // but complete calculation of opening sizes across every planning period.
            units.SetBestSchedule(recordTravel);
            OpeningSizes openingSizes = units.GetMaximumOpeningSizesByPeriod();
            Assert.IsTrue(openingSizes.MaximumOpeningSizeByPeriod[0] == 0.0F);
            for (int planningPeriod = 1; planningPeriod < openingSizes.MaximumOpeningSizeByPeriod.Length; ++planningPeriod)
            {
                Assert.IsTrue(openingSizes.MaximumOpeningSizeByPeriod[planningPeriod] >= units.UnitSize);
                Assert.IsTrue(openingSizes.MaximumOpeningSizeByPeriod[planningPeriod] <= units.MaximumOpeningSize);
            }
        }

        [TestMethod]
        public void WithInfeasibleOpeningSizes()
        {
            HarvestUnits units = new HarvestUnits("FE640_set3_20.xlsx", 100);
            units.SetRectangularAdjacency(10);

            // do record to record travel
            RecordToRecordTravel recordTravel = new RecordToRecordTravel(units)
            {
                Deviation = 500.0 * 1000.0,
                InfeasibilityPenalty = 250.0 * 1000.0,
                MaximumInfeasibleUnits = 1,
                StopAfter = 100 * 1000,
                TargetHarvestPerPeriod = 8700.0,
                TargetHarvestWeights = new double[] { 0.0, 1.34, 1.30, 1.27, 1.23, 1.19, 1.16, 1.13, 1.09, 1.06, 1.03, 1.0 }
            };
            recordTravel.Run();

            // check self reporting from heuristic
            recordTravel.RecalculateHarvestVolumes();
            double endObjectiveFunction = recordTravel.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = recordTravel.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            this.TestContext.WriteLine("objective: best {0:0}, end {1:0}, end ratio {2:0.00000}", recordTravel.BestObjectiveFunction, recordTravel.ObjectiveFunctionByIteration.Last(), objectiveFunctionRatio);

            // apply best schedule found to harvest units check opening sizes
            units.SetBestSchedule(recordTravel);
            OpeningSizes openingSizes = units.GetMaximumOpeningSizesByPeriod();
            float maximumOpeningSize = openingSizes.MaximumOpeningSizeByPeriod.Max();
            this.TestContext.WriteLine("max opening: best {0:0}ac", maximumOpeningSize);
        }
    }
}
