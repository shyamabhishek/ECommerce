using ECommerce.API.Controllers;
using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Models;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Tests
{
    public class ProductControllerTests
    {
        
        //helper method to give us a fresh, empty RAM database for every single test
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
        //Helper method to set up the controller with mocked dependencies
        private ProductController CreateController(AppDbContext context, Mock<IBackgroundJobClient> mockHangfire=null)

        {
            var mockCache = new Mock<IDistributedCache>();
            var mockEnv = new Mock<IWebHostEnvironment>();
            var mockLogger = new Mock<ILogger<ProductController>>();
            mockHangfire ??= new Mock<IBackgroundJobClient>();
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
            return controller;
        }
        [Fact]
        public async Task GetProducts_ReturnsCorrectData_WhenSearching()
        {
            // 1. ARRANGE
            var context = GetInMemoryDbContext();
            context.Products.Add(new Product { Id = 1, Name = "Gaming Laptop", Price = 1500 });
            context.Products.Add(new Product { Id = 2, Name = "Office Mouse", Price = 25 });
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var searchParams = new ProductParams { SearchTerm = "Laptop" };

            // 2. ACT
            var result = await controller.GetProducts(searchParams);

            // 3. ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);

            // We use 'dynamic' here so we can read properties off the PagedList easily
            var returnedData = okResult.Value as dynamic;

            Assert.NotNull(returnedData);
            Assert.Equal(1, returnedData.TotalCount); // It should only find the Laptop!
        }

        [Fact]
        public async Task UploadProductImage_SchedulesHangfireBackgroundJob()
        {
            // 1. ARRANGE
            var context = GetInMemoryDbContext();
            context.Products.Add(new Product { Id = 99, Name = "Test Item", Price = 10 });
            await context.SaveChangesAsync();

            var mockHangfire = new Mock<IBackgroundJobClient>();
            var controller = CreateController(context, mockHangfire);

            // Create a fake file to upload
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("test.png");

            // 2. ACT
            var result = await controller.UploadProductImage(99, mockFile.Object);

            // 3. ASSERT
            Assert.IsType<OkObjectResult>(result);

            // Verify that Hangfire was successfully told to process the image in the background
            mockHangfire.Verify(x => x.Create(
                It.IsAny<Hangfire.Common.Job>(),
                It.IsAny<Hangfire.States.EnqueuedState>()),
            Times.Once);
        }
    }
}
