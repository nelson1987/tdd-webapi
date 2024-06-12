using Chronos.Api.Controllers;
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
using System.Text;
using Testcontainers.SqlEdge;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Chronos.Tests;

[Trait("Category", "IntegrationTests")]
public class CreateAgreementIntegrationTests : IntegrationTests, IClassFixture<ApplicationTestFixture>
{
    private readonly ApplicationTestFixture _fixture;
    private readonly HttpClient _client;
    public CreateAgreementIntegrationTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task TestingGetRedisCache()
    {
        //Arrange
        var client = _fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.RemoveAll<IHttpValidationClientFactory>();
                services.RemoveAll<IMovimentacaoRepository>();
                services.RemoveAll<ICreateAgreementPublisher>();
                services.AddSingleton<IHttpValidationClientFactory, FakeHttpValidationClientFactory>();
                services.AddSingleton<IMovimentacaoRepository, FakeMovimentacaoRepository>();
                services.AddSingleton<ICreateAgreementPublisher, FakeCreateAgreementPublisher>();
            });
        }).CreateClient();
        var command = new CreateAgreementCommand("email@site.com",10.00M);
        StringContent content = new StringContent(System.Text.Json.JsonSerializer.Serialize(command),
            Encoding.UTF8, "application/json");
        //Act
        var result = await _client.PostAsync("CreateAgreement", content);
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(201, (int)result.StatusCode);
        //var resultJson = await result.Content.ReadAsStringAsync();
        //string[] response = System.Text.Json.JsonSerializer.Deserialize<string[]>(resultJson,
        //    new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;
        ////Assert
        //Assert.True(result.IsSuccessStatusCode);
        //Assert.Equal(response, produto);
        //Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}
public class FakeHttpValidationClientFactory : IHttpValidationClientFactory, IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient() { BaseAddress = new Uri($"http://localhost:{IntegrationTests.PortServer}") };
    }

    public Task<bool> Validar(string contaDebitante)
    {
        throw new NotImplementedException();
    }
}

public class FakeMovimentacaoRepository : IMovimentacaoRepository
{
    public Task Incluir(string contaDebitante, decimal valor)
    {
        throw new NotImplementedException();
    }
}
public class FakeCreateAgreementPublisher : ICreateAgreementPublisher
{
    public Task Enviar(CreateAgreementEvent evento)
    {
        throw new NotImplementedException();
    }
}