using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongo")
                     .WithLifetime(ContainerLifetime.Persistent);

// Farklý database'ler ekleyin (ayný server üzerinde)
var mainDatabase = mongodb.AddDatabase("main-db");
var logDatabase = mongodb.AddDatabase("log-db");

builder.AddProject<Projects.WebAPI_REST>("webapi-rest")
       .WithReference(mongodb)
       .WithReference(mainDatabase)
       .WithReference(logDatabase);

builder.Build().Run();
