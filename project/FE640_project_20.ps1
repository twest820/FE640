Set-Location -Path ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\FE 640\\project"))
$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Organon\\UnitTests\\bin\\x64\\Debug\\netcoreapp3.0"))
$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Organon\\UnitTests\\bin\\x64\\Release\\netcoreapp3.0"))
Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "Organon.dll"));
$stand = Get-StandFromNelderPlot -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Nelder20.xlsx"));

# net present value: unrestricted entry
# TODO: revise deluge water + rain, record travel deviation, simulated annealing temperatures
$runs = 100
for ($harvestPeriods = 5; $harvestPeriods -lt 9; ++$harvestPeriods)
{
  $acceptorAndObjectives = Optimize-ThresholdAccepting -IterationsPerThreshold 100 -Thresholds (0.90, 0.95, 1.0) -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  $geneticAndObjectives = Optimize-Genetic -MaximumGenerations 100 -PopulationSize 20 -MutationProbability 0.7 -ReservedPopulationProportion 0.8 -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  $recordTravelAndObjectives = Optimize-RecordTravel -Deviation 0.75 -StopAfter 300 -Stand $stand -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

  $delugeAndObjectives = Optimize-GreatDeluge -FinalWaterLevel 1100 -InitialWaterLevel 100 -RainRate 0.5 -StopAfter 1000 -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  #$tabuAndObjectives = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  $annealerAndObjectives = Optimize-SimulatedAnnealing -Alpha 0.99 -InitialTemperature 100 -FinalTemperature 10 -IterationsPerTemperature 10 -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

  $heuristics = ($annealerAndObjectives[0], $acceptorAndObjectives[0], $geneticAndObjectives[0], $recordTravelAndObjectives[0], $delugeAndObjectives[0])
  $distributions = ($annealerAndObjectives[1], $acceptorAndObjectives[1], $geneticAndObjectives[1], $recordTravelAndObjectives[1], $delugeAndObjectives[1])
  Write-Harvest -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npv$($harvestPeriods)_harvest.csv"));
  Write-HarvestSchedule -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npv$($harvestPeriods)_schedule.csv"));
  Write-Objective -Heuristics $heuristics -Step 1 -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npv$($harvestPeriods)_objective.csv"));
  Write-ObjectiveDistribution -Distribution $distributions -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npv$($harvestPeriods)_objectiveDistribution.csv"));
}

# net present value: single entry
$runs = 100
for ($harvestPeriods = 1; $harvestPeriods -lt 9; ++$harvestPeriods)
{
  $acceptorAndObjectives = Optimize-ThresholdAccepting -IterationsPerThreshold 100 -Thresholds (0.9980, 0.9985, 0.9990, 0.9995, 1.0, 1.0) -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  $geneticAndObjectives = Optimize-Genetic -MaximumGenerations 100 -PopulationSize 20 -MutationProbability 0.7 -ReservedPopulationProportion 0.8 -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  $recordTravelAndObjectives = Optimize-RecordTravel -Deviation 0.0025 -StopAfter 330 -Stand $stand -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

  $delugeAndObjectives = Optimize-GreatDeluge -FinalWaterLevel 20 -InitialWaterLevel 16 -RainRate 0.002 -StopAfter 1000 -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  #$tabuAndObjectives = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  $annealerAndObjectives = Optimize-SimulatedAnnealing -Alpha 0.993 -InitialTemperature 0.001 -FinalTemperature 0.0001 -IterationsPerTemperature 8 -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  #$heroAndObjectives = Optimize-Hero -Iterations 665 -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

  $heuristics = ($annealerAndObjectives[0], $acceptorAndObjectives[0], $geneticAndObjectives[0], $recordTravelAndObjectives[0], $delugeAndObjectives[0])
  $distributions = ($annealerAndObjectives[1], $acceptorAndObjectives[1], $geneticAndObjectives[1], $recordTravelAndObjectives[1], $delugeAndObjectives[1])
  Write-Harvest -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntry$($harvestPeriods)_harvest.csv"));
  Write-HarvestSchedule -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntry$($harvestPeriods)_schedule.csv"));
  Write-Objective -Heuristics $heuristics -Step 1 -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntry$($harvestPeriods)_objective.csv"));
  Write-ObjectiveDistribution -Distribution $distributions -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntry$($harvestPeriods)_objectiveDistribution.csv"));
}

