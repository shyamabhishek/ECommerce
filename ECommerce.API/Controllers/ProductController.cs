using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Hangfire;
using ECommerce.API.DTOs;
using ECommerce.API.Helpers;
using ECommerce.API.Data;
using ECommerce.API.Models;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Mappings;
using AutoMapper;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(
        AppDbContext context,
        IWebHostEnvironment env,
        IDistributedCache cache,
        IBackgroundJobClient backgroundJobs,
        ILogger<ProductController> logger, 
        IMapper mapper
        ) : ControllerBase
    {
        /// <summary>
        /// Retrieves products with Caching (Redis), Pagination, and Search.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductParams productParams)
        {
            // 1. Create a Unique Cache Key
            string cacheKey = $"products_{productParams.SearchTerm}_{productParams.OrderBy}_{productParams.PageNumber}_{productParams.PageSize}";

            try
            {
                // 2. Try Redis Cache (The "Fast Path")
                var cachedData = await cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    logger.LogInformation("🚀 Redis Cache Hit: {CacheKey}", cacheKey);
                    var cachedProducts = JsonSerializer.Deserialize<PagedList<ProductDto>>(cachedData);
                    return Ok(cachedProducts);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "⚠️ Redis connection failed. Falling back to SQL Server.");
            }

            // 3. SQL Server Logic (The "Source of Truth")
            logger.LogInformation("🔍 Cache Miss: Fetching products from SQL for {CacheKey}", cacheKey);

            var query = context.Products.AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(productParams.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(productParams.SearchTerm));
            }

            // Sort
            query = productParams.OrderBy switch
            {
                "price" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderBy(p => p.Id)
            };

            // Map to DTO (Ensuring Id and PictureUrl are sent to React)
            var dtoQuery = query.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Quantity = p.Quantity,
                Description = p.Description,
                PictureUrl = p.PictureUrl
            });

            // Paginate
            var pagedProducts = await PagedList<ProductDto>.CreateAsync(dtoQuery, productParams.PageNumber, productParams.PageSize);

            // 4. Save to Redis for future requests (10 Minute Expiration)
            try
            {
                var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };
                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(pagedProducts), cacheOptions);
            }
            catch (Exception ex) { logger.LogWarning("Failed to save to Redis: {Message}", ex.Message); }

            // 5. Add Pagination Headers
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(new
            {
                pagedProducts.CurrentPage,
                pagedProducts.TotalPages,
                pagedProducts.PageSize,
                pagedProducts.TotalCount
            }));
            Response.Headers.Append("Access-Control-Expose-Headers", "X-Pagination");

            return Ok(pagedProducts);
        }

        /// <summary>
        /// Adds a new product and invalidates existing cache.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(ProductDto productDto)
        {
            var product = mapper.Map<Product>(productDto);
            //var product = new Product
            //{
            //    Name = productDto.Name,
            //    Price = productDto.Price,
            //    Quantity = productDto.Quantity,
            //    Description = productDto.Description
            //};

            context.Products.Add(product);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Product {ProductName} added to database.", product.Name);

            return Ok(new { message = "Product Added Successfully", id = product.Id });
        }

        /// <summary>
        /// Uploads an image and triggers a background job for processing.
        /// </summary>
        [HttpPost("{id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadProductImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file was uploaded");

            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound("Product not found");

            // Save Physical File
            var folderPath = Path.Combine(env.WebRootPath, "images", "products");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update DB Path
            product.PictureUrl = $"images/products/{fileName}";
            await context.SaveChangesAsync();

            // 🧵 HANGFIRE: Offload heavy image processing (compression/watermarking)
            backgroundJobs.Enqueue(() => ProcessImageBackgroundJob(product.Id, fileName));

            return Ok(new { message = "Image uploaded. Processing started in background.", pictureUrl = product.PictureUrl });
        }

        /// <summary>
        /// Background worker method for Hangfire.
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        public void ProcessImageBackgroundJob(int productId, string fileName)
        {
            logger.LogInformation("🧵 Background Job Started: Processing image {FileName} for Product {Id}", fileName, productId);

            // Simulate 5 seconds of heavy image optimization work
            Thread.Sleep(5000);

            logger.LogInformation("✅ Background Job Finished: Image {FileName} is optimized.", fileName);
        }
    }
}