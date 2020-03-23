library(ggplot2)
library(gridExtra)
library(reshape2)
library(scales)

ggtheme = theme_bw() + theme(axis.line = element_line(), panel.border = element_blank(), plot.margin = unit(c(0.1, 0.05, 0.1, 0.1), "cm"), axis.title.x = element_text(margin = margin(0, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, -2, 0, 0)))
theme_set(ggtheme)
options(scipen = 10)

heuristicNames = c("simulated annealing", "threshold accepting", "genetic", "record travel", "great deluge", "tabu", "hero")
heuristicAbbreviations = c("SA", "TA", "GA", "RT", "GD", "TU", "HE")
heuristicAbbreviations = c("S", "A", "G", "R", "D", "T", "H")
runs = c(rep(100, 5), 4, 100)

getObjectiveDistribution = function(harvestPeriod, heroDistribution, tabuDistribution)
{
  objectiveDistribution = read.csv(file.path(getwd(), paste0("project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntry", harvestPeriod, "_objectiveDistribution.csv")))
  tasartDistribution = read.csv(file.path(getwd(), paste0("project\\NpvSingleEntryIII\\FE640_project_665npvTASART", harvestPeriod, "_objectiveDistribution.csv")))
  objectiveDistribution$RecordTravel = tasartDistribution$RecordTravel
  objectiveDistribution$SimulatedAnnealing = tasartDistribution$SimulatedAnnealing
  objectiveDistribution$ThresholdAccepting = tasartDistribution$ThresholdAccepting
  objectiveDistribution = cbind(objectiveDistribution[, 2:ncol(objectiveDistribution)], Tabu = tabuDistribution[, harvestPeriod + 1], Hero = heroDistribution[, harvestPeriod + 1])
  return(objectiveDistribution)
}

getScheduleCorrelation = function(harvestPeriods, heuristicPaths)
{
  heuristicSchedules = list()
  for (heuristicIndex in 1:length(heuristicPaths))
  {
    heuristicSchedule = read.csv(file.path(getwd(), heuristicPaths[heuristicIndex]))
    heuristicSchedules[[heuristicIndex]] = heuristicSchedule[, 2:ncol(heuristicSchedule)]
  }
  
  schedule = read.csv(file.path(getwd(), paste0("project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntry", 1, "_schedule.csv")))
  schedule = schedule[, 2:ncol(schedule)]
  tasartSchedule = read.csv(file.path(getwd(), paste0("project\\NpvSingleEntryIII\\FE640_project_665npvTASART", 1, "_schedule.csv")))
  schedule$RecordTravel = tasartSchedule$RecordTravel
  schedule$SimulatedAnnealing = tasartSchedule$SimulatedAnnealing
  schedule$ThresholdAccepting = tasartSchedule$ThresholdAccepting
  for (heuristicIndex in 1:length(heuristicPaths))
  {
    schedule = cbind(schedule, heuristicSchedules[[heuristicIndex]][1])
  }
  colnames(schedule) = paste(heuristicAbbreviations[1:ncol(schedule)], 1, sep = "")
  
  for (harvestPeriod in 2:harvestPeriods)
  {
    nextSchedule = read.csv(file.path(getwd(), paste0("project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntry", harvestPeriod, "_schedule.csv")))
    nextSchedule = nextSchedule[, 2:ncol(nextSchedule)]
    tasartSchedule = read.csv(file.path(getwd(), paste0("project\\NpvSingleEntryIII\\FE640_project_665npvTASART", harvestPeriod, "_schedule.csv")))
    nextSchedule$RecordTravel = tasartSchedule$RecordTravel
    nextSchedule$SimulatedAnnealing = tasartSchedule$SimulatedAnnealing
    nextSchedule$ThresholdAccepting = tasartSchedule$ThresholdAccepting
    for (heuristicIndex in 1:length(heuristicPaths))
    {
      nextSchedule = cbind(nextSchedule, heuristicSchedules[[heuristicIndex]][harvestPeriod])
    }
    colnames(nextSchedule) = paste(heuristicAbbreviations[1:ncol(nextSchedule)], harvestPeriod, sep = "")
    schedule = cbind(schedule, nextSchedule)
  }
  correlation = cor(schedule)
  correlation = melt(correlation, value.name = "correlation")
  return(correlation)
}

