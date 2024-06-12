using Microsoft.AspNetCore.Mvc;

namespace Chronos.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CreateAgreementsController : ControllerBase
{
    private readonly IHttpValidationClientFactory _logger;
    private readonly IMovimentacaoRepository _repository;
    private readonly ICreateAgreementPublisher _publisher;
    public CreateAgreementsController(IHttpValidationClientFactory logger,
        IMovimentacaoRepository repository,
        ICreateAgreementPublisher publisher)
    {
        _logger = logger;
        _repository = repository;
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateAgreementCommand command)
    {
        if (await _logger.Validar(command.Email)) return BadRequest();
        //try
        //{
            await _repository.Incluir(command.Email, command.Valor);
        //}
        //catch (Exception ex)
        //{
        //    throw;
        //}
        await _publisher.Enviar(new CreateAgreementEvent(command.Email, command.Valor));
        return Created();
    }
}
public record CreateAgreementCommand(string Email, decimal Valor);
public record CreateAgreementEvent(string Email, decimal Valor);

public interface IHttpValidationClientFactory
{
    Task<bool> Validar(string contaDebitante);
}
public interface IMovimentacaoRepository
{
    Task Incluir(string contaDebitante, decimal valor);
}
public interface ICreateAgreementPublisher
{
    Task Enviar(CreateAgreementEvent evento);
}