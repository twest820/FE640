options(scipen = 99)

## objective function distributions
distributionHS100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_hs100_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionPS100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_ps100_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
c(min(distributionHS100$V1), max(distributionHS100$V1), min(distributionPS100$V1), max(distributionPS100$V1)) / 1E6

distributionHS2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_hs2k_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionPS2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_ps2k_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
c(min(distributionHS2k$V1), max(distributionHS2k$V1), min(distributionPS2k$V1), max(distributionPS2k$V1)) / 1E6

breaksHS100 = seq(0.15, 1.1, length.out = 100)
breaksPS100 = seq(0.15, 0.25, length.out = 250)
breaksHS2k = seq(0.15, 1, length.out = 100)
breaksPS2k = seq(0, 31E3, length.out = 1000)
png("objectiveDistribution.png", width = 6.5, height = 5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(2, 0.6, 0), mfrow = c(2, 2))
hist(distributionHS100$V1 / 1E6, breaks = breaksHS100, main = "", xlab = "", xlim = c(0, 1.2), ylab = "harmony search, 100 units", ylim = c(0, 15))
hist(distributionHS2k$V1 / 1E6, breaks = breaksHS2k, main = "", xlab = "", xlim = c(0, 1.2), ylab = "harmony search, 2000 units", ylim = c(0, 15))
hist(distributionPS100$V1 / 1E6, breaks = breaksPS100, main = "", xlab = expression("objective, Mu"^2), xlim = c(0, 1.2), ylab = "particle swarm, 100 units", ylim = c(0, 15))
hist(distributionPS2k$V1 / 1E6, breaks = breaksPS2k, main = "", xlab = expression("objective, Mu"^2), xlim = c(0, 10E3), ylab = "particle swarm, 2000 units", ylim = c(0, 15))
dev.off()

# best objective function trajectories
objectivesPS100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_ps100_objective.csv"), fileEncoding = "UTF-8-BOM")
objectivesPS2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_ps2k_objective.csv"), fileEncoding = "UTF-8-BOM")
objectivesHS100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_hs100_objective.csv"), fileEncoding = "UTF-8-BOM")
objectivesHS2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_hs2k_objective.csv"), fileEncoding = "UTF-8-BOM")

objectives = rbind(c(hs100 = max(objectivesHS100$SA0), hs2k = max(objectivesHS2k$SA0), ps100 = max(objectivesPS100$SA0), ps2k = max(objectivesPS2k$SA0)),
                   c(min(objectivesHS100$SA0), min(objectivesHS2k$SA0), min(objectivesPS100$SA0), min(objectivesPS2k$SA0)))
log10(objectives / 1E6)
objectives / 1E6
c(hs100 = max(objectivesHS100$iteration), hs2k = max(objectivesHS2k$iteration), ps100 = max(objectivesPS100$iteration), ps2k = max(objectivesPS2k$iteration)) / 1E3

png("objectivesHS.png", width = 6.5, height = 4, units = "in", res = 300)
par(mar = c(3, 3, 1.3, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 2))
plot(objectivesHS100$iteration, 1E-6 * objectivesHS100$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 10E3), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-1, 2.5), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectivesHS2k$iteration, 1E-6 * objectivesHS2k$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 50E3), yaxt = "n", ylab = "", ylim = 10^c(-1, 6), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))

plot(objectivesHS100$iteration, 1E-6 * objectivesHS100$SA0, type = "l", xaxt = "n", xlab = "iteration, 100-unit problem", xlim = c(0, 10E3), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = c(0.15, 0.2), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectivesHS2k$iteration, 1E-6 * objectivesHS2k$SA0, type = "l", xaxt = "n", xlab = "iteration, 2000-unit problem", xlim = c(0, 50E3), yaxt = "n", ylab = "", ylim = c(0.3, 0.5), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()

png("objectivesPS.png", width = 6.5, height = 4, units = "in", res = 300)
par(mar = c(3, 3, 1.3, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 2))
plot(objectivesPS100$iteration, 1E-6 * objectivesPS100$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 1000), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-0.8, 3), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectivesPS2k$iteration, 1E-6 * objectivesPS2k$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 1000), yaxt = "n", ylab = "", ylim = 10^c(-0.5, 6), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))

plot(objectivesPS100$iteration, 1E-6 * objectivesPS100$SA0, type = "l", xaxt = "n", xlab = "iteration, 100-unit problem", xlim = c(0, 1000), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = c(0.15, 0.2), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectivesPS2k$iteration, 1E-6 * objectivesPS2k$SA0, type = "l", xaxt = "n", xlab = "iteration, 2000-unit problem", xlim = c(0, 1000), yaxt = "n", ylab = "", ylim = c(0.3, 0.5), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()
