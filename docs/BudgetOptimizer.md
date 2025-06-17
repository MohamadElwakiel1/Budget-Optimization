# Budget Optimizer Documentation

This document describes the budget optimization components of this repository. It focuses on the .NET projects and the Python script that are used to create and run a budget optimization model.

## Project Structure

- `BudgetOptimization.Domain` – Domain entities representing inputs and outputs.
- `BudgetOptimization.Application` – Interfaces defining services used by the application.
- `BudgetOptimization.Infrastructure` – Implementations of the services, including a basic optimization algorithm and an ONNX runtime service.
- `BudgetOptimization.API` – ASP.NET Core Web API exposing endpoints for budget optimization.
- `BudgetOptimization.Infrastructure/Models/export_nsga2_to_onnx.py` – Python script used to generate an ONNX model using the NSGA-II algorithm.

## Domain Layer

### `BudgetAllocation`
Defines how funds are distributed across different categories.
```csharp
namespace BudgetOptimization.Domain.Entities
{
    public class BudgetAllocation
    {
        public double Education { get; set; }
        public double Healthcare { get; set; }
        public double Infrastructure { get; set; }
        public double SocialPrograms { get; set; }
        public double Defense { get; set; }
    }
}
```
Source: `BudgetOptimization.Domain/BudgetAllocation.cs`【F:BudgetOptimization.Domain/BudgetAllocation.cs†L1-L11】

### `BudgetOptimizationInput`
Represents revenue sources, initial allocations, and debt preferences used for optimization.
```csharp
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
```
Source: `BudgetOptimization.Domain/BudgetOptimizationInput.cs`【F:BudgetOptimization.Domain/BudgetOptimizationInput.cs†L1-L18】

## Application Layer

The application project defines two key interfaces used by the optimizer services.

### `IBudgetOptimizationService`
```csharp
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
```
Source: `BudgetOptimization.Application/IBudgetOptimizationService.cs`【F:BudgetOptimization.Application/IBudgetOptimizationService.cs†L1-L15】

### `IOnnxModelService`
```csharp
public interface IOnnxModelService
{
    double[] OptimizeBudget(double[] revenueInputs, double[] allocationInputs, bool prioritizeDebt, double? debtTarget, bool allowNewDebt);
}
```
Source: `BudgetOptimization.Application/IOnnxModelService.cs`【F:BudgetOptimization.Application/IOnnxModelService.cs†L1-L8】

## Infrastructure Layer

### Basic Optimization Algorithm
`BudgetOptimizationService` implements `IBudgetOptimizationService`. It uses a simple algorithm to illustrate how an optimization could work.
```csharp
public double[] OptimizeBudget(
    double[] revenueInputs,
    double[] allocationInputs,
    bool prioritizeDebt,
    double? debtTarget,
    bool allowNewDebt)
{
    // Example: Use Binh and Korn function for two objectives
    double x = allocationInputs[0] + allocationInputs[1]; // Education + Healthcare
    double y = allocationInputs[2] + allocationInputs[3] + allocationInputs[4]; // Others

    double f1 = x * x + y * y;
    double f2 = (x - 5) * (x - 5) + (y - 5) * (y - 5);

    double total = x + y;
    double[] optimized = new double[5];
    if (total > 0)
    {
        // Allocate proportionally to minimize f1
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

    // Optionally adjust allocations if debt reduction is prioritized
    if (prioritizeDebt && debtTarget.HasValue)
    {
        for (int i = 0; i < 5; i++)
            optimized[i] = optimized[i] * (1 - debtTarget.Value / 100.0);
    }

    return optimized;
}
```
Source: `BudgetOptimization.Infrastructure/BudgetOptimizationService.cs`【F:BudgetOptimization.Infrastructure/BudgetOptimizationService.cs†L1-L51】

### ONNX Model Service
`OnnxModelService` loads a pre-trained model from the `Models` directory and runs inference using ONNX Runtime.
```csharp
public class OnnxModelService : IOnnxModelService
{
    private readonly InferenceSession _session;

    public OnnxModelService()
    {
        _session = new InferenceSession("Models/budget_optimizer.onnx");
    }

    public double[] OptimizeBudget(double[] revenueInputs, double[] allocationInputs, bool prioritizeDebt, double? debtTarget, bool allowNewDebt)
    {
        var inputData = revenueInputs.Concat(allocationInputs).ToArray();
        var inputTensor = new DenseTensor<float>(inputData.Select(d => (float)d).ToArray(), new[] { 1, inputData.Length });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", inputTensor)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();
        return output.Select(o => (double)o).ToArray();
    }
}
```
Source: `BudgetOptimization.Infrastructure/OnnxModelService.cs`【F:BudgetOptimization.Infrastructure/OnnxModelService.cs†L1-L33】

