var builder = DistributedApplication.CreateBuilder(args);

var idpDatabase = builder
    .AddPostgres("idp-postgres")
    .AddDatabase("DefaultConnection");

var api = builder.AddProject<Projects.CourseLibrary_Api>("courselibrary-api");
var idp = builder
    .AddProject<Projects.CourseLibrary_Idp>("idp")
    .WithReference(idpDatabase)
    .WaitFor(idpDatabase);

builder
    .AddProject<Projects.CourseLibrary_Gateway>("gateway")
    .WithReference(api)
    .WithReference(idp);

builder.Build().Run();
