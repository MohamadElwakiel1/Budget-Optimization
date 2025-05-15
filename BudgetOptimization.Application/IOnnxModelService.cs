namespace BudgetOptimization.Application.Interfaces
{
    public interface IOnnxModelService
    {
        double[] OptimizeBudget(double[] revenueInputs, double[] allocationInputs, bool prioritizeDebt, double? debtTarget, bool allowNewDebt);
    }
}
