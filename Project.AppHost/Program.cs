var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("appdb");

var worker = builder.AddProject<Projects.Project_Worker>("worker")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.AddProject<Projects.Project_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(worker)
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();
