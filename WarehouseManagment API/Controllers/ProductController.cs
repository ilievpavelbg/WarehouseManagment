using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {

        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("AllProducts")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving products");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while retrieving products");
            }
        }

        [HttpGet("GetProduct/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                    return NotFound(); 

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving product with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error occurred while retrieving product with ID: {id}");
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductModel model)
        {
            try
            {
                if (model == null || !ModelState.IsValid)
                    return BadRequest("Invalid model data");

                var newProduct = await _productService.CreateProductAsync(model, true);

                return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, newProduct);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new product");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while creating a new product");
            }
        }
    }

}