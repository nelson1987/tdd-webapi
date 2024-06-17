using AutoMapper;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics;
using System.Text.Json;

namespace AspireApp.ApiService.Features;
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
public record WeatherForecastEvent(DateOnly Date, int TemperatureC, string? Summary);

public static class Extensions
{
    public static string ToJson(this object model)
    {
        return JsonSerializer.Serialize(model);
    }
}
public class HelloWorldMessageConsumer : IConsumer<WeatherForecastEvent>
{
    public async Task Consume(ConsumeContext<WeatherForecastEvent> context)
    {
        Debug.WriteLine($"Received: {context.Message}");
        await Task.CompletedTask;
    }
}

public static class CreateProduct
{
    public record Request(string FirstName, string LastName, decimal Price);
    public record Response(int Id, string Name, decimal Price);
    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).MaximumLength(100);
            RuleFor(x => x.Price).GreaterThan(0);
        }
    }
    public class PagamentoMapping : Profile
    {
        public PagamentoMapping()
        {
            CreateMap<Request, Product>();
            CreateMap<Product, Response>();
        }
    }
    public class Endpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/products", Handler)
                .WithTags("Products");
        }

        public static async Task<IResult> Handler(Request request,
            IValidator<Request> validator,
            YourDbContext context)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }

            var product = request.MapTo<Product>();
            product.isActive = true;

            context.Products.Add(product);

            context.SaveChanges();

            return Results.Ok(product.MapTo<Response>());
        }
    }

    public class Dependencies
    {
        public static void AddServices(IServiceCollection services)
        {
            services.AddScoped<IValidator<Request>, Validator>();
        }
    }
}

public class Product
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public decimal Price { get; set; }
    public bool isActive { get; set; }
}


public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options)
    {
    }
    public DbSet<Product> Products { get; set; }

    /*
     OnModelCreating mainly used to configured our EF model
     And insert master data if required
    */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Setting a primary key in OurHero model
        //modelBuilder.Entity<Product>().HasKey(x => x.Id);
        new ProductEntityTypeConfiguration().Configure(modelBuilder.Entity<Product>());

        // Inserting record in OurHero table
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                FirstName = "System",
                LastName = "",
                isActive = true,
                Price = 1.99M
            }
        );
    }
}
public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("TB_PRODUTO");
        builder.HasKey(x => x.Id)
                .HasName("IDT_PRODUTO");
        builder.Property(x => x.Price)
            .HasColumnType("decimal(5,2)");
    }
}

public static class Mappers
{
    private static readonly Lazy<IMapper> Lazy = new(() =>
    {
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Mappers).Assembly));
        return config.CreateMapper();
    });

    public static IMapper Mapper => Lazy.Value;

    public static T MapTo<T>(this object source) => Mapper.Map<T>(source);
}