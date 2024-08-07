using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using Test.Backend.Abstractions.Costants;
using Test.Backend.Abstractions.Extensions;
using Test.Backend.Dependencies.Utils;
using Test.Backend.Kafka.Configurations;
using Test.Backend.Services.AddressService.Configurations;
using Test.Backend.Services.AddressService.DatabaseContext.Configurations;
using Test.Backend.Services.AddressService.HostedService;
using Test.Backend.Services.AddressService.Interfaces;
using Test.Backend.Services.AddressService.Services;

var builder = WebApplication.CreateBuilder(args);

//Get Api versions
var apiVersions = ApiVersionHelper.GetApiVersions(Assembly.GetExecutingAssembly());

//Configure Kafka Service and Options
builder.Services.AddEventBusService(builder.Configuration);

//Configure Services
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IAddressDbContext, AddressDbContext>();

//Configure Handlers
builder.Services.ConfigureEventHandlerMsgBus();

//Kafka Consumer Service
builder.Services.AddHostedService<KafkaConsumerService>();

//Add DbContext
builder.Services.AddDbContext<AddressDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString(ConnectionStrings.SqlConnection)!);
});

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

//Add Api versioning using namespace convention
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

//Apply Db migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AddressDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

//Handle Errors
app.UseErrorHandlingMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();
