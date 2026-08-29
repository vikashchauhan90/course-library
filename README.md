# course-library

```
CourseLibrary.Idp>
dotnet ef migrations add InitialCreate --project ..\CourseLibrary.Idp.Infrastructure --startup-project . --context ApplicationDbContext --output-dir Persistence\Migrations
dotnet ef database update --project ..\CourseLibrary.Idp.Infrastructure --startup-project . --context ApplicationDbContext
```

```
azurite --skipApiVersionCheck
```

## Redis:

open wsl terminal and run following command:

```
sudo service redis-server start
 for status:
 sudo service redis-server status
```