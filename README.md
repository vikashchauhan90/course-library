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

## CourseLibrary.App

The web application runs at `https://localhost:7061` and uses the IDP at
`https://localhost:59890`. Create an authorization-code client from the IDP
admin screen (`/admin/clients/create`) with these values:

```text
Client ID: course-library-app
Display name: Course Library Web App
Redirect URI: https://localhost:7061/signin-oidc
Grant type: Authorization Code
Scope: course-library-api
```

Copy the generated secret into
`src/app/CourseLibrary.App/appsettings.Development.json` under
`OpenId:ClientSecret`, then start the services:

```powershell
dotnet run --project .\src\idp\CourseLibrary.Idp
dotnet run --project .\src\gateway\CourseLibrary.Gateway
dotnet run --project .\src\app\CourseLibrary.App
```

The app signs users in with OpenID Connect, stores the access token in its
server-side authentication session, and sends it as a bearer token when
calling the gateway. The initial UI supports lookup through the existing
`GET /api/v1/courses/{courseId}/{partitionKey}` API endpoint.

The App also provides global course search and authenticated ownership
management. Users can view courses returned by search, but only the owner can
create, update, or delete a course. The gateway validates the bearer token and
the API derives ownership from the forwarded subject instead of trusting a
browser-supplied owner ID.

In Development, the IDP seeds the database on startup when
`Database:ApplyMigrationsOnStartup` and `Database:SeedDevelopmentUser` are
enabled. It creates the `course-library-app` client, the `course-library-api`
scope, and this administrator account:

```text
Email: admin@courselibrary.local
Password: Admin@12345!
```

The administrator must enable authenticator-based MFA before accessing the
administration area. Restart the IDP after changing the seeder or its
configuration. The applications and scopes tables are seeded immediately;
authorization and token rows are created only after a successful OAuth login.