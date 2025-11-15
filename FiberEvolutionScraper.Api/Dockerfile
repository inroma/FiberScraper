# PROD
#FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim-arm64v8 AS base
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim-arm64v8 AS build
#WORKDIR /src
#COPY ["FiberEvolutionScraper.Api/FiberEvolutionScraper.Api.csproj", "FiberEvolutionScraper.Api/"]
#RUN dotnet restore "FiberEvolutionScraper.Api/FiberEvolutionScraper.Api.csproj"
#COPY . .
#RUN dotnet build "FiberEvolutionScraper.Api/FiberEvolutionScraper.Api.csproj" -r linux-arm64 -c Release -o /app/api/build
#
#FROM build AS publish
#RUN dotnet publish "FiberEvolutionScraper.Api/FiberEvolutionScraper.Api.csproj" -r linux-arm64 -c Release -o /app/api/publish
#
#FROM base as final
#WORKDIR /app/api
#COPY --from=publish /app/api/publish .
#ENV ASPNETCORE_URLS=http://+:5003
#ENTRYPOINT ["dotnet", "FiberEvolutionScraper.Api.dll"]


# DEV
 FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
 WORKDIR /app
 
 FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
 WORKDIR /src
 COPY ["FiberEvolutionScraper.Api/FiberEvolutionScraper.Api.csproj", "FiberEvolutionScraper.Api/"]
 RUN dotnet restore "FiberEvolutionScraper.Api/FiberEvolutionScraper.Api.csproj"
 COPY . .
 RUN dotnet build "FiberEvolutionScraper.Api/FiberEvolutionScraper.Api.csproj" -c Release -o /app/api/build
 
 FROM build AS publish
 RUN dotnet publish "FiberEvolutionScraper.Api/FiberEvolutionScraper.Api.csproj" -c Release -o /app/api/publish
 
 FROM mcr.microsoft.com/dotnet/aspnet:8.0
 WORKDIR /app/api
 COPY --from=publish /app/api/publish .
 ENTRYPOINT ["dotnet", "FiberEvolutionScraper.Api.dll"]