getStats = function(objectiveDistribution, coreSeconds)
{
  stats = data.frame(max = apply(objectiveDistribution, 2, max, na.rm = TRUE),
                     mean = colMeans(objectiveDistribution, na.rm = TRUE),
                     min = apply(objectiveDistribution, 2, min, na.rm = TRUE),
                     variance = apply(objectiveDistribution, 2, var, na.rm = TRUE))
  #stats$confidenceHalfwidth = qt(0.975, runs - 1) * sqrt(npv1$mean / runs)
  stats$heuristic = rownames(stats)
  stats$coreSeconds = coreSeconds / runs
  return(stats)
}

getVolume = function(harvestPeriod, stats, tabuHeroVolume)
{
  volume = read.csv(file.path(getwd(), paste0("project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntry", harvestPeriod, "_harvest.csv")))
  volume = subset(volume, period != 0)
  
  tasartVolume = read.csv(file.path(getwd(), paste0("project\\NpvSingleEntryIII\\FE640_project_665npvTASART", harvestPeriod, "_harvest.csv")))
  tasartVolume = subset(tasartVolume, period != 0)
  volume$RecordTravelH = tasartVolume$RecordTravelH
  volume$SimulatedAnnealingH = tasartVolume$SimulatedAnnealingH
  volume$ThresholdAcceptingH = tasartVolume$ThresholdAcceptingH
  volume$RecordTravelS = tasartVolume$RecordTravelS
  volume$SimulatedAnnealingS = tasartVolume$SimulatedAnnealingS
  volume$ThresholdAcceptingS = tasartVolume$ThresholdAcceptingS
  
  volume = cbind(volume, tabuHeroVolume)
  volume = melt(volume, id.vars = "period", na.rm = TRUE)
  colnames(volume) = c("period", "ID", "volume")

  volume$heuristic = NA
  volume$standing = TRUE
  volume$age = 5 * volume$period + 15
  for (heuristicIndex in 1:nrow(stats))
  {
    heuristicName = rownames(stats)[heuristicIndex]
    
    harvestName = paste(heuristicName, "H", sep = "")
    harvestIndices = which(volume$ID == harvestName)
    volume$heuristic[harvestIndices] = rep(heuristicName, length(harvestIndices))
    volume$standing[harvestIndices] = FALSE
    
    standingName = paste(heuristicName, "S", sep = "")
    standingIndices = which(volume$ID == standingName)
    volume$heuristic[standingIndices] = rep(heuristicName, length(standingIndices))
  }
  return(volume)  
}

reshapeObjectiveDistribution = function(distribution, stats)
{
  reshape = melt(distribution, na.rm = TRUE)
  colnames(reshape) = c("heuristic", "npv")
  reshape$coreSeconds = NA
  for (heuristicIndex in 1:nrow(stats))
  {
    heuristicName = stats$heuristic[heuristicIndex]
    heuristicIndices = which(reshape$heuristic == heuristicName)
    reshape$coreSeconds[heuristicIndices] = rep(stats$coreSeconds[heuristicIndex], length(heuristicIndices))
  }
  return(reshape)
}

heroObjectiveDistribution = read.csv(file.path(getwd(), "project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntryHero_objectiveDistribution.csv"))
tabuObjectiveDistribution = read.csv(file.path(getwd(), "project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntryTabu_objectiveDistribution.csv"))
heroVolume = read.csv(file.path(getwd(), "project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntryHero_harvest.csv"))
heroVolume = subset(heroVolume, period != 0)
tabuVolume = read.csv(file.path(getwd(), "project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntryTabu_harvest.csv"))
tabuVolume = subset(tabuVolume, period != 0)

