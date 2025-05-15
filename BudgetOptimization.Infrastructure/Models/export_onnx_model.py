import numpy as np
import onnx
import onnxruntime as ort
from sklearn.linear_model import LinearRegression
import skl2onnx
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

# Example: Dummy model for budget optimization
# X: [IncomeTax, GoodsServicesTax, InternationalTradeTax, NonTaxRevenue, Education, Healthcare, Infrastructure, SocialPrograms, Defense]
# y: [OptimizedEducation, OptimizedHealthcare, OptimizedInfrastructure, OptimizedSocialPrograms, OptimizedDefense]

# Generate dummy data
np.random.seed(42)
X = np.random.rand(100, 9) * 100
# For demo, just normalize allocations to sum to 100
allocs = X[:, 4:]
allocs = allocs / allocs.sum(axis=1, keepdims=True) * 100

y = allocs

# Train a simple linear regression model
model = LinearRegression()
model.fit(X, y)

# Convert to ONNX
initial_type = [('input', FloatTensorType([None, 9]))]
onnx_model = convert_sklearn(model, initial_types=initial_type)

with open('budget_optimizer.onnx', 'wb') as f:
    f.write(onnx_model.SerializeToString())

print('ONNX model exported as budget_optimizer.onnx')
