options(scipen = 99)

## 100 unit problem
distributionGD100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set4_20_gd100_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionRT100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Debug\\FE640_set4_20_rt100_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionSA100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set4_20_sa100_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionTU100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set4_20_tu100_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionGA100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Debug\\FE640_set4_20_ga100_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)

c(max(distributionGD100$V1), max(distributionRT100$V1), max(distributionSA100$V1), max(distributionTU100$V1), max(distributionGA100$V1)) / 1E6
c(min(distributionGA100), max(distributionGA100)) / 1E6

breaks = seq(0.15, 0.35, length.out = 100)
breaksGA = seq(0.1615, 0.163, length.out = 100)
png("objectiveDistribution100.png", width = 6.5, height = 5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(2, 0.6, 0), mfrow = c(2, 3))
hist(distributionGD100$V1 / 1E6, breaks = breaks, main = "", xlab = "", xlim = c(0.15, 0.35), ylab = "great deluge runs, 100 units", ylim = c(0, 100))
hist(distributionRT100$V1 / 1E6, breaks = breaks, main = "", xlab = "", xlim = c(0.15, 0.35), ylab = "record travel runs, 100 units", ylim = c(0, 100))
hist(distributionGA100$V1 / 1E6, breaks = breaks, main = "", xlab = "", xlim = c(0.15, 0.35), ylab = "genetic runs, 100 units", ylim = c(0, 100))
hist(distributionSA100$V1 / 1E6, breaks = breaks, main = "", xlab = expression("objective, Mu"^2), xlim = c(0.15, 0.35), ylab = "simulated annealing runs, 100 units", ylim = c(0, 100))
hist(distributionTU100$V1 / 1E6, breaks = breaks, main = "", xlab = expression("objective, Mu"^2), xlim = c(0.15, 0.35), ylab = "tabu searches, 100 units", ylim = c(0, 100))
hist(distributionGA100$V1 / 1E6, breaks = breaksGA, main = "", xlab = expression("objective, Mu"^2), ylab = "detail of genetic runs, 100 units", ylim = c(0, 15))
dev.off()

## 2000 units
distributionGD2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set2_20_gd_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionGD2k$V1 = 1.1357 * distributionGD2k$V1 # correct for slightly different great deluge harvest target
distributionRT2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set2_20_rt_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionSA2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set4_20_sa2k_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionTU2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set2_20_tu_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionGA2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set4_20_ga2k_objectiveDistribution2.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)

c(min(distributionGD2k$V1), min(distributionRT2k$V1), min(distributionSA2k$V1), min(distributionTU2k$V1), min(distributionGA2k$V1)) / 1E6
c(max(distributionGD2k$V1), max(distributionRT2k$V1), max(distributionSA2k$V1), max(distributionTU2k$V1), max(distributionGA2k$V1)) / 1E6
c(min(distributionGA2k$V1), max(distributionGA2k$V1)) / 1E6

breaks = seq(0.3, 0.5, length.out = 100)
breaksGA = seq(0.32029, 0.32030, length.out = 100)
png("objectiveDistribution2k.png", width = 6.5, height = 5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(2, 0.6, 0), mfrow = c(2, 3))
hist(distributionGD2k$V1 / 1E6, breaks = breaks, main = "", xlab = "", xlim = c(0.3, 0.5), ylab = "great deluge runs, 2000 units", ylim = c(0, 100))
hist(distributionRT2k$V1 / 1E6, breaks = breaks, main = "", xlab = "", xlim = c(0.3, 0.5), ylab = "record travel runs, 2000 units", ylim = c(0, 100))
hist(distributionGA2k$V1 / 1E6, breaks = breaks, main = "", xlab = "", xlim = c(0.3, 0.5), ylab = "genetic runs, 2000 units", ylim = c(0, 100))
hist(distributionSA2k$V1 / 1E6, breaks = breaks, main = "", xlab = expression("objective, Mu"^2), xlim = c(0.3, 0.5), ylab = "simulated annealing runs, 2000 units", ylim = c(0, 100))
hist(distributionTU2k$V1 / 1E6, breaks = breaks, main = "", xlab = expression("objective, Mu"^2), xlim = c(0.3, 0.5), ylab = "tabu searches, 2000 units", ylim = c(0, 100))
hist(distributionGA2k$V1 / 1E6, breaks = breaksGA, main = "", xlab = expression("objective, Mu"^2), xlim = c(0.32029, 0.32030), ylab = "detail of genetic runs, 2000 units", ylim = c(0, 10))
dev.off()

# objective functions
objectives100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Debug\\FE640_set4_20_ga100_objective.csv"), fileEncoding = "UTF-8-BOM")
objectives2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set4_20_ga2k_objective0.755.csv"), fileEncoding = "UTF-8-BOM")

objectives = rbind(c(max(objectives100$SA0), max(objectives2k$SA0)),
                   c(min(objectives100$SA0), min(objectives2k$SA0)))
log10(objectives / 1E6)

png("objectivesGA.png", width = 6.5, height = 4, units = "in", res = 300)
par(mar = c(3, 3, 1.3, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 2))
plot(objectives100$iteration, 1E-6 * objectives100$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 250), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-0.8, 3), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectives2k$iteration, 1E-6 * objectives2k$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 1000), yaxt = "n", ylab = "", ylim = 10^c(-0.5, 6), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))

plot(objectives100$iteration, 1E-6 * objectives100$SA0, type = "l", xaxt = "n", xlab = "iteration, 100-unit problem", xlim = c(0, 250), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = c(0.160, 0.165), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectives2k$iteration, 1E-6 * objectives2k$SA0, type = "l", xaxt = "n", xlab = "iteration, 2000-unit problem", xlim = c(0, 1000), yaxt = "n", ylab = "", ylim = c(0.320, 0.325), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()