npv1objectiveDistribution = getObjectiveDistribution(1, heroObjectiveDistribution, tabuObjectiveDistribution)
#                                                 SA   TA    GA    RT    GD    TU     HE
npv1stats = getStats(npv1objectiveDistribution, c(3904, 2117, 1068, 5936, 3333, 29088, 6341))
npv1 = reshapeObjectiveDistribution(npv1objectiveDistribution, npv1stats)
npv1volume = getVolume(1, npv1stats, cbind(TabuH = tabuVolume[, 2], TabuS = tabuVolume[, 10], HeroH = heroVolume[, 2], HeroS = heroVolume[, 10]))

npvScheduleCorrelation = getScheduleCorrelation(8, c("project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntryTabu_schedule.csv", 
                                                     "project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntryHero_schedule.csv"))
periodVertices = seq(0.5, 8 * 7 + 0.5, by = 7)
ggplot(npvScheduleCorrelation, aes(x = Var1, y = Var2)) + geom_raster(aes(fill = correlation)) +
  geom_rect(aes(xmin = periodVertices[1], ymin = periodVertices[8], xmax = periodVertices[2], ymax = periodVertices[9]), color = "grey90", fill = NA) +
  geom_rect(aes(xmin = periodVertices[2], ymin = periodVertices[7], xmax = periodVertices[3], ymax = periodVertices[8]), color = "grey90", fill = NA) +
  geom_rect(aes(xmin = periodVertices[3], ymin = periodVertices[6], xmax = periodVertices[4], ymax = periodVertices[7]), color = "grey90", fill = NA) +
  geom_rect(aes(xmin = periodVertices[4], ymin = periodVertices[5], xmax = periodVertices[5], ymax = periodVertices[6]), color = "grey90", fill = NA) +
  geom_rect(aes(xmin = periodVertices[5], ymin = periodVertices[4], xmax = periodVertices[6], ymax = periodVertices[5]), color = "grey90", fill = NA) +
  geom_rect(aes(xmin = periodVertices[6], ymin = periodVertices[3], xmax = periodVertices[7], ymax = periodVertices[4]), color = "grey90", fill = NA) +
  geom_rect(aes(xmin = periodVertices[7], ymin = periodVertices[2], xmax = periodVertices[8], ymax = periodVertices[3]), color = "grey90", fill = NA) +
  geom_rect(aes(xmin = periodVertices[8], ymin = periodVertices[1], xmax = periodVertices[9], ymax = periodVertices[2]), color = "grey90", fill = NA) +
  scale_fill_viridis_c(limits = c(-0.2, 1)) + scale_y_discrete(limits = rev(levels(npvScheduleCorrelation$Var2))) + labs(fill = "correlation") + 
  xlab(NULL) + ylab(NULL) + theme(axis.text = element_text(size = 7), legend.background = element_rect(fill = alpha("white", 0)), legend.justification = c(1, 1), legend.position = c(0.995, 0.99), legend.text = element_text(color = "white"), legend.title = element_text(color = "white"))
ggsave(device = "png", path = file.path(getwd(), "project\\figures"), filename = "npv1correlation.png", dpi = "print", width = 8, height = 4.5, units = "in")

ggplot(npv1, aes(x = coreSeconds, y = npv, fill = heuristic)) + geom_boxplot(color = "grey40", outlier.shape = 3, outlier.size = 0.75, position = "identity", width = 0.1) +
  stat_summary(fun.y = mean, geom = "point", shape = 21, color = "grey40", size = 2.5) +
  stat_summary(fun.y = max, geom = "point", shape = 24, color = "grey40", size = 2) +
  scale_fill_viridis_d(labels = heuristicNames) + scale_x_continuous(limits = c(2, 10000), trans = "log10") +
  labs(fill = NULL) + xlab("i7-3770 core-seconds per run") + ylab("net present value, $1000/acre") +
  theme(axis.title.x = element_text(margin = margin(2, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, 2, 0, 0)), legend.justification = c(1, 0), legend.position = c(0.99, 0.01))
