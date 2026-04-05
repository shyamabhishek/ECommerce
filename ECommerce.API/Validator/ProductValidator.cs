using FluentValidation;
using ECommerce.API.DTOs;
namespace ECommerce.API.Validator
{
    public class ProductValidator : AbstractValidator<ProductDto>
    {
        public ProductValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is Required");
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price Must be greater than 0");
            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity can not be negative");
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required");
        }
    }
}
