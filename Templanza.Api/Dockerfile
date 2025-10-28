# syntax=docker/dockerfile:1

# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos los .csproj primero para cachear restore
COPY Templanza.Domain/Templanza.Domain.csproj Templanza.Domain/
COPY Templanza.Api/Templanza.Api.csproj Templanza.Api/

RUN dotnet restore Templanza.Api/Templanza.Api.csproj

# Ahora el resto del código
COPY . .

WORKDIR /src/Templanza.Api
RUN dotnet publish -c Release -o /app/out

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/out .

# Kestrel escucha en 0.0.0.0:10000 (Render detecta este puerto)
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Templanza.Api.dll"]