ggsave(device = "png", path = file.path(getwd(), "project\\figures"), filename = "npv1objective.png", dpi = "print", width = 4.5, height = 4, units = "in")

ggplot(npv1volume, aes(x = age, volume, color = heuristic)) + geom_line(aes(group = ID), size = 0.6) +
  geom_point(aes(shape = standing), size = 2.2) + scale_color_viridis_d(labels = heuristicNames) + 
  scale_shape_manual(labels = c("harvest in period", "standing"), values = c(18, 19)) +
  scale_y_continuous(limits = c(0, 65)) + labs(color = NULL, shape = NULL) + 
  xlab("stand age, years") + ylab("volume, MBF")
ggsave(device = "png", path = file.path(getwd(), "project\\figures"), filename = "npv1volume.png", dpi = "print", width = 4.5, height = 4, units = "in")



npv2objectiveDistribution = getObjectiveDistribution(2, heroObjectiveDistribution, tabuObjectiveDistribution)
#                                                 SA    TA    GA   RT    GD    TU     HE
npv2stats = getStats(npv2objectiveDistribution, c(4161, 2270, 593, 6420, 3389, 29576, 5816))
npv2 = reshapeObjectiveDistribution(npv2objectiveDistribution, npv2stats)
npv2volume = getVolume(2, npv2stats, cbind(TabuH = tabuVolume[, 3], TabuS = tabuVolume[, 11], HeroH = heroVolume[, 3], HeroS = heroVolume[, 11]))

npv3objectiveDistribution = getObjectiveDistribution(3, heroObjectiveDistribution, tabuObjectiveDistribution)
#                                                 SA    TA    GA   RT    GD    TU     HE
npv3stats = getStats(npv3objectiveDistribution, c(4098, 2260, 532, 6199, 3951, 28347, 6724))
npv3 = reshapeObjectiveDistribution(npv3objectiveDistribution, npv3stats)
npv3volume = getVolume(3, npv3stats, cbind(TabuH = tabuVolume[, 4], TabuS = tabuVolume[, 12], HeroH = heroVolume[, 4], HeroS = heroVolume[, 12]))

npv4objectiveDistribution = getObjectiveDistribution(4, heroObjectiveDistribution, tabuObjectiveDistribution)
#                                                 SA    TA    GA   RT    GD    TU     HE
npv4stats = getStats(npv4objectiveDistribution, c(4110, 2258, 539, 6198, 3230, 27302, 8544))
npv4 = reshapeObjectiveDistribution(npv4objectiveDistribution, npv4stats)
npv4volume = getVolume(4, npv4stats, cbind(TabuH = tabuVolume[, 5], TabuS = tabuVolume[, 13], HeroH = heroVolume[, 5], HeroS = heroVolume[, 13]))

npv5objectiveDistribution = getObjectiveDistribution(5, heroObjectiveDistribution, tabuObjectiveDistribution)
#                                                 SA    TA    GA   RT    GD    TU     HE
npv5stats = getStats(npv5objectiveDistribution, c(4193, 2292, 455, 5147, 3216, 27189, 7909))
npv5 = reshapeObjectiveDistribution(npv5objectiveDistribution, npv5stats)
npv5volume = getVolume(5, npv5stats, cbind(TabuH = tabuVolume[, 6], TabuS = tabuVolume[, 14], HeroH = heroVolume[, 6], HeroS = heroVolume[, 14]))

