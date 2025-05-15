using BudgetOptimization.Application.Interfaces;

namespace BudgetOptimization.Infrastructure.Services
{
    public class BudgetOptimizationService : IBudgetOptimizationService
    {
        public double[] OptimizeBudget(
            double[] revenueInputs,
            double[] allocationInputs,
            bool prioritizeDebt,
            double? debtTarget,
            bool allowNewDebt)
        {
            // Example: Use Binh and Korn function for two objectives
            // Map: x = sum of stable allocations (Education + Healthcare), y = sum of others
            double x = allocationInputs[0] + allocationInputs[1]; // Education + Healthcare
            double y = allocationInputs[2] + allocationInputs[3] + allocationInputs[4]; // Others

            // Binh and Korn objectives
            double f1 = x * x + y * y;
            double f2 = (x - 5) * (x - 5) + (y - 5) * (y - 5);

            // Simple logic: minimize both f1 and f2 (for demo, return allocations that minimize f1)
            // In real use, you would use a multi-objective optimizer
            double total = x + y;
            double[] optimized = new double[5];
            if (total > 0)
            {
                // Allocate proportionally to minimize f1 (favoring stable sources)
                optimized[0] = allocationInputs[0] / total * 100;
                optimized[1] = allocationInputs[1] / total * 100;
                optimized[2] = allocationInputs[2] / total * 100;
                optimized[3] = allocationInputs[3] / total * 100;
                optimized[4] = allocationInputs[4] / total * 100;
            }
            else
            {
                // Default allocation
                for (int i = 0; i < 5; i++) optimized[i] = 20;
            }

            // Optionally, adjust for debt priorities
            if (prioritizeDebt && debtTarget.HasValue)
            {
                // Example: reduce all allocations by debt target percent
                for (int i = 0; i < 5; i++)
                    optimized[i] = optimized[i] * (1 - debtTarget.Value / 100.0);
            }

            return optimized;
        }
    }
}
