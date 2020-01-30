options(scipen = 99)

# simulated annealing
saDistribution = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set1_20_sa_objectiveDistribution.csv"), header = FALSE)
saObjective = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set1_20_sa_objective.csv"))

png("saObjective.png", width = 3.55, height = 3.5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 1))
plot(saObjective$iteration, 1E-6 * saObjective$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 100000), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(0, 5), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(saObjective$iteration, 1E-6 * saObjective$SA0, log = "y", type = "l", xaxt = "n", xlab = "simulated annealing iterations", xlim = c(0, 100000), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-1, 1), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()

# threshold accepting
taDistribution = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set1_20_ta_objectiveDistribution.csv"), header = FALSE)
taObjective = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set1_20_ta_objective.csv"))

png("taObjective.png", width = 3.55, height = 3.5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 1))
plot(taObjective$iteration, 1E-6 * taObjective$SA0, log = "y", type = "l", xaxt = "n", xlab = "", yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-1, 5), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(taObjective$iteration, 1E-6 * taObjective$SA0, log = "y", type = "l", xaxt = "n", xlab = "threshold accepting iterations", yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-1, 1), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()

# comparison
distribution = cbind(saDistribution, taDistribution)
colnames(distribution) = c("SA", "TA")
colMeans(distribution) / 1E6
sqrt(diag(cov(distribution)) / (100 - 1)) / 1E6

breaks = seq(0, 1.5, length.out = 40)
png("objectiveDistribution.png", width = 6.5, height = 3.5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(1.7, 0.7, 0), mfrow = c(1, 2))
hist(saDistribution$V1 / 1E6, breaks = breaks, main = "", xlab = expression("objective, Mu"^2), xlim = c(0, 1.5), ylab = "simulated annealing runs", ylim = c(0, 80))
hist(taDistribution$V1 / 1E6, breaks = breaks, main = "", xlab = expression("objective, Mu"^2), xlim = c(0, 1.5), ylab = "threshold accepting runs", ylim = c(0, 80))
dev.off()
