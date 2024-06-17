var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var messaging = builder.AddRabbitMQ("RabbitMQConnection");
var sql = builder.AddSqlServer("sql");
var sqldb = sql.AddDatabase("sqldb");

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(messaging)
    .WithReference(sqldb);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(apiService);

builder.AddProject<Projects.Vertical>("vertical");

builder.Build().Run();
