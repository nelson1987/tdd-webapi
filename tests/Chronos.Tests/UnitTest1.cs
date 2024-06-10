using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Net;
using Testcontainers.SqlEdge;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Chronos.Tests;
public class IntegrationTests : IAsyncLifetime
{
    public static int PortServer = 9080;
    public required WireMockServer Server;

    public Task InitializeAsync()
    {
        Server = WireMockServer.Start(PortServer);
        HelloWorldEndpoint();
        ValidacaoTransferenciaEndpoint();
        return Task.CompletedTask;
    }

    private void HelloWorldEndpoint()
    {
        Server.Given(
            Request.Create().WithPath("/hello-world").UsingGet()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/plain")
                .WithBody("Hello, world!")
        );
    }

    private void ValidacaoTransferenciaEndpoint()
    {
        var response = new { IsValid = true };
        Server.Given(
            Request.Create().WithPath("/validation").UsingGet()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(System.Text.Json.JsonSerializer.Serialize(response))
        );
    }

    public Task DisposeAsync()
    {
        Server.Stop();
        return Task.CompletedTask;
    }
}

[Trait("Category", "IntegrationTests")]
public class HttpClientFactoryIntegrationTests : IntegrationTests, IClassFixture<ApplicationTestFixture>
{
    private readonly ApplicationTestFixture _fixture;
    private readonly HttpClient _client;
    public HttpClientFactoryIntegrationTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task TestingGetRequest()
    {
        //Act
        var result = await _client.GetAsync("Agreements");
        //Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task TestingGetRequest_WithSeparateBehavioursForEachClient()
    {
        //Arrange
        var client = _fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.RemoveAll<IHttpClientFactory>();
                services.AddSingleton<IHttpClientFactory, FakeHttpClientFactoryWhereTypedClientTwoWillFail>();
            });
        }).CreateClient();
        //Act
        var result = await client.GetAsync("Agreements");
        //Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task TestingGetRequest_WithSeparateBehavioursForEachClient_()
    {
        //Arrange
        var client = _fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.RemoveAll<IHttpClientFactory>();
                services.AddSingleton<IHttpClientFactory, FakeHttpClientFactoryWhereTypedClientTwoWillFail>();
            });
        }).CreateClient();
        //Act
        var result = await client.GetAsync("NotFoundController");
        //Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
}
public class MyTypedClientIntegrationTests : IntegrationTests, IClassFixture<ApplicationTestFixture>
{
    private readonly ApplicationTestFixture _fixture;
    private readonly MyTypedClient _client;
    public MyTypedClientIntegrationTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
        _client = new MyTypedClient(new HttpClient() { BaseAddress = new Uri($"http://localhost:{IntegrationTests.PortServer}") });
    }

    [Fact]
    public async Task TestingGetRequest()
    {
        //Act
        var response = await _client.TakeIsValid();
        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //Assert.Equal("text/plain", response.Headers);
        //Assert.Equal("Hello, world!", response.Content);
    }

}
public class ApplicationTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqlEdgeContainer _dbContainer =
        new SqlEdgeBuilder()
            .WithImage("mcr.microsoft.com/azure-sql-edge:latest")
            .WithPassword("pass123!")
            .Build();
    public Task InitializeAsync()
    {
        return _dbContainer.StartAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing")
                .ConfigureServices(services =>
                {
                    services.RemoveAll<IHttpClientFactory>();
                    services.AddSingleton<IHttpClientFactory, FakeHttpClientFactory>();
                });
        return base.CreateHost(builder);
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<DatabaseContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });
        });
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _dbContainer.StopAsync();
    }
}
public class FakeHttpClientFactoryWhereTypedClientTwoWillFail : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient() { BaseAddress = new Uri($"http://localhost:{IntegrationTests.PortServer}") };
    }
}
public class FakeHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient() { BaseAddress = new Uri($"http://localhost:{IntegrationTests.PortServer}") };
    }
}
public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Directory.GetCurrentDirectory() + "/../Api/appsettings.json")
            .AddJsonFile(Directory.GetCurrentDirectory() + $"/../Api/appsettings.{environment}.json", true)
            .Build();
        var builder = new DbContextOptionsBuilder<DatabaseContext>();

        var connectionString = configuration.GetConnectionString(DataModelOptions.ConnectionString) ??
                               throw new Exception(
                                   $"{DataModelOptions.ConnectionString} is not found in configuration");

        builder.UseSqlServer(connectionString);

        return new DatabaseContext(builder.Options);
    }
}
public static class DataModelOptions
{
    public static string ConnectionString = "";
}
public class User { }
public class DatabaseContext : DbContext
{
    //private readonly IEventBus? _eventBus;
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
    //public DatabaseContext(DbContextOptions<DatabaseContext> options, IEventBus? eventBus = null) : base(options)
    //{
    //    _eventBus = eventBus;
    //}

    public DbSet<User> Users { get; set; }
    //public DbSet<UserTask> UserTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //UserContext.Build(builder);
        //UserTaskContext.Build(builder);

        //// Refs
        //UserStatusRefContext.Build(builder);
        //TaskStatusRefContext.Build(builder);

        //// Add strongly typed id ef core conversions
        //builder.AddStronglyTypedIdConversions();
    }

    //public async Task<int> SaveChangesAndCommitAsync(CancellationToken cancellationToken = default,
    //    params IEvent[] events)
    //{
    //    await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
    //    var result = await SaveChangesAsync(cancellationToken);
    //    if (_eventBus is not null)
    //    {
    //        await _eventBus.Commit(events);
    //    }

    //    await transaction.CommitAsync(cancellationToken);

    //    return result;
    //}
}
//public class FakeHandler : DelegatingHandler
//{
//    private readonly HttpResponseMessage _response;

//    public FakeHandler(HttpResponseMessage? response = null)
//    {
//        _response = response ?? new HttpResponseMessage(HttpStatusCode.OK);
//    }

//    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//    {
//        return Task.FromResult(_response);
//    }
//}
/*
internal sealed class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly LoggerExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(_testOutputHelper, _scopeProvider, categoryName);
    }

    public void Dispose()
    {
    }
}
internal sealed class XUnitLogger<T> : XUnitLogger, ILogger<T>
{
    public XUnitLogger(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider)
            : base(testOutputHelper, scopeProvider, typeof(T).FullName)
    {
    }
}

internal class XUnitLogger : ILogger
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _categoryName;
    private readonly LoggerExternalScopeProvider _scopeProvider;

    public static ILogger CreateLogger(ITestOutputHelper testOutputHelper) => new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), "");
    public static ILogger<T> CreateLogger<T>(ITestOutputHelper testOutputHelper) => new XUnitLogger<T>(testOutputHelper, new LoggerExternalScopeProvider());

    public XUnitLogger(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider, string categoryName)
    {
        _testOutputHelper = testOutputHelper;
        _scopeProvider = scopeProvider;
        _categoryName = categoryName;
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public IDisposable BeginScope<TState>(TState state) => _scopeProvider.Push(state);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var sb = new StringBuilder();
        sb.Append(GetLogLevelString(logLevel))
            .Append(" [").Append(_categoryName).Append("] ")
            .Append(formatter(state, exception));

        if (exception != null)
        {
            sb.Append('\n').Append(exception);
        }

        // Append scopes
        _scopeProvider.ForEachScope((scope, state) =>
        {
            state.Append("\n => ");
            state.Append(scope);
        }, sb);

        _testOutputHelper.WriteLine(sb.ToString());
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }
}
*/