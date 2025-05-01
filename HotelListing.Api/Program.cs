using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using HotelListing.Api.Configuration;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Middlewares;
using HotelListing.Api.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Serilog 

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();


builder.Host.UseSerilog();


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddIdentityCore<ApiUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<HotelListingDbContext>();
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0); 
    options.ReportApiVersions = true; 
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
    {
        Title="Hotel Listing API",
        Version="1.0"
    });

    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo()
    {
        Title = "Hotel Listing API",
        Version = "2.0"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
    {
        Description=@"JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.
                     Example:Bearer 1234ghtgf",
        Name="Authorization",
        In=ParameterLocation.Header,
        Type=SecuritySchemeType.ApiKey,
        Scheme="Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
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


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(MapperConfig));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});


var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var desc in provider.ApiVersionDescriptions)
    {
        Console.WriteLine($"Detected API version: {desc.GroupName}");
        options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
    }
});
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
