using AspireApp.ApiService.Features;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.AddSqlServerDbContext<YourDbContext>("sqldb");

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<HelloWorldMessageConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var host = configuration.GetConnectionString("RabbitMQConnection");
        cfg.Host(host);
        cfg.ConfigureEndpoints(context);
    });
});

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (IBus bus,
            YourDbContext context, 
            CancellationToken cancellationToken) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    var product = new Product() { FirstName = "Frist", isActive = true, LastName = "Last", Price = 10.0M };
    context.Products.Add(product);
    await bus.Publish(forecast.Select(x => new WeatherForecastEvent(x.Date, x.TemperatureC, x.Summary)).FirstOrDefault()!);
    context.SaveChanges();
    return forecast;
});

app.MapDefaultEndpoints();

app.Run();