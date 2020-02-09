$workingDirectory = Get-Location
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Debug")
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Release")
$harvestWeights = (0.0, 1.343916, 1.304773, 1.266770, 1.229874, 1.194052, 1.159274, 1.125509, 1.092727, 1.0609, 1.03, 1.0) 

Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "FE640.dll"));
$units = Get-Units -Units 100 -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20.xlsx"));
$units.SetRectangularAdjacency(10);
#Write-Units -Units $units -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_units.csv"));

# non-spatial solutions @ 100 units: 8400-8600 units/year, 1.0-1.2% even flow, range 150-200ish
#   4.4 Mips debug, 4.3 Mips release
#$targetHarvestPerPeriod = 8700
#$recordTravel = Optimize-RecordTravel -Units $units -Deviation 2.5E5 -StopAfter 5E5 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;

# spatial solutions @ 100 units: best deviation deviation 2.5E5, stop after 5E5
#   full recursion: 10 runs, 8600 units/year (8700 target), 1.2-1.5% even flow, range 150-200, 125 kips debug, 550 kips release
#   ref recursion: 100 runs, 8600 units/year (8700 target), 0.5% even flow, range 80, 8.98 Mips release
#   loop unrolled: 100 runs, 8600 units/year (8700 target), 1.0% even flow, range 128, 9.17 Mips release
$targetHarvestPerPeriod = 8700
$recordTravel = Optimize-RecordTravel -Units $units -Deviation 2.5E5 -StopAfter 5E5 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$recordTravel.BestHarvestByPeriod
$recordTravel.BestHarvestPeriods

$units.SetBestSchedule($recordTravel)
$openings = $units.GetMaximumOpeningSizesByPeriod()
$openings.MaximumOpeningSizeByPeriod


$targetHarvestPerPeriod = 8700
$recordTravelAndObjectives = Optimize-RecordTravel -BestOf 100 -Deviation 2.5E5 -StopAfter 7.5E5 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$recordTravelAndObjectives[0].BestHarvestByPeriod

$units.SetBestSchedule($recordTravelAndObjectives[0])
$openings = $units.GetMaximumOpeningSizesByPeriod()
$openings.MaximumOpeningSizeByPeriod

$recordTravelAndObjectives = Optimize-RecordTravel -BestOf 1000 -Deviation 0.9E6 -StopAfter 7.5E5 -UniformHarvestProbability -Units $units -TargetHarvestPerPeriod 439620 -TargetHarvestWeights $harvestWeights -Verbose;

Write-Harvest -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_harvest100.csv"));
Write-HarvestSchedule -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_schedule100.csv"));
Write-Objective -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_objective100.csv"));
$recordTravelAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_objectiveDistribution100.csv")) -Encoding utf8;


# 2000 units
$units = Get-Units -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20.xlsx"));
$units.SetRectangularAdjacency(50);
$harvestWeights = (0.0, 1.343916, 1.304773, 1.266770, 1.229874, 1.194052, 1.159274, 1.125509, 1.092727, 1.0609, 1.03, 1.0) 
$targetHarvestPerPeriod = 159000
$recordTravel = Optimize-RecordTravel -Units $units -Deviation 5E5 -StopAfter 2.5E6 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;

$recordTravel.BestHarvestByPeriod
$units.SetBestSchedule($recordTravel)
$openings = $units.GetMaximumOpeningSizesByPeriod()
$openings.MaximumOpeningSizeByPeriod

Write-Harvest -Heuristics $recordTravel -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_2kHarvest.csv"));
Write-HarvestSchedule -Heuristics $recordTravel -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_2kSchedule.csv"));
Write-Objective -Heuristics $recordTravel -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_2k10Objective.csv"));
$recordTravel.ObjectiveFunctionByIteration | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_2kObjectiveDistribution.csv")) -Encoding utf8;


$recordTravelAndObjectives = Optimize-RecordTravel -BestOf 100 -Deviation 5E5 -StopAfter 2.5E6 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$recordTravelAndObjectives[0].BestHarvestByPeriod

$units.SetBestSchedule($recordTravelAndObjectives[0])
$openings = $units.GetMaximumOpeningSizesByPeriod()
$openings.MaximumOpeningSizeByPeriod

Write-Harvest -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_2kHarvest.csv"));
Write-HarvestSchedule -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_2kSchedule.csv"));
Write-Objective -Heuristics $recordTravelAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_2kObjective.csv"));
$recordTravelAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set3_20_rt_2kObjectiveDistribution.csv")) -Encoding utf8;
