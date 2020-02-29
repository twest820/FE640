$workingDirectory = Get-Location
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Debug")
$buildDirectory = [System.IO.Path]::Combine($workingDirectory, "UnitTests\bin\x64\Release")
Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "FE640.dll"));
$harvestWeights = (0.0, 2.86, 2.20, 1.69, 1.30, 1.00)
#$harvestWeights = (0.0, 2.856, 2.197, 1.690, 1.300, 1.000)

# 100 unit problem
# record to record travel control: 23,770 units/period, 8.6 σ
$units = Get-Units -Units 100 -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20.xlsx"));
$targetHarvestPerPeriod = 23900

$harmony = Optimize-Harmony -Generations 25000 -MaximumBandwidth 4 -MinimumBandwidth 4 -MemoryRate 1 -MemorySize 10 -MaximumPitchAdjustmentRate 0.1 -MinimumPitchAdjustmentRate 0.1 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$harmony.BestHarvestByPeriod

$harmonyAndObjectives = Optimize-Harmony -Generations 20000 -MaximumBandwidth 1.9 -MinimumBandwidth 1.9 -MemoryRate 0.985 -MemorySize 10 -MaximumPitchAdjustmentRate 0.005 -MinimumPitchAdjustmentRate 0.001 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$harmonyAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $harmonyAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_hs100_schedule.csv"));
Write-Objective -Heuristics $harmonyAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_hs100_objective.csv"));
$harmonyAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_hs100_objectiveDistribution.csv")) -Encoding utf8;

# global harmony search probing
$harmonyAndObjectives = Optimize-Harmony -Generations 10000 -MemoryRate 0.985 -MemorySize 10 -MaximumPitchAdjustmentRate 0.05 -MinimumPitchAdjustmentRate 0.01 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;


#$tabu = Optimize-Tabu -Tenure 30 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
#$units.SetBestSchedule($tabu)
$swarm = Optimize-ParticleSwarm -Inertia 0.5 -CognitiveConstant 0.001 -SocialConstant 0.0005 -TimeSteps 1000 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$swarm.BestHarvestByPeriod

# -Inertia 0.6 -CognitiveConstant 0.3 -SocialConstant 0.15
$swarmAndObjectives = Optimize-ParticleSwarm -Particles 1000 -Inertia 0.6 -CognitiveConstant 1.2 -SocialConstant 0.8 -TimeSteps 1000 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$swarmAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $swarmAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps100_schedule.csv"));
Write-Objective -Heuristics $swarmAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps100_objective.csv"));
$swarmAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps100_objectiveDistribution.csv")) -Encoding utf8;

# 200 unit problem: 13.2 sigma
$units = Get-Units -Units 200 -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20.xlsx"));
$targetHarvestPerPeriod = 46000
#$units.SetRandomSchedule()
#$tabu = Optimize-Tabu -Tenure 300 -Iterations 1000 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;

$swarmAndObjectives = Optimize-ParticleSwarm -Particles 30 -Inertia 0.6 -CognitiveConstant 1.2 -SocialConstant 0.8 -TimeSteps 1000 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$swarmAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $swarmAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps200_schedule.csv"));
Write-Objective -Heuristics $swarmAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps200_objective.csv"));
$swarmAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps200_objectiveDistribution.csv")) -Encoding utf8;

# 500 unit problem: 28 sigma
$units = Get-Units -Units 500 -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20.xlsx"));
$targetHarvestPerPeriod = 110500
#$units.SetRandomSchedule()
#$tabu = Optimize-Tabu -Tenure 300 -Iterations 1000 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;

$swarmAndObjectives = Optimize-ParticleSwarm -Particles 30 -Inertia 0.6 -CognitiveConstant 1.2 -SocialConstant 0.8 -TimeSteps 1000 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$swarmAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $swarmAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps500_schedule.csv"));
Write-Objective -Heuristics $swarmAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps500_objective.csv"));
$swarmAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps500_objectiveDistribution.csv")) -Encoding utf8;

# 1000 unit problem: 28 sigma
$units = Get-Units -Units 1000 -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20.xlsx"));
$targetHarvestPerPeriod = 219000
#$units.SetRandomSchedule()
#$tabu = Optimize-Tabu -Tenure 300 -Iterations 1000 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;

$swarmAndObjectives = Optimize-ParticleSwarm -Particles 30 -Inertia 0.6 -CognitiveConstant 1.2 -SocialConstant 0.8 -TimeSteps 1000 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$swarmAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $swarmAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps1k_schedule.csv"));
Write-Objective -Heuristics $swarmAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps1k_objective.csv"));
$swarmAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps1k_objectiveDistribution.csv")) -Encoding utf8;

# 2000 unit problem
$units = Get-Units -UnitXlsx ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20.xlsx"));
$targetHarvestPerPeriod = 439800

$harmony = Optimize-Harmony -Generations 100E3 -MaximumBandwidth 1.9 -MinimumBandwidth 1.9 -MemoryRate 0.99 -MemorySize 25 -MaximumPitchAdjustmentRate 0.000001 -MinimumPitchAdjustmentRate 0.0000001 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$harmony.BestHarvestByPeriod

$harmonyAndObjectives = Optimize-Harmony -Generations 100E3 -MaximumBandwidth 1.9 -MinimumBandwidth 1.9 -MemoryRate 0.9995 -MemorySize 25 -MaximumPitchAdjustmentRate 0.00005 -MinimumPitchAdjustmentRate 0.00001 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
$harmonyAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $harmonyAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_hs2k_schedule.csv"));
Write-Objective -Heuristics $harmonyAndObjectives[0] -Step 1 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_hs2k_objective.csv"));
$harmonyAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_hs2k_objectiveDistribution.csv")) -Encoding utf8;


#$units.SetRandomSchedule()
#$tabu = Optimize-Tabu -Tenure 300 -Iterations 1000 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose;
#$units.SetBestSchedule($tabu)

$swarm = Optimize-ParticleSwarm -Particles 30 -Inertia 0.7 -CognitiveConstant 1.2 -SocialConstant 0.8 -Units $units -TimeSteps 1000 -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose

$swarmAndObjectives = Optimize-ParticleSwarm -Particles 30 -Inertia 0.6 -CognitiveConstant 1.2 -SocialConstant 0.8 -TimeSteps 1000 -BestOf 100 -Units $units -TargetHarvestPerPeriod $targetHarvestPerPeriod -TargetHarvestWeights $harvestWeights -Verbose
$swarmAndObjectives[0].BestHarvestByPeriod
Write-HarvestSchedule -Heuristics $swarmAndObjectives[0] -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps2k_schedule.csv"));
Write-Objective -Heuristics $swarmAndObjectives[0] -Step 10 -CsvFile ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps2k_objective.csv"));
$swarmAndObjectives[1] | Out-File -FilePath ([System.IO.Path]::Combine($buildDirectory, "FE640_set5_20_ps2k_objectiveDistribution.csv")) -Encoding utf8;
