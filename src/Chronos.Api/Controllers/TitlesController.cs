using Microsoft.AspNetCore.Mvc;

namespace Chronos.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class TitlesController : ControllerBase
{
    private readonly TitlesRepository _repository;
    public TitlesController(TitlesRepository repository)
    {
        _repository = repository;
    }

    //Get
    [HttpGet(Name = "GetTitles")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        var response = _repository.Titles.Select(x => new TitlesQueryResponse(x.Id, x.Description, x.Value));
        return Ok(response);
    }
    //GetById
    [HttpGet("{id:Guid}", Name = "GetTitlesById")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var responseFilter = _repository.Titles.FirstOrDefault(x => x.Id == id);
        if (responseFilter == null)
            return NotFound();
        TitlesQueryResponse response = new TitlesQueryResponse(responseFilter.Id, responseFilter.Description, responseFilter.Value);
        return Ok(response);
    }
    //Post
    [HttpPost(Name = "PostTitles")]
    public async Task<IActionResult> Post(TitlesPostCommand command, CancellationToken cancellationToken = default)
    {
        _repository.Adicionar(new Titles() { Id = Guid.NewGuid(), Description = command.Description, Value = command.Value });
        return Created();
    }
    //Put
    [HttpPut(Name = "PutTitles")]
    public async Task<IActionResult> Put(TitlesPutCommand command, CancellationToken cancellationToken = default)
    {
        return NoContent();
    }
    //Delete
    [HttpDelete("{id:Guid}", Name = "DeleteTitles")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        return NoContent();
    }
}
public class TitlesRepository
{
    public List<Titles> Titles { get; private set; }
    public TitlesRepository()
    {
        Titles = new List<Titles>(){
                new Titles() { Id = Guid.Parse("7c236a06-cd56-495c-bfb5-ded9aa7d93ae"), Description = "Description_1", Value = 10.00M },
                new Titles() { Id = Guid.Parse("32304383-22ff-4b41-bf42-1c376cd736db"), Description = "Description_2", Value = 20.00M }
            };
    }
    public void Adicionar(Titles titles)
    {
        Titles.Add(titles);
    }
}
public record TitlesQuery(Guid Id);
public record TitlesQueryResponse(Guid Id, string Description, decimal Value);
public record TitlesPostCommand(string Description, decimal Value);
public record TitlesPutCommand();
public record TitlesDeleteCommand();
public class Titles
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public decimal Value { get; set; }
}