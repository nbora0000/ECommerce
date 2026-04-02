FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SharedLibrary/SharedLibrary.csproj SharedLibrary/
COPY NotificationApi/NotificationApi.csproj NotificationApi/
RUN dotnet restore NotificationApi/NotificationApi.csproj

COPY SharedLibrary/ SharedLibrary/
COPY NotificationApi/ NotificationApi/
WORKDIR /src/NotificationApi
RUN dotnet publish NotificationApi.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationApi.dll"]
