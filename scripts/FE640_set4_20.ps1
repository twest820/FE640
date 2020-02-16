$workingDirectory = Get-Location
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Debug")
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Release")
Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "FE640.dll"));
$harvestWeights = (0.0, 2.86, 2.20, 1.69, 1.30, 1.00)
#$harvestWeights = (0.0, 2.856, 2.197, 1.690, 1.300, 1.000)

# 100 unit problem
# record to record travel control: 23,770 units/period, 8.6 σ
$units = Get-Units -Units 100 -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20.xlsx"));
$targetHarvestPerPeriod = 23900

$deluge = Optimize-GreatDeluge -Units $units -InitialWaterLevelMultiplier 1.2 -RainRate 0.99995 -StopAfter 5E5 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$deluge.BestHarvestByPeriod

$delugeAndObjectives = Optimize-GreatDeluge -BestOf 100 -RainRate 0.99995 -StopAfter 5E5 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$delugeAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $delugeAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_gd100_schedule.csv"));
Write-Objective -Heuristics $delugeAndObjectives[0] -Step 25 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_gd100_objective.csv"));
$delugeAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_gd100_objectiveDistribution.csv")) -Encoding utf8;

$recordTravelAndObjectives = Optimize-RecordTravel -BestOf 100 -Deviation 5E5 -StopAfter 5E4 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
$recordTravelAndObjectives[0].BestHarvestByPeriod
Write-Harvest -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_rt100_harvest.csv"));
Write-HarvestSchedule -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_rt100_schedule.csv"));
Write-Objective -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_rt100_objective.csv"));
$recordTravelAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_rt100_objectiveDistribution.csv")) -Encoding utf8;

$annealer = Optimize-SimulatedAnnealing -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$annealer.BestHarvestByPeriod
$annealer.CurrentHarvestByPeriod
$annealerAndObjectives = Optimize-SimulatedAnnealing -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$annealerAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $annealerAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_sa100_schedule.csv"));
Write-Objective -Heuristics $annealerAndObjectives[0] -Step 50 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_sa100_objective.csv"));
$annealerAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_sa100_objectiveDistribution.csv")) -Encoding utf8;


$tabu = Optimize-Tabu -Units $units -Iterations 100 -Tenure 4 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose

$tabuAndObjectives = Optimize-Tabu -BestOf 100 -UniformHarvestProbability -Units $units -Iterations 250 -Tenure 5 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
$tabuAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $tabuAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_tu100_schedule.csv"));
Write-Objective -Heuristics $tabuAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_tu100_objective.csv"));
$tabuAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_tu100_objectiveDistribution.csv")) -Encoding utf8;

$genetic = Optimize-Genetic -Units $units -MaximumGenerations 500 -ReservedPopulationProportion 0.1 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
$geneticAndObjectives = Optimize-Genetic -Units $units -BestOf 100 -MaximumGenerations 250 -MutationProbability 0.1 -ReservedPopulationProportion 0.1 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
$geneticAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $geneticAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga100_schedule.csv"));
Write-Objective -Heuristics $geneticAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga100_objective.csv"));
$geneticAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga100_objectiveDistribution.csv")) -Encoding utf8;


# 2000 unit problems
# reuse of problem set 2 deluge, record travel, and tabu results
$units = Get-Units -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20.xlsx"));
$targetHarvestPerPeriod = 439800

$genetic = Optimize-Genetic -Units $units -MaximumGenerations 750 -MutationProbability 0.3 -ReservedPopulationProportion 0.3 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
#$genetic = Optimize-Genetic -Units $units -MaximumGenerations 5000 -MutationProbability 1 -PopulationSize 2 -ReservedPopulationProportion 0.3 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
$genetic.BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $genetic -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga2k_schedule0.754.csv"));
Write-Objective -Heuristics $genetic -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga2k_objective0.754.csv"));
#$genetic.ObjectiveFunctionByIteration | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga2k_objectiveDistribution.csv")) -Encoding utf8;
#$genetic.BestHarvestPeriods | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga2k_scheduleOutFile.csv")) -Encoding utf8;

#$geneticAndObjectives = Optimize-Genetic -Units $units -BestOf 100 -MaximumGenerations 200 -MutationProbability 0.3 -ReservedPopulationProportion 0.3 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
$geneticAndObjectives = Optimize-Genetic -Units $units -BestOf 10 -MaximumGenerations 200 -MutationProbability 0.3 -ReservedPopulationProportion 0.2 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
#$geneticAndObjectives = Optimize-Genetic -Units $units -BestOf 100 -MaximumGenerations 200 -MutationProbability 0.5 -PopulationSize 30 -ReservedPopulationProportion 0.3 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
$geneticAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $geneticAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga2k_schedule.csv"));
Write-Objective -Heuristics $geneticAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga2k_objective.csv"));
$geneticAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga2k_objectiveDistribution750.csv")) -Encoding utf8;
#$geneticAndObjectives[0].BestHarvestPeriods | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set4_20_ga2k_scheduleOutFile.csv")) -Encoding utf8;
