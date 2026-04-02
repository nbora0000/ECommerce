FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SharedLibrary/SharedLibrary.csproj SharedLibrary/
COPY InventoryApi/InventoryApi.csproj InventoryApi/
RUN dotnet restore InventoryApi/InventoryApi.csproj

COPY SharedLibrary/ SharedLibrary/
COPY InventoryApi/ InventoryApi/
WORKDIR /src/InventoryApi
RUN dotnet publish InventoryApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InventoryApi.dll"]
