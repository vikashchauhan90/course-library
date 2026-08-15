var builder = DistributedApplication.CreateBuilder(args);
var api = builder.AddProject<Projects.CourseLibrary_Api>("courselibrary-api");
//var idp = builder.AddProject<Projects.CourseLibrary_Idp>("idp");

builder
    .AddProject<Projects.CourseLibrary_Gateway>("gateway")
    .WithReference(api);

builder.Build().Run();