# single entry NPV Monte Carlo probing
$runs = 100
for ($harvestPeriods = 1; $harvestPeriods -lt 9; ++$harvestPeriods)
{
  $acceptorAndObjectives = Optimize-ThresholdAccepting -IterationsPerThreshold 100 -Thresholds (1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.9995, 1.0, 0.9995, 1.0) -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  $annealerAndObjectives = Optimize-SimulatedAnnealing -Alpha 0.993 -InitialTemperature 0.001 -FinalTemperature 0.0001 -IterationsPerTemperature 8 -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
  $recordTravelAndObjectives = Optimize-RecordTravel -Deviation 0.0025 -StopAfter 200 -Stand $stand -BestOf $runs -Cores 4 -HarvestPeriods $harvestPeriods -NetPresentValue -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

  $heuristics = ($annealerAndObjectives[0], $acceptorAndObjectives[0], $recordTravelAndObjectives[0])
  $distributions = ($annealerAndObjectives[1], $acceptorAndObjectives[1], $recordTravelAndObjectives[1])
  Write-Harvest -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvTASART$($harvestPeriods)_harvest.csv"));
  Write-HarvestSchedule -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvTASART$($harvestPeriods)_schedule.csv"));
  Write-Objective -Heuristics $heuristics -Step 1 -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvTASART$($harvestPeriods)_objective.csv"));
  Write-ObjectiveDistribution -Distribution $distributions -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvTASART$($harvestPeriods)_objectiveDistribution.csv"));
}

# TODO: multiple entry with 8 periods
# TODO: multiple entry hero
# TODO: multiple entry tabu re-runs
# TODO: single entry tabu runs
$tabuAndObjectives1 = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods 1 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabuAndObjectives2 = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods 2 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabuAndObjectives3 = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods 3 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabuAndObjectives4 = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods 4 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabuAndObjectives5 = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods 5 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabuAndObjectives6 = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods 6 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabuAndObjectives7 = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods 7 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabuAndObjectives8 = Optimize-Tabu -Iterations 665 -Tenure 50 -BestOf 4 -Cores 4 -HarvestPeriods 8 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heuristics = ($tabuAndObjectives1[0], $tabuAndObjectives2[0], $tabuAndObjectives3[0], $tabuAndObjectives4[0], $tabuAndObjectives5[0], $tabuAndObjectives6[0], $tabuAndObjectives7[0], $tabuAndObjectives8[0])
$distributions = ($tabuAndObjectives1[1], $tabuAndObjectives2[1], $tabuAndObjectives3[1], $tabuAndObjectives4[1], $tabuAndObjectives5[1], $tabuAndObjectives6[1], $tabuAndObjectives7[1], $tabuAndObjectives8[1])
Write-Harvest -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntryTabu_harvest.csv"));
Write-HarvestSchedule -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntryTabu_schedule.csv"));
Write-Objective -Heuristics $heuristics -Step 1 -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntryTabu_objective.csv"));
Write-ObjectiveDistribution -Distribution $distributions -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntryTabu_objectiveDistribution.csv"));

# single entry hero runs
$runs = 100
$heroAndObjectives1 = Optimize-Hero -Iterations 100 -BestOf $runs -Cores 4 -HarvestPeriods 1 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heroAndObjectives2 = Optimize-Hero -Iterations 100 -BestOf $runs -Cores 4 -HarvestPeriods 2 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heroAndObjectives3 = Optimize-Hero -Iterations 100 -BestOf $runs -Cores 4 -HarvestPeriods 3 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heroAndObjectives4 = Optimize-Hero -Iterations 100 -BestOf $runs -Cores 4 -HarvestPeriods 4 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heroAndObjectives5 = Optimize-Hero -Iterations 100 -BestOf $runs -Cores 4 -HarvestPeriods 5 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heroAndObjectives6 = Optimize-Hero -Iterations 100 -BestOf $runs -Cores 4 -HarvestPeriods 6 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heroAndObjectives7 = Optimize-Hero -Iterations 100 -BestOf $runs -Cores 4 -HarvestPeriods 7 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heroAndObjectives8 = Optimize-Hero -Iterations 100 -BestOf $runs -Cores 4 -HarvestPeriods 8 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heuristics = ($heroAndObjectives1[0], $heroAndObjectives2[0], $heroAndObjectives3[0], $heroAndObjectives4[0], $heroAndObjectives5[0], $heroAndObjectives6[0], $heroAndObjectives7[0], $heroAndObjectives8[0])
$distributions = ($heroAndObjectives1[1], $heroAndObjectives2[1], $heroAndObjectives3[1], $heroAndObjectives4[1], $heroAndObjectives5[1], $heroAndObjectives6[1], $heroAndObjectives7[1], $heroAndObjectives8[1])
Write-Harvest -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntryHero_harvest.csv"));
Write-HarvestSchedule -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntryHero_schedule.csv"));
Write-Objective -Heuristics $heuristics -Step 1 -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntryHero_objective.csv"));
Write-ObjectiveDistribution -Distribution $distributions -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665npvSingleEntryHero_objectiveDistribution.csv"));

