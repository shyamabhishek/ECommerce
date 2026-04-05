using ECommerce.API.Data;
using ECommerce.API.Middleware;
using ECommerce.API.Repositories;
using ECommerce.API.Services;
using ECommerce.API.Validator;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using ECommerce.API.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();
builder.Services.AddAutoMapper(typeof(MappingProfiles));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options=>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["jwt:Issuer"],
            ValidAudience = builder.Configuration["jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();