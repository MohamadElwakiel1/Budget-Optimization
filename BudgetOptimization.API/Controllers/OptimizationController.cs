using Microsoft.AspNetCore.Mvc;
using BudgetOptimization.Application.Interfaces;
using BudgetOptimization.Domain.Entities;

namespace BudgetOptimization.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OptimizationController : ControllerBase
    {
        private readonly IOnnxModelService _onnxService;
        private readonly IBudgetOptimizationService _budgetOptimizationService;
        public OptimizationController(IOnnxModelService onnxService, IBudgetOptimizationService budgetOptimizationService)
        {
            _onnxService = onnxService;
            _budgetOptimizationService = budgetOptimizationService;
        }

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
}
