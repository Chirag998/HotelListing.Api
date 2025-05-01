using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using HotelListing.Api.Configuration;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Middlewares;
using HotelListing.Api.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Logging: Serilog setup
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Database: EF Core DbContext
var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options =>
{
    options.UseSqlServer(connectionString); // Configuring SQL Server connection
});

// Identity setup: Adding Identity with roles
builder.Services.AddIdentityCore<ApiUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<HotelListingDbContext>();

// API Versioning: Setting up versioning strategy
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true; // Setting default API version
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // API version via URL segment
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger: Adding Swagger docs and security definition
builder.Services.AddSwaggerGen(options =>
{
    // Swagger docs per version
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hotel Listing API",
        Version = "1.0"
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Hotel Listing API",
        Version = "2.0"
    });

    // JWT Bearer Authorization
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                        Enter 'Bearer' [space] and then your token.
                        Example: Bearer 12345abcdef",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Security requirements for Bearer token
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Authorization",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Authentication & JWT Setup: Configuring authentication middleware
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true, // Validates the signing key
        ValidateIssuer = true, // Validates the issuer
        ValidateAudience = true, // Validates the audience
        ValidateLifetime = true, // Validates token expiration
        ClockSkew = TimeSpan.Zero, // No clock skew
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])) // Signing key
    };
});

// Dependency Injection: Configuring repositories and services
builder.Services.AddAutoMapper(typeof(MapperConfig)); // AutoMapper setup
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>)); // Generic Repository
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>(); // Countries Repository
builder.Services.AddScoped<IAuthManager, AuthManager>(); // Auth Manager

// Controllers & API Explorer: Adding controllers and endpoint exploration
builder.Services.AddControllers(); // Adding controllers
builder.Services.AddEndpointsApiExplorer(); // API Explorer for endpoints

// Application Pipeline: Configuring middleware and endpoints
var app = builder.Build();

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

if (app.Environment.IsDevelopment())
{
    // Optional: Add dev-specific middleware/logging here
}

// Exception Handling Middleware: Global exception handling
app.UseMiddleware<ExceptionMiddleware>();

// Swagger UI with versioning: Setting up Swagger UI for different versions
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var desc in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant()); // Swagger endpoint for each version
    }
});

app.UseHttpsRedirection(); // Enforcing HTTPS redirection

app.UseAuthentication(); // Enabling authentication middleware
app.UseAuthorization(); // Enabling authorization middleware

app.MapControllers(); // Mapping controllers to routes

app.Run(); // Running the app