npv6objectiveDistribution = getObjectiveDistribution(6, heroObjectiveDistribution, tabuObjectiveDistribution)
#                                                 SA    TA    GA   RT    GD    TU     HE
npv6stats = getStats(npv6objectiveDistribution, c(4170, 2271, 341, 3682, 3260, 27791, 9267))
npv6 = reshapeObjectiveDistribution(npv6objectiveDistribution, npv6stats)
npv6volume = getVolume(6, npv6stats, cbind(TabuH = tabuVolume[, 7], TabuS = tabuVolume[, 15], HeroH = heroVolume[, 7], HeroS = heroVolume[, 15]))

npv7objectiveDistribution = getObjectiveDistribution(7, heroObjectiveDistribution, tabuObjectiveDistribution)
#                                                 SA    TA    GA   RT    GD    TU     HE
npv7stats = getStats(npv7objectiveDistribution, c(4546, 2469, 235, 2914, 3381, 28576, 8559))
npv7 = reshapeObjectiveDistribution(npv7objectiveDistribution, npv7stats)
npv7volume = getVolume(7, npv7stats, cbind(TabuH = tabuVolume[, 8], TabuS = tabuVolume[, 16], HeroH = heroVolume[, 8], HeroS = heroVolume[, 16]))

npv8objectiveDistribution = getObjectiveDistribution(8, heroObjectiveDistribution, tabuObjectiveDistribution)
#                                                 SA    TA    GA   RT    GD    TU     HE
npv8stats = getStats(npv8objectiveDistribution, c(4767, 2565, 173, 2536, 3451, 28000, 11009))
npv8 = reshapeObjectiveDistribution(npv8objectiveDistribution, npv8stats)
npv8volume = getVolume(8, npv8stats, cbind(TabuH = tabuVolume[, 9], TabuS = tabuVolume[, 17], HeroH = heroVolume[, 9], HeroS = heroVolume[, 17]))


npv25plot = ggplot(npv2, aes(x = coreSeconds, y = npv, fill = heuristic)) + geom_boxplot(color = "grey40", outlier.shape = 3, outlier.size = 0.75, position = "identity", width = 0.1) +
  stat_summary(fun.y = mean, geom = "point", shape = 21, color = "grey40", size = 2.5) +
  stat_summary(fun.y = max, geom = "point", shape = 24, color = "grey40", size = 2) +
  scale_fill_viridis_d(labels = heuristicNames) + scale_x_continuous(limits = c(2, 10500), trans = "log10") +
  scale_y_continuous(limits = c(6.2, 8.0)) + labs(fill = "thin at year 25") + xlab("") + ylab("NPV, $1000/acre") +
  theme(axis.title.x = element_text(margin = margin(2, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, 2, 0, 0)), legend.justification = c(1, 0), legend.position = "none")
npv30plot = ggplot(npv3, aes(x = coreSeconds, y = npv, fill = heuristic)) + geom_boxplot(color = "grey40", outlier.shape = 3, outlier.size = 0.75, position = "identity", width = 0.1) +
  stat_summary(fun.y = mean, geom = "point", shape = 21, color = "grey40", size = 2.5) +
  stat_summary(fun.y = max, geom = "point", shape = 24, color = "grey40", size = 2) +
  scale_fill_viridis_d(labels = heuristicNames) + scale_x_continuous(limits = c(2, 10500), trans = "log10") +
  scale_y_continuous(limits = c(6.2, 8.0)) + labs(fill = "thin at year 30") + xlab("") + ylab("") +
  theme(axis.title.x = element_text(margin = margin(2, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, 2, 0, 0)), legend.justification = c(1, 0), legend.position = "none")
npv35plot = ggplot(npv4, aes(x = coreSeconds, y = npv, fill = heuristic)) + geom_boxplot(color = "grey40", outlier.shape = 3, outlier.size = 0.75, position = "identity", width = 0.1) +
  stat_summary(fun.y = mean, geom = "point", shape = 21, color = "grey40", size = 2.5) +
  stat_summary(fun.y = max, geom = "point", shape = 24, color = "grey40", size = 2) +
  scale_fill_viridis_d(labels = heuristicNames) + scale_x_continuous(limits = c(2, 10500), trans = "log10") +
  scale_y_continuous(limits = c(6.2, 8.0)) + labs(fill = "thin at year 35") + xlab("") + ylab("") +
  theme(axis.title.x = element_text(margin = margin(2, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, 2, 0, 0)), legend.justification = c(1, 0), legend.position = "none")
