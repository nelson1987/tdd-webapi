using Chronos.Api.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text;

namespace Chronos.Tests;

[Trait("Category", "IntegrationTests")]
public class TitlesIntegrationTests : IntegrationTests, IClassFixture<ApplicationTestFixture>
{
    private readonly ApplicationTestFixture _fixture;
    private readonly HttpClient _client;
    private readonly string _uri;
    public TitlesIntegrationTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.AddSingleton<ITitlesRepository, TitlesRepository>();
            });
        }).CreateClient();
        _uri = "api/v1/Titles";
    }

    [Fact]
    public async Task Request_Valido_Lança_Exception_Retorna_Global_Exception_Handler()
    {
        var exceptiuoClient = _fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.RemoveAll<ITitlesRepository>();
                services.AddSingleton<ITitlesRepository, FakeTitlesRepositoryWithException>();
            });
        }).CreateClient();
        //Act
        var result = await exceptiuoClient.GetAsync($"{_uri}");
        //Assert
        Assert.Equal(500, (int)result.StatusCode);
    }

    [Fact]
    public async Task Get_Request_Valido_Retorna_Ok()
    {
        //Act
        var result = await _client.GetAsync($"{_uri}");
        //Assert
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(200, (int)result.StatusCode);
    }

    [Fact]
    public async Task GetById_Request_Valido_Retorna_Ok()
    {
        //Arrange
        var id = Guid.Parse("7c236a06-cd56-495c-bfb5-ded9aa7d93ae");
        //Act
        var result = await _client.GetAsync($"{_uri}/{id}");
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(200, (int)result.StatusCode);
    }

    [Fact]
    public async Task GetById_Request_Invalido_Retorna_Not_Found()
    {
        //Arrange
        var id = Guid.NewGuid();
        //Act
        var result = await _client.GetAsync($"{_uri}/{id}");
        Assert.Equal(404, (int)result.StatusCode);
    }

    [Fact]
    public async Task Post_Request_Valido_Retorna_Created()
    {
        //Arrange
        var query = new TitlesPostCommand("Description", 10.00M);
        StringContent content = new StringContent(System.Text.Json.JsonSerializer.Serialize(query),
            Encoding.UTF8, "application/json");
        //Act
        var result = await _client.PostAsync($"{_uri}", content);
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(204, (int)result.StatusCode);
        var resultGet = await _client.GetAsync($"{_uri}");
        var responseGet = await resultGet.Content.ReadAsStringAsync();
        var response = System.Text.Json.JsonSerializer.Deserialize<List<TitlesQueryResponse>>(responseGet);
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task Put_Request_Valido_Retorna_Sucesso()
    {
        //Arrange
        var id = Guid.Parse("7c236a06-cd56-495c-bfb5-ded9aa7d93ae");
        var novoValor = 1.99M;
        var query = new TitlesPutCommand(id, novoValor);
        StringContent content = new StringContent(System.Text.Json.JsonSerializer.Serialize(query),
            Encoding.UTF8, "application/json");
        //Act
        var result = await _client.PutAsync($"{_uri}", content);
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(204, (int)result.StatusCode);
        //Buscar Por Id
        var resultGet = await _client.GetAsync($"{_uri}/{id}");
        var responseGet = await resultGet.Content.ReadAsStringAsync();
        TitlesQueryResponse response = System.Text.Json.JsonSerializer.Deserialize<TitlesQueryResponse>(responseGet,
                    new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;
        Assert.Equal(200, (int)resultGet.StatusCode);
        Assert.Equal(id, response.Id);
        Assert.Equal(novoValor, response.Value);
    }

    [Fact]
    public async Task Delete_Request_Valido_Retorna_Sucesso()
    {
        //Arrange
        var id = Guid.Parse("7c236a06-cd56-495c-bfb5-ded9aa7d93ae");
        //Act
        var result = await _client.DeleteAsync($"{_uri}/{id}");
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(204, (int)result.StatusCode);
        //Buscar Por Id
        var resultGet = await _client.GetAsync($"{_uri}/{id}");
        Assert.Equal(404, (int)resultGet.StatusCode);
    }
}