$workingDirectory = Get-Location
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Debug")
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Release")
Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "FE640.dll"));

# 100 unit problem
# record to record travel control: 23,770 units/period, 8.6 σ
$units = Get-Units -Units 100 -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set6_20.xlsx"));
$targetHarvestPerPeriod = 23900

$antColony = Optimize-AntColony -Ants 10 -Iterations 200 -PheremoneEvaporationRate 0.3 -PheremoneProportion 0.9 -ReservedPopulationProportion 0.3 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$antColony.BestHarvestByPeriod

#$harvestWeights = (0.0, 2.86, 2.20, 1.69, 1.30, 1.00)
#$harvestWeights = (0.0, 2.856, 2.197, 1.690, 1.300, 1.000)

#harvest weight visibility + proportional pheromones
#$harvestWeights = (0.0, 1.0, 1.0, 1.0, 1.0, 1.0)
#$antColonyAndObjectives = Optimize-AntColony -Ants 50 -Iterations 500 -PheremoneEvaporationRate 0.9 -PheremoneProportion 0.999 -ReservedPopulationProportion 0.1 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;

#harvest weight visibility + max-min ant system
#$antColonyAndObjectives = Optimize-AntColony -Ants 50 -Iterations 250 -PheremoneEvaporationRate 0.5 -PheremoneProportion 0.995 -TrailTranspositionProbability 0.4 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$harvestWeights = (0.0, 2.86, 2.20, 1.69, 1.30, 1.00)
$antColonyAndObjectives = Optimize-AntColony -Ants 50 -Iterations 250 -PheremoneEvaporationRate 0.45 -PheremoneProportion 0.995 -TrailTranspositionProbability 0.4 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;

$antColonyAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $antColonyAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set6_20_ac100tmm_schedule.csv"));
Write-Objective -Heuristics $antColonyAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set6_20_ac100tmm_objective.csv"));
$antColonyAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set6_20_ac100tmm_objectiveDistribution.csv")) -Encoding utf8;


# 2000 unit problem
$units = Get-Units -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set6_20.xlsx"));
$targetHarvestPerPeriod = 439800

$antColonyAndObjectives = Optimize-AntColony -Ants 50 -Iterations 250 -PheremoneEvaporationRate 0.5 -PheremoneProportion 0.998 -TrailTranspositionProbability 0.5 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$antColonyAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $antColonyAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set6_20_ac2ktmm_schedule.csv"));
Write-Objective -Heuristics $antColonyAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set6_20_ac2ktmm_objective.csv"));
$antColonyAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set6_20_ac2ktmm_objectiveDistribution.csv")) -Encoding utf8;
