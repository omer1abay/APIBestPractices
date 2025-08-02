
var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongo").WithLifetime(ContainerLifetime.Persistent).AddDatabase("log-db");

builder.AddProject<Projects.WebAPI_REST>("webapi-rest").WithReference(mongodb);

builder.Build().Run();
