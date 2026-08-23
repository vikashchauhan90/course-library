var builder = DistributedApplication.CreateBuilder(args);

var api = builder
    .AddProject<Projects.CourseLibrary_Api>("api");

var idp = builder
    .AddProject<Projects.CourseLibrary_Idp>("idp");

var consumer = builder
    .AddProject<Projects.CourseLibrary_EventConsumer>("consumer");

builder
    .AddProject<Projects.CourseLibrary_Gateway>("gateway")
    .WithReference(api)
    .WithReference(idp)
    .WithReference(consumer);

builder.Build().Run();