# Scribner
$runs = 100
$acceptorAndObjectives = Optimize-ThresholdAccepting -IterationsPerThreshold 100 -Thresholds (0.90, 0.95, 1.0) -BestOf $runs -Cores 4 -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$geneticAndObjectives = Optimize-Genetic -MaximumGenerations 60 -PopulationSize 20 -MutationProbability 0.7 -ReservedPopulationProportion 0.8 -BestOf $runs -Cores 4 -Stand $stand -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$recordTravelAndObjectives = Optimize-RecordTravel -Deviation 20 -StopAfter 300 -Stand $stand -BestOf $runs -Cores 4 -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

$delugeAndObjectives = Optimize-GreatDeluge -FinalWaterLevel 1100 -InitialWaterLevel 100 -RainRate 0.5 -StopAfter 1000 -BestOf $runs -Cores 4 -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabuAndObjectives = Optimize-Tabu -Iterations 600 -Tenure 50 -BestOf 4 -Cores 4 -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$annealerAndObjectives = Optimize-SimulatedAnnealing -Alpha 0.99 -InitialTemperature 100 -FinalTemperature 10 -IterationsPerTemperature 10 -BestOf $runs -Cores 4 -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

$heuristics = ($annealerAndObjectives[0], $acceptorAndObjectives[0], $geneticAndObjectives[0], $recordTravelAndObjectives[0], $delugeAndObjectives[0], $tabuAndObjectives[0])
$distributions = ($annealerAndObjectives[1], $acceptorAndObjectives[1], $geneticAndObjectives[1], $recordTravelAndObjectives[1], $delugeAndObjectives[1], $tabuAndObjectives[1])
Write-Harvest -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665scribner_harvest.csv"));
Write-HarvestSchedule -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665scribner_schedule.csv"));
Write-Objective -Heuristics $heuristics -Step 1 -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665scribner_objective.csv"));
Write-ObjectiveDistribution -Distributions $distributions -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665scribner_objectiveDistribution.csv"));

$annealerAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine((Get-Location), "FE640_project_sa665_objectiveDistribution.csv")) -Encoding utf8;
$acceptorAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine((Get-Location), "FE640_project_ta665_objectiveDistribution.csv")) -Encoding utf8;
$geneticAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine((Get-Location), "FE640_project_ga665_objectiveDistribution.csv")) -Encoding utf8;
$recordTravelAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine((Get-Location), "FE640_project_rt665_objectiveDistribution.csv")) -Encoding utf8;
$delugeAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine((Get-Location), "FE640_project_gd665_objectiveDistribution.csv")) -Encoding utf8;
$tabuAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine((Get-Location), "FE640_project_tu665_objectiveDistribution.csv")) -Encoding utf8;


# CVTS4
# 100 runs
$acceptorAndObjectives = Optimize-ThresholdAccepting -IterationsPerThreshold 100 -Thresholds (0.95, 0.96, 0.97, 0.98, 0.99, 1.0) -BestOf 100 -Cores 4 -Stand $stand -UniformHarvestProbability -Verbose
$geneticAndObjectives = Optimize-Genetic -MaximumGenerations 60 -PopulationSize 20 -MutationProbability 0.7 -ReservedPopulationProportion 0.8 -BestOf 100 -Cores 4 -Stand $stand -Verbose
$recordTravelAndObjectives = Optimize-RecordTravel -Deviation 20 -StopAfter 300 -Stand $stand -BestOf 100 -Cores 4 -UniformHarvestProbability -Verbose

$delugeAndObjectives = Optimize-GreatDeluge -FinalWaterLevel 1100 -InitialWaterLevel 200 -RainRate 0.25 -StopAfter 2000 -BestOf 100 -Cores 4 -Stand $stand -UniformHarvestProbability -Verbose
$tabuAndObjectives = Optimize-Tabu -Iterations 300 -Tenure 50 -Cores 4 -Stand $stand -UniformHarvestProbability -Verbose
$annealerAndObjectives = Optimize-SimulatedAnnealing -Alpha 0.99 -InitialTemperature 100 -FinalTemperature 10 -IterationsPerTemperature 10 -BestOf 100 -Cores 4 -Stand $stand -UniformHarvestProbability -Verbose

$geneticAndObjectives[0].BestTrajectory.IndividualTreeSelection


