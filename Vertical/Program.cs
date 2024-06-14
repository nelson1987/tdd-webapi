using Microsoft.EntityFrameworkCore;
using Vertical.Controllers;

var builder = WebApplication.CreateBuilder(args);

//*********************** Add services to the container.***********************
//*********************** Add services to the container end.***********************

//*********************** Register DbContext and provide ConnectionString .***********************
//builder.Services.AddDbContext<AppDbContext>(db => 
//db.UseSqlServer(builder.Configuration.GetConnectionString("OurHeroConnectionString")), 
//ServiceLifetime.Singleton);
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("EntFra"));
//*********************** Register DbContext end.***********************
CreateProduct.Dependencies.AddServices(builder.Services);
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

CreateProduct.Endpoint.MapEndpoint(app);

app.Run();

