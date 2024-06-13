using Microsoft.AspNetCore.Mvc;

namespace Phanes.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class TitlesController : ControllerBase
{
    private readonly List<Titles> _response;
    public TitlesController()
    {
        _response = new List<Titles>() {
            new Titles() { Id = Guid.NewGuid(), Description = "Description_1", Value = 10.00M },
            new Titles() { Id = Guid.NewGuid(), Description = "Description_2", Value = 20.00M }
        };
    }

    //Get
    [HttpGet(Name = "GetTitles")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        var response = _response.Select(x => new TitlesQueryResponse(x.Id, x.Description, x.Value));
        return Ok(response);
    }
    //GetById
    [HttpGet("{id}", Name = "GetTitlesById")]
    public async Task<IActionResult> GetById(TitlesQuery query, CancellationToken cancellationToken = default)
    {
        var responseFilter = _response.FirstOrDefault(x => x.Id == query.Id);
        if (responseFilter == null)
            return NotFound();
        TitlesQueryResponse response = new TitlesQueryResponse(responseFilter.Id, responseFilter.Description, responseFilter.Value);
        return Ok(response);
    }
    //Post
    [HttpPost(Name = "PostTitles")]
    public async Task<IActionResult> Post(TitlesPostCommand command, CancellationToken cancellationToken = default)
    {
        return Created();
    }
    //Put
    [HttpPut(Name = "PutTitles")]
    public async Task<IActionResult> Put(TitlesPutCommand command, CancellationToken cancellationToken = default)
    {
        return NoContent();
    }
    //Delete
    [HttpDelete(Name = "DeleteTitles")]
    public async Task<IActionResult> Delete(TitlesDeleteCommand command, CancellationToken cancellationToken = default)
    {
        return NoContent();
    }
}
public record TitlesQuery(Guid Id);
public record TitlesQueryResponse(Guid Id, string Description, decimal Value);
public record TitlesPostCommand();
public record TitlesPutCommand();
public record TitlesDeleteCommand();
public class Titles
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public decimal Value { get; set; }
}