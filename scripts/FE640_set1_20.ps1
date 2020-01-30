﻿$workingDirectory = Get-Location
#$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Debug")
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Release")

Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "FE640.dll"));
$units = Get-Units -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20.xlsx"));
#$units.SetRandomSchedule()
$harvestWeights = (0.0, 2.2, 1.9, 1.6, 1.3, 1.0) 

$annealer = Optimize-SimulatedAnnealing -Units $units -TargetHarvestPerPeriod 439800 -TargetHarvestWeights $harvestWeights -Verbose;
$annealer.BestHarvestByPeriod
$annealer.CurrentHarvestByPeriod
$annealerAndObjectives = Optimize-SimulatedAnnealing -BestOf 100 -Units $units -TargetHarvestPerPeriod 439800 -TargetHarvestWeights $harvestWeights -Verbose;
$annealerAndObjectives[0].BestObjectiveFunction / 1E6
$annealerAndObjectives[0].BestHarvestByPeriod
Write-Harvest -Heuristics $annealerAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_harvest.csv"));
Write-HarvestSchedule -Heuristics $annealerAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_schedule.csv"));
Write-Objective -Heuristics $annealerAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_objective.csv"));
$annealerAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_objectiveDistribution.csv"));

$acceptor = Optimize-ThresholdAccepting -Units $units -TargetHarvestPerPeriod 439800 -TargetHarvestWeights $harvestWeights -Thresholds (1.05, 1.03, 1.00) -Verbose;
$acceptor.BestHarvestByPeriod
#$acceptor.CurrentHarvestByPeriod
$acceptorAndObjectives = Optimize-ThresholdAccepting -BestOf 100 -Units $units -TargetHarvestPerPeriod 439800 -TargetHarvestWeights $harvestWeights -Thresholds (1.05, 1.03, 1.00) -Verbose;
$acceptorAndObjectives[0].BestObjectiveFunction / 1E6
$acceptorAndObjectives[0].BestHarvestByPeriod
Write-Harvest -Heuristics $acceptorAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_ta_harvest.csv"));
Write-HarvestSchedule -Heuristics $acceptorAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_ta_schedule.csv"));
Write-Objective -Heuristics $acceptorAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_ta_objective.csv"));
$acceptorAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_ta_objectiveDistribution.csv"));
