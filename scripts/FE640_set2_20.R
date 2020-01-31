options(scipen = 99)

# great deluge
gdDistribution = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set2_20_gd_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
gdObjective = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set2_20_gd_objective.csv"))

png("gdObjective.png", width = 3.55, height = 3.5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 1))
plot(gdObjective$iteration, 1E-6 * gdObjective$SA0, log = "y", type = "l", xaxt = "n", xlab = "", yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-1, 5), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(gdObjective$iteration, 1E-6 * gdObjective$SA0, log = "y", type = "l", xaxt = "n", xlab = "great deluge iterations", yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-1, 1), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()

# record to record travel
rtDistribution = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Debug\\FE640_set2_20_rt_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
rtObjective = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Debug\\FE640_set2_20_rt_objective.csv"))

png("rtObjective.png", width = 3.55, height = 3.5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 1))
plot(rtObjective$iteration, 1E-6 * rtObjective$SA0, log = "y", type = "l", xaxt = "n", xlab = "", yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-1, 6), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(rtObjective$iteration, 1E-6 * rtObjective$SA0, log = "y", type = "l", xaxt = "n", xlab = "record to record travel iterations", yaxt = "n", ylab = expression("objective, Mu"^2), ylim = 10^c(-1, 1), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()

# comparison
distribution = cbind(gdDistribution, rtDistribution)
c(max(distribution[, 1]), max(distribution[, 2])) / 1E6
colnames(distribution) = c("SA", "TA")
colMeans(distribution) / 1E6
sqrt(diag(cov(distribution)) / (100 - 1)) / 1E6

breaks = seq(0, 0.4, length.out = 40)
png("objectiveDistribution.png", width = 6.5, height = 3.5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(2, 0.6, 0), mfrow = c(1, 2))
hist(gdDistribution$V1 / 1E6, breaks = breaks, main = "", xlab = expression("objective, Mu"^2), xlim = c(0, 0.4), ylab = "great deluge runs", ylim = c(0, 80))
hist(rtDistribution$V1 / 1E6, breaks = breaks, main = "", xlab = expression("objective, Mu"^2), xlim = c(0, 0.4), ylab = "record to record travel runs", ylim = c(0, 80))
dev.off()
