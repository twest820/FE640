$workingDirectory = Get-Location
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Debug")
#$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Release")

Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "FE640.dll"));

$units = Get-Units -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20.xlsx"));
$annealer = Optimize-SimulatedAnnealing -Units $units -TargetHarvestPerPeriod 445000 -Verbose;
$annealer.BestHarvestByPeriod
$annealer.CurrentHarvestByPeriod
Write-Harvest -Heuristics $annealer -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_harvest.csv"));
Write-HarvestSchedule -Heuristics $annealer -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_schedule.csv"));
Write-Objective -Heuristics $annealer -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_sa_objective.csv"));

$acceptor = Optimize-ThresholdAccepting -Units $units -TargetHarvestPerPeriod 445000 -Verbose;
$acceptor = Optimize-Harvest -Units $units -TargetHarvestPerPeriod 445000 -Verbose;  
#$acceptor = Optimize-Harvest -Units $units -TargetHarvestPerPeriod 445000 -Thresholds (1.05, 1.05, 1.05, 1.04, 1.03, 1.00) -Verbose;
$acceptor.BestHarvestByPeriod
$acceptor.CurrentHarvestByPeriod
Write-Harvest -Heuristics $acceptor -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_ta_harvest.csv"));
Write-HarvestSchedule -Heuristics $acceptor -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_ta_schedule.csv"));
Write-Objective -Heuristics $acceptor -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set1_20_ta_objective.csv"));
