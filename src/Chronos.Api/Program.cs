using Chronos.Api.Controllers;
using Chronos.Api.Features;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IMyTypedClient, MyTypedClient>();
builder.Services.AddHttpClient<IHttpValidationClientFactory, HttpValidationClientFactory>();
builder.Services.AddScoped<IMovimentacaoRepository, MovimentacaoRepository>();
builder.Services.AddScoped<ICreateAgreementPublisher, CreateAgreementPublisher>();

builder.Services.AddRedisCache(builder.Configuration);
//builder.Services.AddCache();
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); 
app.UseExceptionHandler();
app.AddRedisHealthCheck();
app.Run();

public partial class Program { }

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server error"
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}


public interface IMyTypedClient
{
    Task MakeRequest();
}
public class MyTypedClient : IMyTypedClient
{
    private readonly HttpClient _httpClient;

    public MyTypedClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task MakeRequest()
    {
        await _httpClient.GetAsync("/hello-world");
    }

    public async Task<HttpResponseMessage> TakeIsValid()
    {
        var validacao = await _httpClient.GetAsync("/validation");
        return validacao;
    }
}