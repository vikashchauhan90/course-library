var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddContainer("servicebus-sql", "mcr.microsoft.com/mssql/server:2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", "Welcome@123")
    .WithEndpoint(
    port: 1433,
    targetPort: 1433,
    name: "servicebus-sql");

var serviceBus = builder
    .AddContainer(
        "servicebus",
        "mcr.microsoft.com/azure-messaging/servicebus-emulator")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", "Welcome@123")
    .WithEnvironment("SQL_SERVER", "servicebus-sql")
    .WithBindMount(
    "./ServiceBus/Config.json",
    "/ServiceBus_Emulator/ConfigFiles/Config.json")
    .WithEndpoint(
    port: 5672,
    targetPort: 5672,
    name: "servicebus")
    .WaitFor(sql);

var api = builder
    .AddProject<Projects.CourseLibrary_Api>("api")
    .WaitFor(serviceBus);

var idp = builder
   .AddProject<Projects.CourseLibrary_Idp>("idp");

var consumer = builder
    .AddProject<Projects.CourseLibrary_EventConsumer>("consumer")
    .WaitFor(serviceBus);

var app = builder
    .AddProject<Projects.CourseLibrary_App>("app")
    .WaitFor(serviceBus);

builder
    .AddProject<Projects.CourseLibrary_Gateway>("gateway")
    .WithReference(api)
    .WithReference(idp)
    .WithReference(consumer)
    .WithReference(app);

builder.Build().Run();
