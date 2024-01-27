using Microsoft.EntityFrameworkCore;

using Serilog;

using WineMate.Reviews.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
});

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
    configuration.WriteTo.Console();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.Run();
