#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

# Development 7000 http 7001 https
EXPOSE 7000 
EXPOSE 7001

# Production 8000 http 8001 https
EXPOSE 8000 
EXPOSE 8001 

# Define enviroment in launchSettings.json

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HeyaChat-Authorization/HeyaChat-Authorization.csproj", "HeyaChat-Authorization/"]
RUN dotnet restore "./HeyaChat-Authorization/HeyaChat-Authorization.csproj"
COPY . .
WORKDIR "/src/HeyaChat-Authorization"
RUN dotnet build "./HeyaChat-Authorization.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HeyaChat-Authorization.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HeyaChat-Authorization.dll"]