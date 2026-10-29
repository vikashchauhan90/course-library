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

## Shared Course API Client

`src/course/CourseLibrary.Client` is the reusable Course API client module.
Applications consume `ICourseApiClient` and do not construct gateway URLs or
bearer headers themselves. The module provides:

- typed course contracts and operations;
- access-token injection through `IAccessTokenProvider`;
- standard HTTP resilience with exponential jitter, circuit breaking, timeout,
	and no retries for unsafe mutation methods;
- `ActivitySource`, `Meter`, and source-generated logs for each operation;
- structured `CourseApiException` errors that preserve HTTP status and problem
	details.

The App registers this module with `AddCourseLibraryClients`. Its OIDC token
provider is kept in the App composition root, while HTTP concerns remain in
the shared module.

# Execution example:
<img width="1905" height="762" alt="image" src="https://github.com/user-attachments/assets/dd71ad0e-6d9e-4b1b-ba48-f593e45bf588" />
<img width="1910" height="934" alt="image" src="https://github.com/user-attachments/assets/1890b263-2604-496f-8799-670c3e14b623" />
<img width="1831" height="840" alt="image" src="https://github.com/user-attachments/assets/c6f24d02-48f9-4498-b695-c0a3d8189aee" />
<img width="1911" height="802" alt="image" src="https://github.com/user-attachments/assets/da7994b8-0d4a-435a-b859-626663bad131" />
<img width="1910" height="946" alt="image" src="https://github.com/user-attachments/assets/493b762c-b019-4399-a344-ead3cee384b4" />
<img width="1905" height="937" alt="image" src="https://github.com/user-attachments/assets/b4fb1334-ea44-4567-aea6-1d8ce7f0dd8c" />
<img width="1911" height="934" alt="image" src="https://github.com/user-attachments/assets/76e2905a-df09-44d4-b793-0d52d8bbe752" />
<img width="1910" height="933" alt="image" src="https://github.com/user-attachments/assets/ad823af8-c6da-4ce2-ab89-e81994bca81e" />
<img width="1913" height="958" alt="image" src="https://github.com/user-attachments/assets/64b67a86-f76d-48fd-94d4-ef56daec55b8" />
<img width="1910" height="934" alt="image" src="https://github.com/user-attachments/assets/62e832fa-4995-4b1a-a410-32df71061e7c" />
<img width="1910" height="946" alt="image" src="https://github.com/user-attachments/assets/01671376-b47f-410e-a4df-5b4e315acba6" />









