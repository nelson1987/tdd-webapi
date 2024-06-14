using Chronos.Api.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text;

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
        //var client = _fixture.WithWebHostBuilder(b =>
        //{
        //    b.ConfigureTestServices(services =>
        //    {
        //        services.RemoveAll<IHttpValidationClientFactory>();
        //        services.RemoveAll<IMovimentacaoRepository>();
        //        services.RemoveAll<ICreateAgreementPublisher>();
        //        services.AddSingleton<IHttpValidationClientFactory, FakeHttpValidationClientFactory>();
        //        services.AddSingleton<IMovimentacaoRepository, FakeMovimentacaoRepository>();
        //        services.AddSingleton<ICreateAgreementPublisher, FakeCreateAgreementPublisher>();
        //    });
        //}).CreateClient();
        var command = new CreateAgreementCommand("email@site.com",10.00M);
        StringContent content = new StringContent(System.Text.Json.JsonSerializer.Serialize(command),
            Encoding.UTF8, "application/json");
        //Act
        var result = await _client.PostAsync("CreateAgreements", content);
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(201, (int)result.StatusCode);
    }
}
public class FakeHttpValidationClientFactory : IValidationTitlesHttpClientFactory, IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient() { BaseAddress = new Uri($"http://localhost:{IntegrationTests.PortServer}") };
    }

    public Task<bool> Validar(string contaDebitante, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}

public class FakeMovimentacaoRepository : IMovimentacaoRepository
{
    public Task Incluir(string contaDebitante, decimal valor, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
public class FakeCreateAgreementPublisher : ICreateAgreementPublisher
{
    public Task Enviar(CreateAgreementEvent evento, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}