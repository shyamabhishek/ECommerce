using ECommerce.API.Data;
using ECommerce.API.Mappings;
using ECommerce.API.Middleware;
using ECommerce.API.Repositories;
using ECommerce.API.Services;
using ECommerce.API.Validator;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;


//configure serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() //log everything frominformation level and up
    .WriteTo.Console() //log to console
    .WriteTo.File("Logs/ecommerce-log-.txt", rollingInterval: RollingInterval.Day) // Creates a new file every day
    .CreateLogger();
try
{
    Log.Information("Starting ECommerce.API...");
    var builder = WebApplication.CreateBuilder(args);
    //tell the builder to use serilog
    builder.Host.UseSerilog();


    // --- 1. CORE SERVICES ---
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // --- 2. SWAGGER CONFIGURATION (With JWT & XML Comments) ---
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token. \r\n\r\n Example: \"Bearer eyJhbGciOiJIUzI1Ni...\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
        });
    });

    // --- 3. DATABASE & REPOSITORIES ---
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<IOrderService, OrderService>();

    // --- 4. UTILITIES (AutoMapper & FluentValidation) ---
    builder.Services.AddAutoMapper(typeof(MappingProfiles));
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

    // --- 5. AUTHENTICATION (JWT) ---
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration.");
            var key = Encoding.UTF8.GetBytes(jwtKey);

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

    // --- 6. CORS POLICY (Updated for Vite Ports) ---
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithOrigins("http://localhost:5173", "http://localhost:5174");
        });
    });
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "ECommerce_";
    });
    builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddHangfireServer();


    var app = builder.Build();
    app.UseHangfireDashboard(); // Optional: Exposes the Hangfire Dashboard at /hangfire for monitoring background jobs

    // --- 7. MIDDLEWARE PIPELINE (Order Matters!) ---

    // 1st: Global Error Handling catches crashes from ALL other middleware
    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Ensure the API can handle HTTPS
    app.UseHttpsRedirection();

    // Serve static files (Images)
    app.UseStaticFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // This tells the browser: "It's okay to show this image on localhost:5174"
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:5174");
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        }
    });

    app.UseRouting();
    app.UseSerilogRequestLogging(); //logger

    // CORS must be after Routing but before Auth
    app.UseCors("CorsPolicy");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ECommerce.API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush(); // Ensure all logs are flushed before the application exits
}