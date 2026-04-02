FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BasketApi/BasketApi.csproj", "BasketApi/"]
COPY ["SharedLibrary/SharedLibrary.csproj", "SharedLibrary/"]
RUN dotnet restore "BasketApi/BasketApi.csproj"
COPY . .
WORKDIR "/src/BasketApi"
RUN dotnet build "BasketApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasketApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BasketApi.dll"]
