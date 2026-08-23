var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddContainer("servicebus-sql", "mcr.microsoft.com/mssql/server:2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", "Welcome@123")
    .WithEndpoint(targetPort: 1433, name: "sql");

var serviceBus = builder
    .AddContainer(
        "servicebus",
        "mcr.microsoft.com/azure-messaging/servicebus-emulator")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", "Welcome@123")
    .WithEndpoint(targetPort: 5672, name: "amqp");

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
