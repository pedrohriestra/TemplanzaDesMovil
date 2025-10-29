# 🌿 Proyecto Templanza — App Móvil + API + DB

## 📖 Descripción General
**Templanza** es una aplicación desarrollada en **.NET 9 con MAUI Blazor Hybrid** que permite gestionar usuarios y blends (mezclas de hierbas aromáticas), conectándose a una **API REST** desplegada en Render y utilizando una **base de datos SQL Server** alojada en Somee.

El proyecto implementa:
- 🔐 **Login y registro de usuarios con JWT**
- 👤 **Gestión completa de usuarios (ABM)** con roles y foto de perfil
- 🍵 **Gestión de blends** con nombre, tipo, stock, precio e imagen
- 💾 **Persistencia en base de datos remota (Somee)**
- ☁️ **Despliegue en Render** con Docker
- 📱 **Interfaz móvil híbrida (MAUI Blazor Hybrid)**

---

## 🧩 Estructura de Solución

### 🗂️ 1. `Templanza.Domain`
Contiene las **entidades del negocio**:
- `Usuario`
- `Blend`

También define los **DTOs** compartidos entre la API y la app móvil.  
Sirve como “modelo común” para evitar duplicación de estructuras.

### ⚙️ 2. `Templanza.Api`
Proyecto **ASP.NET Core Web API** encargado de:
- Exponer endpoints REST (`/api/users`, `/api/blends`, `/api/auth`)
- Implementar autenticación JWT
- Conectarse a la base de datos SQL Server (Somee)
- Aplicar migraciones automáticas con EF Core

📍 Base de datos: `templanzaDB.mssql.somee.com`  
📍 Deploy en: [https://templanza-api.onrender.com](https://templanza-api.onrender.com)

### 📱 3. `Templanza.Mobile`
Aplicación **.NET MAUI Blazor Hybrid**, multiplataforma (Android, Windows, iOS, macOS).  
Funciones principales:
- Login y registro de usuario
- Visualización y edición de blends
- Gestión de perfil con imagen de usuario
- Interfaz adaptada con **Bootstrap 5** + **íconos de Bootstrap**

El `HttpClient` de la app apunta a la API desplegada en Render.

---

## 🚀 Despliegue

### 🧱 API — Render
1. Crear un nuevo servicio en [Render.com](https://render.com)
2. En el campo **Repository**, vincular el repositorio GitHub con el proyecto.
3. Crear un archivo `Dockerfile` en la raíz del proyecto API con:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
   WORKDIR /app
   COPY . .
   RUN dotnet publish -c Release -o out

   FROM mcr.microsoft.com/dotnet/aspnet:9.0
   WORKDIR /app
   COPY --from=build /app/out .
   ENTRYPOINT ["dotnet", "Templanza.Api.dll"]
   ```
4. En **Environment Variables**, agregar:
   - `ConnectionStrings__Default` → cadena de conexión Somee  
   - `Jwt__Key` → clave secreta para JWT  
   - `ASPNETCORE_ENVIRONMENT` → `Production`

5. Render compilará automáticamente con Docker y publicará la API.

---

### 🧩 Base de datos — Somee
1. Crear base de datos en [https://www.somee.com](https://www.somee.com)
2. Copiar la cadena de conexión completa.
3. Incluirla en las variables de entorno (`ConnectionStrings__Default`).
4. EF Core se encargará de crear las tablas al iniciar la API.

---

### 📱 Aplicación Móvil — MAUI
Para ejecutar en un equipo nuevo:

1. Instalar **.NET 9 SDK** y **Visual Studio 2022/2025** con el workload de **.NET MAUI**.
2. Clonar el repositorio:
   ```bash
   git clone https://github.com/<usuario>/TemplanzaDesMovil.git
   cd TemplanzaDesMovil
   ```
3. Restaurar paquetes:
   ```bash
   dotnet restore
   ```
4. Abrir el proyecto `Templanza.Mobile` y ejecutar:
   - En Android Emulator  
   - En Windows (Blazor Hybrid)
5. Confirmar que en `MauiProgram.cs` el `HttpClient` apunte a:
   ```csharp
   BaseAddress = new Uri("https://templanza-api.onrender.com/")
   ```

---

## 🧠 Tecnologías Principales
- **.NET 9 / C#**
- **Entity Framework Core**
- **JWT Authentication**
- **Bootstrap 5 + Bootstrap Icons**
- **MAUI Blazor Hybrid**
- **SQL Server (Somee)**
- **Docker / Render Deployment**

---

## 📚 Créditos
Desarrollado por **Pedro Herrera**, estudiante de *Tecnicatura en Desarrollo de Software*.  
Trabajo práctico integrador de **Programación II / Desarrollo de Software Móvil**.

---
© 2025 — Proyecto académico Templanza 🌱
