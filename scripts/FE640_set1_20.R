# simulated annealing
saObjective445k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Debug\\FE640_set1_20_sa_objective.csv"))

png("saObjective.png", width = 5.5, height = 3.8, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 1))
plot(saObjective445k$iteration, 1E-9 * saObjective445k$SA0, log = "y", type = "l", xaxt = "n", xlab = "", yaxt = "n", ylab = expression("objective, Gu"^2), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(saObjective445k$iteration, 1E-9 * saObjective445k$SA0, log = "y", type = "l", xaxt = "n", xlab = "simulated annealing iterations", yaxt = "n", ylab = expression("objective, Gu"^2), ylim = 10^c(-0.9, -0.87), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()
