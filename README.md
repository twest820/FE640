### Summary
This repo contains problem set implementations for Oregon State University's combinatorial optimization course (FE 640), 
winter quarter 2020. The forest harvest scheduling heuristics implemented are

* simulated annealing
* threshold accepting

Heuristics are accessible from their C# classes and by loading the algorithms assembly as a PowerShell module. PowerShell 
cmdlets are

* Get-Units
* Optimize-SimulatedAnnealing
* Optimize-ThresholdAccepting
* Write-Harvest
* Write-HarvestSchedule
* Write-Objective

The PowerShell scripts (.ps1) and R scripts (.R) in the scripts directory provide examples of cmdlet use and analysis of
heuristic performance.

### Development Enviroment
[Visual Studio Community 2019](https://visualstudio.microsoft.com/downloads/), PowerShell 5.1 (the Windows 10 default), and [RStudio 1.5](https://rstudio.com/products/rstudio/download/) with R 3.6.1.