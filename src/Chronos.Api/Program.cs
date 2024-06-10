var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<IMyTypedClient, MyTypedClient>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }


public interface IMyTypedClient
{
    Task MakeRequest();
}
public class MyTypedClient : IMyTypedClient
{
    private readonly HttpClient _httpClient;

    public MyTypedClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task MakeRequest()
    {
        await _httpClient.GetAsync("/hello-world");
    }

    public async Task<HttpResponseMessage> TakeIsValid()
    {
        var validacao = await _httpClient.GetAsync("/validation");
        return validacao;
    }
}