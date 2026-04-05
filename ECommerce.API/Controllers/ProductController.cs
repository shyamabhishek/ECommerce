using Microsoft.AspNetCore.Mvc;
using ECommerce.API.Services;
using ECommerce.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            //throw new Exception("Test exception");
            var products = await _service.GetAllAsync();
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can add products
        public async Task<IActionResult> Add(ProductDto productDto)
        {
            //throw new Exception("Test exception");
            await _service.AddAsync(productDto); // ✅ FIXED (async)
            return Ok("Product Added Successfully");
        }
    }
}