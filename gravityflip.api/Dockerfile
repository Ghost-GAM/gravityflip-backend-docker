# ─────────────────────────────────────────────────────────────────────────────
# ETAPA 1: Compilar la aplicación
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copiar todos los archivos del proyecto
COPY . .

# Restaurar dependencias y compilar en modo Release
RUN dotnet restore Gravityflip.Api.sln
RUN dotnet publish Gravityflip.Api/Gravityflip.Api.csproj -c Release -o /app/publish


# ─────────────────────────────────────────────────────────────────────────────
# ETAPA 2: Imagen final ligera (solo runtime)
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copiar el resultado de la compilación de la etapa anterior
COPY --from=build /app/publish .

# Exponer el puerto de la API
EXPOSE 5044

# Que escuche en todas las interfaces (no solo localhost)
ENV ASPNETCORE_URLS=http://+:5044

# Comando que se ejecuta al iniciar el contenedor
ENTRYPOINT ["dotnet", "Gravityflip.Api.dll"]
