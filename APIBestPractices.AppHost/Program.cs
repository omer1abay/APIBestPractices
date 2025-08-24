using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongo")
                     .WithLifetime(ContainerLifetime.Persistent);

// Farklý database'ler ekleyin (ayný server üzerinde)
var mainDatabase = mongodb.AddDatabase("main-db");
var logDatabase = mongodb.AddDatabase("log-db");

// Add Redis for caching
var cache = builder.AddRedis("cache")
                   .WithRedisCommander()
                   .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.WebAPI_REST>("webapi-rest")
       .WithReference(mongodb)
       .WithReference(mainDatabase)
       .WithReference(logDatabase)
       .WithReference(cache);

builder.Build().Run();