# singletons: parameters set for quick runs
# TA: 36.15 i/s debug, 75.57 release
$genetic = Optimize-Genetic -MaximumGenerations 30 -PopulationSize 10 -NetPresentValue -Stand $stand -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$acceptor = Optimize-ThresholdAccepting -IterationsPerThreshold 50 -Thresholds (0.90, 0.95, 1.0) -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$annealer = Optimize-SimulatedAnnealing -Alpha 0.9 -InitialTemperature 100 -FinalTemperature 10 -IterationsPerTemperature 10 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$recordTravel = Optimize-RecordTravel -Deviation 25 -StopAfter 100 -Stand $stand -UniformHarvestProbability -NetPresentValue -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$deluge = Optimize-GreatDeluge -FinalWaterLevel 1000 -RainRate 5 -StopAfter 50 -Stand $stand -UniformHarvestProbability -NetPresentValue -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabu = Optimize-Tabu -Iterations 2 -Tenure 2 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$hero = Optimize-Hero -Iterations 10 -Cores 4 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

$heuristics = ($annealer, $acceptor, $genetic, $recordTravel, $deluge, $tabu, $hero)
Write-Harvest -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665singleton_harvest.csv"));
Write-HarvestSchedule -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665singleton_schedule.csv"));
Write-Objective -Heuristics $heuristics -Step 1 -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665singleton_objective.csv"));

# quads: parameters set for quick runs
$geneticAndObjectives = Optimize-Genetic -MaximumGenerations 30 -PopulationSize 10 -BestOf 4 -NetPresentValue -Stand $stand -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$acceptorAndObjectives = Optimize-ThresholdAccepting -IterationsPerThreshold 50 -Thresholds (0.90, 0.95, 1.0) -BestOf 4 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$annealerAndObjectives = Optimize-SimulatedAnnealing -Alpha 0.9 -InitialTemperature 100 -FinalTemperature 10 -IterationsPerTemperature 10 -BestOf 4 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$recordTravelAndObjectives = Optimize-RecordTravel -Deviation 25 -StopAfter 100 -Stand $stand -UniformHarvestProbability -BestOf 4 -NetPresentValue -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$delugeAndObjectives = Optimize-GreatDeluge -FinalWaterLevel 1000 -RainRate 5 -StopAfter 50 -Stand $stand -UniformHarvestProbability -BestOf 4 -NetPresentValue -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$tabuAndObjectives = Optimize-Tabu -Iterations 2 -Tenure 2 -BestOf 4 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
$heroAndObjectives = Optimize-Hero -Iterations 10 -BestOf 4 -Cores 4 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

# quads: parameters set for exploration
$geneticAndObjectives = Optimize-Genetic -MaximumGenerations 60 -PopulationSize 10 -MutationProbability 0.7 -ReservedPopulationProportion 0.8 -EndStandardDeviation 0.000001 -HarvestPeriods 4 -BestOf 4 -NetPresentValue -Stand $stand -VolumeUnits ScribnerBoardFeetPerAcre -Verbose
Write-Harvest -Heuristics $geneticAndObjectives[0] -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665gaQuad_harvest.csv"));

$heroAndObjectives = Optimize-Hero -Iterations 10 -BestOf 8 -Cores 4 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose


$heuristics = ($annealerAndObjectives[0], $acceptorAndObjectives[0], $geneticAndObjectives[0], $recordTravelAndObjectives[0], $delugeAndObjectives[0], $tabuAndObjectives[0], $heroAndObjectives[0])
$distributions = ($annealerAndObjectives[1], $acceptorAndObjectives[1], $geneticAndObjectives[1], $recordTravelAndObjectives[1], $delugeAndObjectives[1], $tabuAndObjectives[1], $heroAndObjectives[1])
Write-Harvest -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665quad_harvest.csv"));
Write-HarvestSchedule -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665quad_schedule.csv"));
Write-Objective -Heuristics $heuristics -Step 1 -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665quad_objective.csv"));
Write-ObjectiveDistribution -Distribution $distributions -Heuristics $heuristics -CsvFile ([System.IO.Path]::Combine((Get-Location), "FE640_project_665quad_objectiveDistribution.csv"));

$geneticAndObjectives[0].BestTrajectory.HarvestVolumesByPeriod
$geneticAndObjectives[0].BestTrajectory.StandingVolumeByPeriod

$deluge.ObjectiveFunctionByIteration.Count
$recordTravel.ObjectiveFunctionByIteration.Count
$tabu.ObjectiveFunctionByIteration
$acceptor.ObjectiveFunctionByIteration.Count
$genetic.ObjectiveFunctionByIteration
$annealer.ObjectiveFunctionByIteration
$hero.ObjectiveFunctionByIteration

$deluge.BestTrajectory.IndividualTreeSelection
$recordTravel.BestTrajectory.IndividualTreeSelection
$tabu.BestTrajectory.IndividualTreeSelection
$acceptor.BestTrajectory.IndividualTreeSelection
$genetic.BestTrajectory.IndividualTreeSelection
$acceptor.BestTrajectory.IndividualTreeSelection
$hero.BestTrajectory.IndividualTreeSelection