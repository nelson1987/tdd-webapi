using Microsoft.AspNetCore.Mvc;

namespace Chronos.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CreateAgreementsController : ControllerBase
{
    private readonly IValidationTitlesHttpClientFactory _logger;
    private readonly IMovimentacaoRepository _repository;
    private readonly ICreateAgreementPublisher _publisher;
    public CreateAgreementsController(IValidationTitlesHttpClientFactory logger,
        IMovimentacaoRepository repository,
        ICreateAgreementPublisher publisher)
    {
        _logger = logger;
        _repository = repository;
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateAgreementCommand command, CancellationToken token)
    {
        if (!await _logger.Validar(command.Email, token)) return BadRequest();
        await _repository.Incluir(command.Email, command.Valor, token);
        await _publisher.Enviar(new CreateAgreementEvent(command.Email, command.Valor), token);
        //return StatusCode((int)HttpStatusCode.Created);
        return Created("", token);
    }
}
public record IsValidCommand(bool IsValid);
public record CreateAgreementCommand(string Email, decimal Valor);
public record CreateAgreementEvent(string Email, decimal Valor);

public interface IValidationTitlesHttpClientFactory
{
    Task<bool> Validar(string contaDebitante, CancellationToken token = default);
}
public interface IMovimentacaoRepository
{
    Task Incluir(string contaDebitante, decimal valor, CancellationToken token = default);
}
public interface ICreateAgreementPublisher
{
    Task Enviar(CreateAgreementEvent evento, CancellationToken token = default);
}
public class HttpValidationClientFactory : IValidationTitlesHttpClientFactory
{
    private readonly HttpClient _httpClient;

    public HttpValidationClientFactory(HttpClient httpClient) => _httpClient = httpClient;

    private async Task<HttpResponseMessage> TakeIsValid()
    {
        var validacao = await _httpClient.GetAsync("/validation");
        return validacao;
    }

    public async Task<bool> Validar(string contaDebitante, CancellationToken token = default)
    {
        var result = await TakeIsValid();
        var resultJson = await result.Content.ReadAsStringAsync();
        IsValidCommand response = System.Text.Json.JsonSerializer.Deserialize<IsValidCommand>(resultJson,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;
        return response.IsValid;
    }
}

public class MovimentacaoRepository : IMovimentacaoRepository
{
    public async Task Incluir(string contaDebitante, decimal valor, CancellationToken token = default)
    {
        //throw new NotImplementedException();
        await Task.CompletedTask;
    }
}

public class CreateAgreementPublisher : ICreateAgreementPublisher
{
    public async Task Enviar(CreateAgreementEvent evento, CancellationToken token = default)
    {
        await Task.CompletedTask;
    }
}