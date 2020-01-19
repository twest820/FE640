$workingDirectory = Get-Location
#$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Debug")
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Release")

Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "FE640.dll"));

$units = Get-Units -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20.xlsx"));
$annealer = Optimize-SimulatedAnnealing -Units $units -TargetHarvestPerPeriod 445000 -Verbose;
$annealer.BestHarvestByPeriod
$annealer.CurrentHarvestByPeriod
Write-Harvest -Heuristics $annealer -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_harvest.csv"));
Write-HarvestSchedule -Heuristics $annealer -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_schedule.csv"));
Write-Objective -Heuristics $annealer -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_objective.csv"));
