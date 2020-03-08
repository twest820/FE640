## objective function distributions
distributionP100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set6_20_ac100hwp_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionP100 = sum(c(2.86, 2.20, 1.69, 1.30, 1.00)) * distributionP100
distributionMM100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set6_20_ac100hwmmas_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
distributionTmm100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set6_20_ac100tmm_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
round(c(p100 = min(distributionP100$V1), max(distributionP100$V1), mm100 = min(distributionMM100$V1), max(distributionMM100$V1), tmm100 = min(distributionTmm100$V1), max(distributionTmm100$V1)) / 1E6, 3)

distributionTmm2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set6_20_ac2ktmm_objectiveDistribution.csv"), fileEncoding = "UTF-8-BOM", header = FALSE)
round(c(tmm2k = min(distributionTmm2k$V1), max(distributionTmm2k$V1)) / 1E6, 3)

breaks100 = seq(0.0, 6.4, length.out = 500)
breaks2k = seq(0.3, 5, length.out = 100)
png("objectiveDistribution.png", width = 6.5, height = 5, units = "in", res = 300)
par(mar = c(3, 3, 1, 1), mgp = c(2, 0.6, 0), mfrow = c(2, 2))
hist(distributionP100$V1 / 1E6, breaks = breaks100, main = "", xlab = "", xlim = c(0, 8), ylab = "proportional, 100 units", ylim = c(0, 80))
hist(distributionMM100$V1 / 1E6, breaks = breaks100, main = "", xlab = "", xlim = c(0, 8), ylab = "max-min, 100 units", ylim = c(0, 80))
hist(distributionTmm100$V1 / 1E6, breaks = breaks100, main = "", xlab = expression("objective, Mu"^2), xlim = c(0, 8), ylab = "transposing max-min, 100 units", ylim = c(0, 80))
hist(distributionTmm2k$V1 / 1E6, breaks = breaks2k, main = "", xlab = expression("objective, Mu"^2), xlim = c(0, 8), ylab = "transposing max-min, 2000 units", ylim = c(0, 80))
dev.off()

# best objective function trajectories
objectivesP100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set6_20_ac100hwp_objective.csv"), fileEncoding = "UTF-8-BOM")
objectivesP100$SA0 = sum(c(2.86, 2.20, 1.69, 1.30, 1.00)) * objectivesP100$SA0
objectivesMM100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set6_20_ac100hwmmas_objective.csv"), fileEncoding = "UTF-8-BOM")
objectivesTmm100 = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set6_20_ac100tmm_objective.csv"), fileEncoding = "UTF-8-BOM")
objectivesTmm2k = read.csv(file.path(getwd(), "UnitTests\\bin\\x64\\Release\\FE640_set6_20_ac2ktmm_objective.csv"), fileEncoding = "UTF-8-BOM")

objectives = rbind(c(p100 = max(objectivesP100$SA0), mm100 = max(objectivesMM100$SA0), tmm100 = max(objectivesTmm100$SA0), ps2k = max(objectivesTmm2k$SA0)),
                   c(min(objectivesP100$SA0), min(objectivesMM100$SA0), min(objectivesTmm100$SA0), min(objectivesTmm2k$SA0)))
log10(objectives / 1E6)
objectives / 1E6
objectives[, 2] / 1E3

png("objectives100.png", width = 6.5, height = 4, units = "in", res = 300)
par(mar = c(3, 3, 1.3, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 2))
plot(objectivesP100$iteration, 1E-6 * objectivesP100$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 500), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = c(0.1, 100), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectivesMM100$iteration, 1E-6 * objectivesMM100$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 250), yaxt = "n", ylab = "", ylim = c(0.1, 100), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))

plot(objectivesP100$iteration, 1E-6 * objectivesP100$SA0, type = "l", xaxt = "n", xlab = "proportional iteration, 50 ants", xlim = c(0, 500), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = c(0.1, 1), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectivesMM100$iteration, 1E-6 * objectivesMM100$SA0, type = "l", xaxt = "n", xlab = "max-min iteration, 100 ants", xlim = c(0, 250), yaxt = "n", ylab = "", ylim = c(0.1, 1), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()

png("objectivesTmm.png", width = 6.5, height = 4, units = "in", res = 300)
par(mar = c(3, 3, 1.3, 1), mgp = c(1.7, 0.7, 0), mfrow = c(2, 2))
plot(objectivesTmm100$iteration, 1E-6 * objectivesTmm100$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 250), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = c(0.1, 1000), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectivesTmm2k$iteration, 1E-6 * objectivesTmm2k$SA0, log = "y", type = "l", xaxt = "n", xlab = "", xlim = c(0, 250), yaxt = "n", ylab = "", ylim = c(0.1, 1000), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))

plot(objectivesTmm100$iteration, 1E-6 * objectivesTmm100$SA0, type = "l", xaxt = "n", xlab = "100 unit iteration, 50 ants", xlim = c(0, 250), yaxt = "n", ylab = expression("objective, Mu"^2), ylim = c(0.1, 1), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
plot(objectivesTmm2k$iteration, 1E-6 * objectivesTmm2k$SA0, type = "l", xaxt = "n", xlab = "2000 unit iteration, 50 ants", xlim = c(0, 250), yaxt = "n", ylab = "", ylim = c(0.1, 1), panel.first = grid())
axis(side = 1, at = axTicks(1), labels = formatC(axTicks(1), format = "d", big.mark = ','))
axis(side = 2, at = axTicks(2), labels = formatC(axTicks(2), format = "f", drop0trailing = TRUE))
dev.off()
