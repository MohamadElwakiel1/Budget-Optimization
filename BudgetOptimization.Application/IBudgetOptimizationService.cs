namespace BudgetOptimization.Application.Interfaces
{
    public interface IBudgetOptimizationService
    {
        double[] OptimizeBudget(
            double[] revenueInputs,
            double[] allocationInputs,
            bool prioritizeDebt,
            double? debtTarget,
            bool allowNewDebt
        );
    }
}
