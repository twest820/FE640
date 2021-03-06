﻿using FE640.Heuristics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FE640.Test
{
    [TestClass]
    [DeploymentItem("FE640_set2_20.xlsx")]
    public class Assignment2
    {
        [TestMethod]
        public void GreatDeluge()
        {
            HarvestUnits units = new HarvestUnits("FE640_set2_20.xlsx");
            units.SetRandomSchedule();
            GreatDeluge deluge = new GreatDeluge(units)
            {
                TargetHarvestPerPeriod = 440000.0
            };
            deluge.Run();

            deluge.RecalculateHarvestVolumes();
            double endObjectiveFunction = deluge.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = deluge.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            Assert.IsTrue(objectiveFunctionRatio > 0.99999);
            Assert.IsTrue(objectiveFunctionRatio < 1.00001);
        }

        [TestMethod]
        public void RecordToRecordTravel()
        {
            HarvestUnits units = new HarvestUnits("FE640_set2_20.xlsx");
            units.SetRandomSchedule();
            RecordToRecordTravel recordTravel = new RecordToRecordTravel(units)
            {
                TargetHarvestPerPeriod = 440000.0
            };
            recordTravel.Run();

            recordTravel.RecalculateHarvestVolumes();
            double endObjectiveFunction = recordTravel.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = recordTravel.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            Assert.IsTrue(objectiveFunctionRatio > 0.99999);
            Assert.IsTrue(objectiveFunctionRatio < 1.00001);
        }

        [TestMethod]
        public void TabuSearch()
        {
            HarvestUnits units = new HarvestUnits("FE640_set2_20.xlsx", 100);
            units.SetRandomSchedule(new List<double>() { 0.30, 0.25, 0.20, 0.15, 0.10 });
            TabuSearch tabu = new TabuSearch(units)
            {
                TargetHarvestPerPeriod = 25000
            };
            tabu.Run();

            tabu.RecalculateHarvestVolumes();
            double endObjectiveFunction = tabu.ObjectiveFunctionByIteration.Last();
            double recalculatedObjectiveFunction = tabu.RecalculateObjectiveFunction();
            double objectiveFunctionRatio = endObjectiveFunction / recalculatedObjectiveFunction;
            Assert.IsTrue(objectiveFunctionRatio > 0.99999);
            Assert.IsTrue(objectiveFunctionRatio < 1.00001);
        }
    }
}