### Generating the ONNX Model
The Python script `export_nsga2_to_onnx.py` trains a surrogate model using the NSGA‑II algorithm and exports it to ONNX.
```python
class BudgetOptimizationProblem(Problem):
    def __init__(self, n_var=5, budget=100, min_bounds=None, max_bounds=None):
        super().__init__(n_var=n_var, n_obj=3, n_constr=1,
                         xl=min_bounds or [0]*n_var,
                         xu=max_bounds or [budget]*n_var)
        self.budget = budget

    def _evaluate(self, x, out, *args, **kwargs):
        x_sum = x.sum(axis=1).reshape(-1, 1)
        x_norm = x / x_sum * self.budget
        obj1 = -(0.30 * x_norm[:, 0] + 0.25 * x_norm[:, 1] +
                 0.20 * x_norm[:, 2] + 0.15 * x_norm[:, 3] +
                 0.10 * x_norm[:, 4])
        obj2 = -(0.7 * x_norm[:, 0] + 0.3 * x_norm[:, 3])
        obj3 = -(0.8 * x_norm[:, 2] + 0.2 * x_norm[:, 4])
        constr = np.abs(x_norm.sum(axis=1) - self.budget).reshape(-1, 1)
        out["F"] = np.column_stack([obj1, obj2, obj3])
        out["G"] = constr
```
Source: `BudgetOptimization.Infrastructure/Models/export_nsga2_to_onnx.py`【F:BudgetOptimization.Infrastructure/Models/export_nsga2_to_onnx.py†L1-L30】
The remainder of the script samples random allocations, runs NSGA‑II, fits a `RandomForestRegressor` to predict the best allocations, and saves the model to `budget_optimizer.onnx`.

## API Layer

`OptimizationController` exposes two endpoints under `api/optimization`.
```csharp
[ApiController]
[Route("api/[controller]")]
public class OptimizationController : ControllerBase
{
    private readonly IOnnxModelService _onnxService;
    private readonly IBudgetOptimizationService _budgetOptimizationService;

    [HttpPost("onnx")]
    public IActionResult OptimizeWithOnnx([FromBody] BudgetAllocation allocation)
    {
        double[] revenue = new double[] { 10, 10, 10, 10 };
        var result = _onnxService.OptimizeBudget(revenue, new double[]
        {
            allocation.Education,
            allocation.Healthcare,
            allocation.Infrastructure,
            allocation.SocialPrograms,
            allocation.Defense
        }, false, null, false);
        return Ok(result);
    }

    [HttpPost("custom")]
    public IActionResult OptimizeWithCustom([FromBody] BudgetAllocation allocation)
    {
        double[] revenue = new double[] { 10, 10, 10, 10 };
        var result = _budgetOptimizationService.OptimizeBudget(revenue, new double[]
        {
            allocation.Education,
            allocation.Healthcare,
            allocation.Infrastructure,
            allocation.SocialPrograms,
            allocation.Defense
        }, false, null, false);
        return Ok(result);
    }
}
```
Source: `BudgetOptimization.API/Controllers/OptimizationController.cs`【F:BudgetOptimization.API/Controllers/OptimizationController.cs†L1-L49】

The API registers both services in `Program.cs` so they are available via dependency injection. Swagger/OpenAPI is enabled for easy testing.

## Usage

1. **Build the solution** using the .NET CLI:
   ```bash
   dotnet build
   ```
2. **Run the API**:
   ```bash
   dotnet run --project BudgetOptimization.API
   ```
   Swagger UI will be available to test the endpoints.
3. **POST** to `/api/optimization/custom` with a JSON body matching `BudgetAllocation` to run the simple optimization service.
4. **POST** to `/api/optimization/onnx` to run the ONNX-based model (after the ONNX model has been generated and placed in `BudgetOptimization.Infrastructure/Models`).

## Notebook
The `BudgetOptimization.ipynb` notebook provides an overview of budget planning concepts and may serve as a starting point for experimentation.

---

This documentation focuses solely on the budget optimizer components. Other files in the repository are placeholders or standard project configuration files.
