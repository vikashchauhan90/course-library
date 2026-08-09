FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CourseLibrary.Idp.csproj", "./"]
RUN dotnet restore "CourseLibrary.Idp.csproj"
COPY . .
RUN dotnet publish "CourseLibrary.Idp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENTRYPOINT ["dotnet", "CourseLibrary.Idp.dll"]
