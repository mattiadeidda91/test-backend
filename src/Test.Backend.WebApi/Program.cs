using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Extensions;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.AddressService.Interfaces;
using Test.Backend.AddressService.Services;
using Test.Backend.Database.DatabaseContext;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Configurations;
using Test.Backend.OrderService.Interfaces;
using Test.Backend.OrderService.Services;
using Test.Backend.ProductService.Interfaces;
using Test.Backend.ProductService.Services;
using Test.Backend.UserService.Interfaces;
using Test.Backend.UserService.Services;

var builder = WebApplication.CreateBuilder(args);

//Get Api versions
var apiVersions = ApiVersionHelper.GetApiVersions(Assembly.GetExecutingAssembly());

//Configure Kafka Service and Options
builder.Services.AddEventBusService(builder.Configuration);

//Add Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
//builder.Services.AddScoped<IApplicationDbContext>(services => services.GetRequiredService<ApplicationDbContext>());

//Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString(ConnectionStrings.SqlConnection)!);
});

//Configure Controllers
builder.Services.BuildControllerConfigurations();

//Add Serilog
builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
});

//Add Automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Api versioning using namespace convention
builder.Services.UseApiVersioningNamespaceConvention();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var version in apiVersions)
        {
            c.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
        }
    });
}

////Apply Db migrations
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    dbContext.Database.Migrate();
//}

//Serilog log all requests
app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
});

app.UseHttpsRedirection();

//Handle Errors
app.UseErrorHandlingMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();
