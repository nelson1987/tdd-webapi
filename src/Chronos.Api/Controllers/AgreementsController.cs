using Microsoft.AspNetCore.Mvc;

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

    public AgreementsController(ILogger<AgreementsController> logger, IMyTypedClient myTypedClient)
    {
        _logger = logger;
        _myTypedClient = myTypedClient;
    }

    [HttpGet(Name = "GetAgreements")]
    public async Task<IEnumerable<ListAgreementResponse>> Get()
    {
        await _myTypedClient.MakeRequest();

        return Enumerable.Range(1, 5).Select(index => new ListAgreementResponse
            (DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            Summaries[Random.Shared.Next(Summaries.Length)]))
        .ToArray();
    }
    [HttpPost]
    public async Task<IEnumerable<ListAgreementResponse>?> Post()
    {
        await _myTypedClient.MakeRequest();
        return null;
    }
}
public record ListAgreementResponse(DateOnly Date, int TemperatureC, string Summary);