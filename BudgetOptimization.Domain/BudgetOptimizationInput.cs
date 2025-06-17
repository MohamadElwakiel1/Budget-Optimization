namespace BudgetOptimization.Domain.Entities
{
    public class BudgetOptimizationInput
    {
        public double IncomeTax { get; set; }
        public double GoodsServicesTax { get; set; }
        public double InternationalTradeTax { get; set; }
        public double NonTaxRevenue { get; set; }
        public double Education { get; set; }
        public double Healthcare { get; set; }
        public double Infrastructure { get; set; }
        public double SocialPrograms { get; set; }
        public double Defense { get; set; }
        public bool PrioritizeDebtReduction { get; set; }
        public double? DebtReductionTarget { get; set; }
        public bool AllowNewDebt { get; set; }
    }
}
