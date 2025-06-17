import numpy as np
from pymoo.algorithms.moo.nsga2 import NSGA2
from pymoo.optimize import minimize
from pymoo.core.problem import Problem
from pymoo.operators.sampling.rnd import FloatRandomSampling
from pymoo.operators.crossover.sbx import SBX
from pymoo.operators.mutation.pm import PolynomialMutation
from sklearn.ensemble import RandomForestRegressor
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

# Step 1: Define the optimization problem (as in your script)
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

# Step 2: Generate training data
X = []  # Inputs: random allocations
Y = []  # Outputs: best allocations found by NSGA-II

for _ in range(200):
    # Random initial guess for allocations
    x0 = np.random.uniform(0, 100, size=(1, 5))
    problem = BudgetOptimizationProblem()
    algorithm = NSGA2(
        pop_size=50,
        sampling=FloatRandomSampling(),
        crossover=SBX(prob=0.9, eta=15),
        mutation=PolynomialMutation(eta=20),
        eliminate_duplicates=True
    )
    res = minimize(problem, algorithm, ('n_gen', 50), seed=None, verbose=False)
    # Take the first Pareto solution as the 'best' for this input
    best_alloc = res.X[0]
    X.append(x0.flatten())
    Y.append(best_alloc)

X = np.array(X)
Y = np.array(Y)

# Step 3: Train a regression model to predict optimal allocations
model = RandomForestRegressor()
model.fit(X, Y)

# Step 4: Export the model to ONNX
onnx_model = convert_sklearn(model, initial_types=[('input', FloatTensorType([None, 5]))])
with open("budget_optimizer.onnx", "wb") as f:
    f.write(onnx_model.SerializeToString())

print("ONNX model exported as budget_optimizer.onnx")
