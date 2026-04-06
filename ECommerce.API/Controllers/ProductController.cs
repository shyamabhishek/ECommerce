using Microsoft.AspNetCore.Mvc;
using ECommerce.API.Services;
using ECommerce.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using ECommerce.API.Helpers;
using ECommerce.API.Data;
using ECommerce.API.Models; 

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ProductController(AppDbContext context, IProductService productService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductParams productParams)
        {
            
            var query = context.Products
                .OrderBy(p => p.Id)
                .Select(p => new ProductDto
                {
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Description = p.Description 
                })
                .AsQueryable();

            // the query holds ProductDtos, so this line will work perfectly
            var pagedProducts = await PagedList<ProductDto>.CreateAsync(query, productParams.PageNumber, productParams.PageSize);

            Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
            {
                pagedProducts.CurrentPage,
                pagedProducts.TotalPages,
                pagedProducts.PageSize,
                pagedProducts.TotalCount
            }));

            return Ok(pagedProducts);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(ProductDto productDto)
        {
            
            await productService.AddAsync(productDto);
            return Ok("Product Added Successfully");
        }
    }
}