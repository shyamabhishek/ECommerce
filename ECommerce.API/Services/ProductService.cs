using AutoMapper;
using ECommerce.API.DTOs;
using ECommerce.API.Models;
using ECommerce.API.Repositories;

namespace ECommerce.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();
            return _mapper.Map<List<ProductDto>>(products);
            //return products.Select(p => new ProductDto
            //{
            //    Name = p.Name,
            //    Price = p.Price,
            //    Quantity = p.Quantity,
            //    Description = p.Description
            //}).ToList();
        }

        public async Task AddAsync(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            //var product = new Product
            //{
            //    Name = productDto.Name,
            //    Price = productDto.Price,
            //    Quantity = productDto.Quantity,
            //    Description = productDto.Description
            //};

            await _repository.AddAsync(product);
        }
    }
}