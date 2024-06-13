using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json.Serialization;

namespace Chronos.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AgreementsController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<AgreementsController> _logger;
    private readonly IMyTypedClient _myTypedClient;
    private readonly IDistributedCache _cache;

    public AgreementsController(ILogger<AgreementsController> logger,
        IMyTypedClient myTypedClient,
        IDistributedCache cache)
    {
        _logger = logger;
        _myTypedClient = myTypedClient;
        _cache = cache;
    }

    [HttpGet(Name = "GetAgreements")]
    public async Task<IEnumerable<ListAgreementResponse>> Get()
    {
        await _myTypedClient.MakeRequest();

        return Enumerable.Range(1, 5).Select(index => new ListAgreementResponse
            (DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
            Random.Shared.Next(-20, 55),
            Summaries[Random.Shared.Next(Summaries.Length)]))
        .ToArray();
    }

    [HttpGet("{id}", Name = "GetRedisCache")]
    public async Task<IActionResult> GetRedisCache(int id, CancellationToken cancellationToken = default)
    {
        return Ok(await GetProduto(id, cancellationToken));
    }

    private async Task<string[]> GetProduto(int id, CancellationToken cancellationToken = default)
    {

        string redisKey = $"Get_Produto_{id}";
        var responseBytes = await _cache.GetStringAsync(redisKey, cancellationToken);
        if (responseBytes == null)
        {
            string[] produto = new[] { $"ABRE{id}", $"FECHA{id}" };
            await _cache.SetStringAsync(redisKey, System.Text.Json.JsonSerializer.Serialize(produto), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) }, cancellationToken);
            return produto;
        }
        return System.Text.Json.JsonSerializer.Deserialize<string[]>(responseBytes, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;
    }
}

public record ListAgreementResponse(DateOnly Date, int TemperatureC, string Summary);
