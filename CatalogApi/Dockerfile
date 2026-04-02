FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CatalogApi/CatalogApi.csproj", "CatalogApi/"]
COPY ["SharedLibrary/SharedLibrary.csproj", "SharedLibrary/"]
RUN dotnet restore "CatalogApi/CatalogApi.csproj"
COPY . .
WORKDIR "/src/CatalogApi"
RUN dotnet build "CatalogApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatalogApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogApi.dll"]
