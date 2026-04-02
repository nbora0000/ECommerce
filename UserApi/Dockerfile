FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SharedLibrary/SharedLibrary.csproj SharedLibrary/
COPY UserApi/UserApi.csproj UserApi/
RUN dotnet restore UserApi/UserApi.csproj

COPY SharedLibrary/ SharedLibrary/
COPY UserApi/ UserApi/
WORKDIR /src/UserApi
RUN dotnet publish UserApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserApi.dll"]
