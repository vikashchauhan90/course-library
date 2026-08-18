FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/idp/CourseLibrary.Idp/CourseLibrary.Idp.csproj"
RUN dotnet publish "src/idp/CourseLibrary.Idp/CourseLibrary.Idp.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENTRYPOINT ["dotnet", "CourseLibrary.Idp.dll"]
