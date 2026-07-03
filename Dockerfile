# ESBA — imagen de producción (hito 12.6). Multi-stage: SDK para publicar,
# runtime ASP.NET para correr. Toda la config entra por env vars del stack de
# Portainer (12.1, ver docs/migracion/hito12-endurecimiento.md).

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj + props: el restore queda cacheado como capa y no se
# repite mientras no cambien las dependencias. El .editorconfig es parte del
# build: sin él, los analizadores corren con otra config (ej. CA1716) y
# TreatWarningsAsErrors rompe el publish.
COPY Directory.Build.props .editorconfig ./
COPY src/Esba.Domain/Esba.Domain.csproj src/Esba.Domain/
COPY src/Esba.Application/Esba.Application.csproj src/Esba.Application/
COPY src/Esba.Infrastructure/Esba.Infrastructure.csproj src/Esba.Infrastructure/
COPY src/Esba.Web/Esba.Web.csproj src/Esba.Web/
RUN dotnet restore src/Esba.Web/Esba.Web.csproj

COPY src/ src/
RUN dotnet publish src/Esba.Web/Esba.Web.csproj -c Release -o /app/publish --no-restore

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# QuestPDF (SkiaSharp) necesita fontconfig y fuentes del sistema: los reportes
# piden "Arial" y fontconfig la sustituye por Liberation Sans (métricamente
# compatible). Sin esto, la generación de PDFs falla en el contenedor.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# Claves de DataProtection (12.1): /keys se monta como volumen persistente del
# stack; sin la env var el arranque en Production falla a propósito.
ENV DataProtection__KeysPath=/keys
RUN mkdir /keys && chown $APP_UID /keys

COPY --from=build /app/publish .

# Usuario no-root de la imagen base (UID 1654). El puerto por defecto de la
# imagen es 8080 (ASPNETCORE_HTTP_PORTS).
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "Esba.Web.dll"]