npv40plot = ggplot(npv5, aes(x = coreSeconds, y = npv, fill = heuristic)) + geom_boxplot(color = "grey40", outlier.shape = 3, outlier.size = 0.75, position = "identity", width = 0.1) +
  stat_summary(fun.y = mean, geom = "point", shape = 21, color = "grey40", size = 2.5) +
  stat_summary(fun.y = max, geom = "point", shape = 24, color = "grey40", size = 2) +
  scale_fill_viridis_d(labels = heuristicNames) + scale_x_continuous(limits = c(2, 10500), trans = "log10") +
  scale_y_continuous(limits = c(6.2, 8.0)) + labs(fill = "thin at year 40") + xlab("i7-3770 core-seconds per run") + ylab("NPV, $1000/acre") +
  theme(axis.title.x = element_text(margin = margin(2, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, 2, 0, 0)), legend.justification = c(1, 0), legend.position = "none")
npv45plot = ggplot(npv6, aes(x = coreSeconds, y = npv, fill = heuristic)) + geom_boxplot(color = "grey40", outlier.shape = 3, outlier.size = 0.75, position = "identity", width = 0.1) +
  stat_summary(fun.y = mean, geom = "point", shape = 21, color = "grey40", size = 2.5) +
  stat_summary(fun.y = max, geom = "point", shape = 24, color = "grey40", size = 2) +
  scale_fill_viridis_d(labels = heuristicNames) + scale_x_continuous(limits = c(2, 10500), trans = "log10") +
  scale_y_continuous(limits = c(6.2, 8.0)) + labs(fill = "thin at year 45") + xlab("i7-3770 core-seconds per run") + ylab("") +
  theme(axis.title.x = element_text(margin = margin(2, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, 2, 0, 0)), legend.justification = c(1, 0), legend.position = "none")
npv50plot = ggplot(npv7, aes(x = coreSeconds, y = npv, fill = heuristic)) + geom_boxplot(color = "grey40", outlier.shape = 3, outlier.size = 0.75, position = "identity", width = 0.1) +
  stat_summary(fun.y = mean, geom = "point", shape = 21, color = "grey40", size = 2.5) +
  stat_summary(fun.y = max, geom = "point", shape = 24, color = "grey40", size = 2) +
  scale_fill_viridis_d(labels = heuristicNames) + scale_x_continuous(limits = c(2, 10500), trans = "log10") +
  scale_y_continuous(limits = c(6.2, 8.0)) + labs(fill = NULL) + xlab("i7-3770 core-seconds per run") + ylab("") +
  theme(axis.title.x = element_text(margin = margin(2, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, 2, 0, 0)), legend.background = element_rect(fill = alpha("white", 0)), legend.justification = c(1, 0), legend.position = c(1, 0.01))
grid.arrange(npv25plot, npv30plot, npv35plot, npv40plot, npv45plot, npv50plot, nrow = 2, ncol = 3)

npvPlot = arrangeGrob(npv25plot, npv30plot, npv35plot, npv40plot, npv45plot, npv50plot, nrow = 2, ncol = 3)
ggsave(plot = npvPlot, device = "png", path = file.path(getwd(), "project\\figures"), filename = "npv2-7objective.png", dpi = "print", width = 10, height = 5.625, units = "in")


v25plot = ggplot(npv2volume, aes(x = age, volume, color = heuristic)) + geom_line(aes(group = ID), size = 0.6) +
  geom_point(aes(shape = standing), size = 2.2) + scale_color_viridis_d(labels = heuristicNames) + 
  scale_shape_manual(labels = c("harvest in period", "standing"), values = c(18, 19)) +
  scale_y_continuous(limits = c(0, 65)) + labs(color = NULL, shape = NULL) + 
  xlab("") + ylab("volume, MBF") + theme(legend.position = "none")
