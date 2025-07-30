using ConwayGameOfLife.Api.Infrastructure;
using ConwayGameOfLife.Core;
using ConwayGameOfLife.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new BooleanArrayJsonConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add API Explorer and Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conway's Game of Life API",
        Version = "v1",
        Description = "A RESTful API for Conway's Game of Life",
        Contact = new OpenApiContact
        {
            Name = "Conway's Game of Life Team",
            Email = "contact@conwaygame.example.com"
        }
    });

    // Include XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add core services
builder.Services.AddCoreServices();

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<ConwayGameOfLife.Api.HealthChecks.DatabaseHealthCheck>("database");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Use custom exception handling middleware in production
    app.UseMiddleware<ConwayGameOfLife.Api.Middleware.ExceptionHandlingMiddleware>();
}

// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Conway's Game of Life API v1");
    options.RoutePrefix = string.Empty; // Set Swagger UI at the root
});

app.UseHttpsRedirection();
app.UseRouting();

// Add rate limiting middleware
app.UseMiddleware<ConwayGameOfLife.Api.Middleware.RateLimitingMiddleware>(
    100, // Maximum 100 requests
    60   // Per 60 seconds
);

app.UseAuthorization();

app.MapControllers();

// Map health checks with detailed response
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        };
        
        await System.Text.Json.JsonSerializer.SerializeAsync(
            context.Response.Body, 
            response, 
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
    }
});

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ConwayGameOfLife.Infrastructure.Data.AppDbContext>();
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database");
    }
}

app.Run();

// Make the Program class public for integration testing
public partial class Program { }
