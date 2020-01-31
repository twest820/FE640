$workingDirectory = Get-Location
#$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Debug")
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Release")

Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "FE640.dll"));
$units = Get-Units -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set2_20.xlsx"));
$units.SetRandomSchedule()
$harvestWeights = (0.0, 2.2, 1.9, 1.6, 1.3, 1.0) 
$targetHarvestPerPeriod = 439800

#linear rain
#$deluge = Optimize-GreatDeluge -Units $units -RainRate 40E3 -StopAfter 1E7 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
#geometric rain
$deluge = Optimize-GreatDeluge -Units $units -RainRate 0.99995 -StopAfter 1E5 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$deluge.BestHarvestByPeriod

$delugeAndObjectives = Optimize-GreatDeluge -BestOf 100 -RainRate 0.99995 -StopAfter 1E5 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$delugeAndObjectives[0].BestHarvestByPeriod
Write-Harvest -Heuristics $delugeAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set2_20_gd_harvest.csv"));
Write-HarvestSchedule -Heuristics $delugeAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set2_20_gd_schedule.csv"));
Write-Objective -Heuristics $delugeAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set2_20_gd_objective.csv"));
$delugeAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set2_20_gd_objectiveDistribution.csv")) -Encoding utf8;

$recordTravel = Optimize-RecordTravel -Units $units -Deviation 1E5 -StopAfter 1E5 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$recordTravel.BestHarvestByPeriod

$recordTravelAndObjectives = Optimize-RecordTravel -BestOf 100 -Deviation 1E6 -StopAfter 1E5 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$recordTravelAndObjectives[0].BestHarvestByPeriod
Write-Harvest -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set2_20_rt_harvest.csv"));
Write-HarvestSchedule -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set2_20_rt_schedule.csv"));
Write-Objective -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set2_20_rt_objective.csv"));
$recordTravelAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set2_20_rt_objectiveDistribution.csv")) -Encoding utf8;