v30plot = ggplot(npv3volume, aes(x = age, volume, color = heuristic)) + geom_line(aes(group = ID), size = 0.6) +
  geom_point(aes(shape = standing), size = 2.2) + scale_color_viridis_d(labels = heuristicNames) + 
  scale_shape_manual(labels = c("harvest in period", "standing"), values = c(18, 19)) +
  scale_y_continuous(limits = c(0, 65)) + labs(color = NULL, shape = NULL) + 
  xlab("") + ylab("") + theme(legend.position = "none")
v35plot = ggplot(npv4volume, aes(x = age, volume, color = heuristic)) + geom_line(aes(group = ID), size = 0.6) +
  geom_point(aes(shape = standing), size = 2.2) + scale_color_viridis_d(labels = heuristicNames) + 
  scale_shape_manual(labels = c("harvest in period", "standing"), values = c(18, 19)) +
  scale_y_continuous(limits = c(0, 65)) + labs(color = NULL, shape = NULL) + 
  xlab("") + ylab("") + theme(legend.position = "none")
v40plot = ggplot(npv5volume, aes(x = age, volume, color = heuristic)) + geom_line(aes(group = ID), size = 0.6) +
  geom_point(aes(shape = standing), size = 2.2) + scale_color_viridis_d(labels = heuristicNames) + 
  scale_shape_manual(labels = c("harvest in period", "standing"), values = c(18, 19)) +
  scale_y_continuous(limits = c(0, 65)) + labs(color = NULL, shape = NULL) + 
  xlab("stand age, years") + ylab("volume, MBF") + theme(legend.position = "none")
v45plot = ggplot(npv6volume, aes(x = age, volume, color = heuristic)) + geom_line(aes(group = ID), size = 0.6) +
  geom_point(aes(shape = standing), size = 2.2) + scale_color_viridis_d(labels = heuristicNames) + 
  scale_shape_manual(labels = c("harvest in period", "standing"), values = c(18, 19)) +
  scale_y_continuous(limits = c(0, 65)) + labs(color = NULL, shape = NULL) + 
  xlab("stand age, years") + ylab("") + theme(legend.position = "none")
v50plot = ggplot(npv6volume, aes(x = age, volume, color = heuristic)) + geom_line(aes(group = ID), size = 0.6) +
  geom_point(aes(shape = standing), size = 2.2) + scale_color_viridis_d(labels = heuristicNames) + 
  scale_shape_manual(labels = c("harvest in period", "standing"), values = c(18, 19)) +
  scale_y_continuous(limits = c(0, 65)) + labs(color = NULL, shape = NULL) + 
  xlab("stand age, years") + ylab("") + theme(legend.position = "none")

vPlot = arrangeGrob(v25plot, v30plot, v35plot, v40plot, v45plot, v50plot, nrow = 2, ncol = 3)
ggsave(plot = vPlot, device = "png", path = file.path(getwd(), "project\\figures"), filename = "npv2-7volume.png", dpi = "print", width = 10, height = 5.625, units = "in")

age20thin30 = read.csv(file.path(getwd(), "project\\Malcom Knapp Nelder probabilities.csv"))
ggplot(age20thin30, aes(x = DBH..mm, y = top.3.probability)) + geom_bin2d() +
  scale_fill_viridis_c() + labs(fill = "trees") + xlab("DBH at age 20, mm") + ylab("probability of selection at age 30") +
  theme(axis.title.x = element_text(margin = margin(2, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, 2, 0, 0)), legend.margin = margin(0, 10, 0, -9))
ggsave(device = "png", path = file.path(getwd(), "project\\figures"), filename = "pthin3.png", dpi = "print", width = 4.5, height = 4, units = "in")
colMeans(cbind(deluge = age20thin30$Deluge, hero = age20thin30$Hero, tabu = age20thin30$Tabu)) / 3
colSums(pi/(4*304.8^2) * age20thin30$DBH..mm^2 * cbind(ba = 1, baHero = age20thin30$Hero / 3))

