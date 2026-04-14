# Imagen base para el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Imagen para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GymApp.csproj", "./"]
RUN dotnet restore "./GymApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "GymApp.csproj" -c Release -o /app/build

# Publicación de la aplicación
FROM build AS publish
RUN dotnet publish "GymApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GymApp.dll"]
