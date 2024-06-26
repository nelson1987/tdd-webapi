using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Chronos.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[EnableRateLimiting("fixed-by-ip")]
public class TitlesController : ControllerBase
{
    private readonly ITitlesRepository _repository;

    public TitlesController(ITitlesRepository repository)
    {
        _repository = repository;
    }

    //Get
    [HttpGet(Name = "GetTitles")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        var responseFilter = await _repository.GetAll(cancellationToken);
        if (responseFilter == null)
            return NotFound();
        var response = responseFilter.Select(x => new TitlesQueryResponse(x.Id, x.Description, x.Value));
        return Ok(response);
    }

    //GetById
    [HttpGet("{id:Guid}", Name = "GetTitlesById")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var responseFilter = await _repository.GetById(id, cancellationToken);
        if (responseFilter == null)
            return NotFound();
        TitlesQueryResponse response = new TitlesQueryResponse(responseFilter.Id, responseFilter.Description, responseFilter.Value);
        return Ok(response);
    }

    //Post
    [HttpPost(Name = "PostTitles")]
    public async Task<IActionResult> Post(
        [FromServices] ITitlesAddHandler handler,
        [FromBody] TitlesPostCommand command, CancellationToken cancellationToken = default)
    {
        await handler.Handler(command, cancellationToken);
        return Created();
    }

    //Put
    [HttpPut(Name = "PutTitles")]
    public async Task<IActionResult> Put([FromBody] TitlesPutCommand command, CancellationToken cancellationToken = default)
    {
        await _repository.Alterar(command.Id, command.Value, cancellationToken);
        return NoContent();
    }

    //Delete
    [HttpDelete("{id:Guid}", Name = "DeleteTitles")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.Remover(id, cancellationToken);
        return NoContent();
    }
}

public interface ITitlesAddHandler
{
    Task<Result> Handler(TitlesPostCommand command, CancellationToken cancellationToken = default);
}

public class TitlesAddHandler : ITitlesAddHandler
{
    private readonly ITitlesRepository _repository;
    private readonly IValidationTitlesHttpClientFactory _httpClient;
    private readonly ITitlesPublisher _publisher;

    public TitlesAddHandler(ITitlesRepository repository, IValidationTitlesHttpClientFactory httpClient, ITitlesPublisher publisher)
    {
        _repository = repository;
        _httpClient = httpClient;
        _publisher = publisher;
    }

    public async Task<Result> Handler(TitlesPostCommand command, CancellationToken cancellationToken = default)
    {
        if (!await _httpClient.Validar(command.Description, cancellationToken)) return Result.Fail("Erro na valida��o");
        var titulo = await _repository.Adicionar(new Titles() { Id = Guid.NewGuid(), Description = command.Description, Value = command.Value }, cancellationToken);
        await _publisher.Enviar(new TitlesPostEvent(titulo.Id, titulo.Description, titulo.Value), cancellationToken);
        return Result.Ok();
    }
}

public interface ITitlesPublisher
{
    Task Enviar(TitlesPostEvent evento, CancellationToken token = default);
}

public interface ITitlesRepository
{
    List<Titles> Titles { get; }
    Task<List<Titles>> GetAll(CancellationToken cancellationToken = default);
    Task<Titles?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<Titles> Adicionar(Titles titles, CancellationToken cancellationToken = default);
    Task Alterar(Guid id, decimal valor, CancellationToken cancellationToken = default);
    Task Remover(Guid id, CancellationToken cancellationToken = default);
}

public class TitlesRepository : ITitlesRepository
{
    public List<Titles> Titles { get; private set; }
    public TitlesRepository()
    {
        Titles = new List<Titles>(){
                new Titles() { Id = Guid.Parse("7c236a06-cd56-495c-bfb5-ded9aa7d93ae"), Description = "Description_1", Value = 10.00M },
                new Titles() { Id = Guid.Parse("32304383-22ff-4b41-bf42-1c376cd736db"), Description = "Description_2", Value = 20.00M }
            };
    }
    public Task<Titles> Adicionar(Titles titles, CancellationToken cancellationToken = default)
    {
        Titles.Add(titles);
        return Task.FromResult(titles);
    }

    public Task<Titles?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Titles.FirstOrDefault(x => x.Id == id));
    }

    public Task<List<Titles>> GetAll(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Titles);
    }

    public Task Alterar(Guid id, decimal valor, CancellationToken cancellationToken = default)
    {
        var titles = Titles.FirstOrDefault(x => x.Id == id);
        Titles.Remove(titles);
        titles.MudarValor(valor);
        Titles.Add(titles);
        return Task.CompletedTask;
    }

    public Task Remover(Guid id, CancellationToken cancellationToken = default)
    {
        var titles = Titles.FirstOrDefault(x => x.Id == id);
        Titles.Remove(titles);
        return Task.CompletedTask;
    }
}
public class FakeTitlesRepositoryWithException : ITitlesRepository
{
    public List<Titles> Titles => throw new NotImplementedException();

    public Task<Titles> Adicionar(Titles titles, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Alterar(Guid id, decimal valor, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Titles>> GetAll(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Titles?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Remover(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public record TitlesQuery(Guid Id);
public record TitlesQueryResponse(Guid Id, string Description, decimal Value);
public record TitlesPostCommand(string Description, decimal Value);
public record TitlesPutCommand(Guid Id, decimal Value);
public record TitlesDeleteCommand();
public record TitlesPostEvent(Guid Id, string Description, decimal Value);

public class Titles
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public decimal Value { get; set; }

    internal void MudarValor(decimal valor)
    {
        Value = valor;
    }
}