n = nrow(age20thin30)
age30Trajectories = read.csv(file.path(getwd(), "project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntry3_objective.csv"))
age30TrajectoriesTasart = read.csv(file.path(getwd(), "project\\NpvSingleEntryIII\\FE640_project_665npvTASART3_objective.csv"))
age30Trajectories[(nrow(age30Trajectories) + 1):nrow(age30TrajectoriesTasart), ] = NA
age30Trajectories$RecordTravel = age30TrajectoriesTasart$RecordTravel
age30Trajectories$SimulatedAnnealing = age30TrajectoriesTasart$SimulatedAnnealing
age30Trajectories$ThresholdAccepting = age30TrajectoriesTasart$ThresholdAccepting

age30Trajectories = melt(age30Trajectories, id.vars = c("iteration"), na.rm = TRUE)
colnames(age30Trajectories) = c("iteration", "heuristic", "objective")
age30Trajectories$move = age30Trajectories$iteration
geneticIndices = which(age30Trajectories$heuristic == "Genetic")
age30Trajectories$move[geneticIndices] = 2 * 20 * age30Trajectories$iteration[geneticIndices]
levels(age30Trajectories$heuristic) = c(levels(age30Trajectories$heuristic), "Tabu", "Hero")

heroTrajectory = read.csv(file.path(getwd(), "project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntryHero_objective.csv"))
heroIndices = which(is.na(heroTrajectory[, 4]) == FALSE)
heroTrajectory = data.frame(iteration = heroTrajectory[heroIndices, 1], heuristic = rep("Hero", length(heroIndices)), objective = heroTrajectory[heroIndices, 4])
heroTrajectory$move = n * heroTrajectory$iteration
tabuTrajectory = read.csv(file.path(getwd(), "project\\NpvSingleEntryIII\\FE640_project_665npvSingleEntryTabu_objective.csv"))
tabuIndices = which(is.na(tabuTrajectory[, 4]) == FALSE)
tabuTrajectory = data.frame(iteration = tabuTrajectory[tabuIndices, 1], heuristic = rep("Tabu", length(tabuIndices)), objective = tabuTrajectory[tabuIndices, 4])
tabuTrajectory$move = n * tabuTrajectory$iteration

age30Trajectories = rbind(age30Trajectories, tabuTrajectory, heroTrajectory)
age30geneticTrajectory = subset(age30Trajectories, heuristic == "Genetic")
age30heroTrajectory = subset(age30Trajectories, heuristic == "Hero")
age30tabuTrajectory = subset(age30Trajectories, heuristic == "Tabu")
ggplot(age30Trajectories, aes(x = move + 1, y = objective, color = heuristic)) + geom_line(aes(group = heuristic)) +
  geom_point(data = age30geneticTrajectory, aes(x = move + 1, y = objective, color = "Genetic"), size = 1.5) +
  geom_point(data = age30heroTrajectory, aes(x = move + 1, y = objective, color = "Hero"), size = 1.5) +
  geom_point(data = age30tabuTrajectory, aes(x = move + 1, y = objective, color = "Tabu"), size = 0.75) +
  scale_color_viridis_d(labels = heuristicNames) + scale_x_continuous(labels = comma, trans = "log10") + 
  labs(color = NULL) + xlab("move") + ylab("NPV for thin at age 30, $1000/acre") + 
  theme(axis.title.x = element_text(margin = margin(2, 0, 0, 0)), axis.title.y = element_text(margin = margin(0, 2, 0, 0)), legend.margin = margin(10, 10, 10, -40))
ggsave(device = "png", path = file.path(getwd(), "project\\figures"), filename = "trajectory3.png", dpi = "print", width = 4.5, height = 4, units = "in")
