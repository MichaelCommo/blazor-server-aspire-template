var builder = DistributedApplication.CreateBuilder(args);

// WithDataVolume gives Postgres a named volume so the schema and seeded user
// survive restarts. Without it the container gets an anonymous volume that is
// discarded on shutdown, so every run starts on an empty database.
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
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
