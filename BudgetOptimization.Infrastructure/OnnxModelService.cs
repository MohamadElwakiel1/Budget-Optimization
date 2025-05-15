using BudgetOptimization.Application.Interfaces;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BudgetOptimization.Infrastructure.Services
{
    public class OnnxModelService : IOnnxModelService
    {
        private readonly InferenceSession _session;

        public OnnxModelService()
        {
            // Adjust the path as needed for your ONNX model
            _session = new InferenceSession("Models/budget_optimizer.onnx");
        }

        public double[] OptimizeBudget(double[] revenueInputs, double[] allocationInputs, bool prioritizeDebt, double? debtTarget, bool allowNewDebt)
        {
            // Example: concatenate all inputs (adjust as per your model's input requirements)
            var inputData = revenueInputs.Concat(allocationInputs).ToArray();
            var inputTensor = new DenseTensor<float>(inputData.Select(d => (float)d).ToArray(), new[] { 1, inputData.Length });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor) // Replace 'input' with your model's input name
            };

            using var results = _session.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToArray();
            return output.Select(o => (double)o).ToArray();
        }
    }
}
