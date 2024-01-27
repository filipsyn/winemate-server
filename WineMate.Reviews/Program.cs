using System.Text.Json.Serialization;

using Carter;

using FluentValidation;

using HealthChecks.UI.Client;

using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using Serilog;

using WineMate.Reviews.Database;
using WineMate.Reviews.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
});

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
    configuration.WriteTo.Console();
});

var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
});

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddCarter();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddFluentValidationRulesToSwagger();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "WineMate.Reviews",
            Version = "v1",
            Description = "Service for managing wine reviews.",
            Contact = new OpenApiContact
            {
                Url = new Uri("https://github.com/filipsyn/winemate-server")
            }
        });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database") ??
               throw new InvalidOperationException("Database connection string is not configured."));


builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
);


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapHealthChecks("_health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapCarter();

app.Run();
