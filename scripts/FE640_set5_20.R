options(scipen = 99)

## 100 unit problem
distributionPS100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_ps100_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)

c(max(distributionPS100$V1), max(distributionPS100$V1)) / 1E6
c(min(distributionPS100), max(distributionPS100)) / 1E6

breaks = seq(0.15, 0.35, length.out = 100)
png("objectiveDistribution100.png", width = 6.5, height = 5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(2, 0.6, 0), mfrow = c(2, 3))
hist(distributionPS100$V1 / 1E6, breaks = breaks, main = "", xlab = "", xlim = c(0.15, 0.35), ylab = "particle swarm time step, 100 units", ylim = c(0, 100))
dev.off()

## 2000 units
distributionPS2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_ps2k_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)

c(max(distributionPS2k$V1), max(distributionPS2k$V1)) / 1E6
c(min(distributionPS2k$V1), max(distributionPS2k$V1)) / 1E6

breaks = seq(0.3, 0.5, length.out = 100)
png("objectiveDistribution2k.png", width = 6.5, height = 5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(2, 0.6, 0), mfrow = c(2, 3))
hist(distributionPS2k$V1 / 1E6, breaks = breaks, main = "", xlab = "", xlim = c(0.3, 0.5), ylab = "particle swarm time step, 2000 units", ylim = c(0, 100))
dev.off()

# objective functions
objectives100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_ps100_objective.csv"), fileEncoding = "UTF-8-BOM")
objectives2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set5_20_ps2k_objective.csv"), fileEncoding = "UTF-8-BOM")

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
