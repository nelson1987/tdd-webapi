using Chronos.Api.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Chronos.Tests;

[Trait("Category", "IntegrationTests")]
public class TitlesIntegrationTests : IntegrationTests, IClassFixture<ApplicationTestFixture>
{
    private readonly ApplicationTestFixture _fixture;
    private readonly HttpClient _client;
    public TitlesIntegrationTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.AddSingleton<TitlesRepository>();
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Get_Request_Valido_Retorna_Ok()
    {
        //Act
        var result = await _client.GetAsync("Titles");
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
        var result = await _client.GetAsync($"Titles/{id}");
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
        var result = await _client.GetAsync($"Titles/{id}");
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
        var result = await _client.PostAsync("Titles", content);
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(204, (int)result.StatusCode);
        var resultGet = await _client.GetAsync("Titles");
        var responseGet = await resultGet.Content.ReadAsStringAsync();
        var response = System.Text.Json.JsonSerializer.Deserialize<List<TitlesQueryResponse>>(responseGet);
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task Put_Request_Valido_Retorna_Sucesso()
    {
        //Arrange
        var query = new TitlesPutCommand();
        StringContent content = new StringContent(System.Text.Json.JsonSerializer.Serialize(query),
            Encoding.UTF8, "application/json");
        //Act
        var result = await _client.PutAsync("Titles", content);
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(204, (int)result.StatusCode);
    }
    [Fact]
    public async Task Delete_Request_Valido_Retorna_Sucesso()
    {
        //Arrange
        var id = Guid.NewGuid();
        //Act
        var result = await _client.DeleteAsync($"Titles/{id}");
        result.EnsureSuccessStatusCode();
        Assert.True(result.IsSuccessStatusCode);
        Assert.Equal(204, (int)result.StatusCode);
    }
}