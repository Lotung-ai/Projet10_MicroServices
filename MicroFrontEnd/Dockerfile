# �tape 1: Utiliser l'image de base ASP.NET pour l'ex�cution
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# �tape 2: Utiliser l'image SDK pour la construction
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copier les fichiers csproj pour chaque projet
COPY ["MicroFrontEnd/MicroFrontEnd.csproj", "MicroFrontEnd/"]
COPY ["MicroServices/MicroServices.csproj", "MicroServices/"]
COPY ["OcelotAPIGw/OcelotAPIGw.csproj", "OcelotAPIGw/"]

# Restaurer les d�pendances pour le projet MicroFrontEnd
RUN dotnet restore "./MicroFrontEnd/MicroFrontEnd.csproj"

# Copier le reste des fichiers source
COPY . .

# Construire le projet MicroFrontEnd
WORKDIR "/src/MicroFrontEnd"
RUN dotnet build "MicroFrontEnd.csproj" -c $BUILD_CONFIGURATION -o /app/build

# �tape 3: Publier le projet MicroFrontEnd dans un r�pertoire distinct
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MicroFrontEnd.csproj" -c $BUILD_CONFIGURATION -o /app/microfrontend_publish /p:UseAppHost=false

# �tape 4: Image finale pour l'ex�cution
FROM base AS final
WORKDIR /app
COPY --from=publish /app/microfrontend_publish .
ENTRYPOINT ["dotnet", "MicroFrontEnd.dll"]
