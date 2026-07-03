# Hito 12 — Endurecimiento para producción

Deploy objetivo: **contenedor Docker gestionado por Portainer**; toda la configuración y
los secretos entran por variables de entorno del stack. Desglose por incrementos según
CLAUDE.md §6.

## 12.1 — Config por env vars + DataProtection ✅ 2026-07-02

### Qué se hizo

- **`sysdba/masterkey` fuera del repo**: `appsettings.Development.json` ya no trae
  `ConnectionStrings`. En desarrollo la cadena vive en **user-secrets**
  (`UserSecretsId` ya existía por las credenciales SMTP):

  ```bash
  dotnet user-secrets set "ConnectionStrings:Esba" \
    "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1" \
    --project src/Esba.Web
  ```

  > La contraseña quedó en el historial de git; es la default de Firebird y el 12.2 la
  > reemplaza por un usuario de mínimos privilegios con credenciales nuevas.

- **Sin código de parsing nuevo**: `AddInfrastructure` ya leía
  `configuration.GetConnectionString("Esba")` y falla con mensaje claro si no está;
  ASP.NET Core resuelve `ConnectionStrings__Esba` desde el entorno sin nada extra.

- **DataProtection persistido** (`Program.cs`): `SetApplicationName("Esba")` siempre;
  si `DataProtection__KeysPath` está definida, las claves se persisten ahí
  (volumen del stack). **En Production sin esa variable el arranque falla** a
  propósito: sin claves persistidas, cada redeploy invalida la cookie de auth y el
  estado de los circuitos (⚠️ del roadmap). En Development no hace falta (default del
  perfil del usuario).

### Variables de entorno del stack de Portainer

| Variable | Obligatoria | Ejemplo / default | Para qué |
|---|---|---|---|
| `ConnectionStrings__Esba` | **Sí** | `database=firebird:/datos/esba.gdb;user=esba_app;password=***;charset=ISO8859_1` | Conexión a Firebird (host según decisión pendiente de 12.6: contenedor del stack o servidor externo). Usuario de mínimos privilegios: 12.2. |
| `DataProtection__KeysPath` | **Sí** (el arranque falla sin ella) | `/keys` (volumen persistente) | Claves de cifrado de cookie de auth + circuitos Blazor. |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` (default) | Entorno. |
| `Smtp__Host` | Para enviar correo | `smtp.gmail.com` | Servidor SMTP del correo por comisión. |
| `Smtp__Port` | No | `587` | Puerto SMTP. |
| `Smtp__Security` | No | `StartTls` (`None`/`StartTls`/`SslOnConnect`) | Seguridad de la conexión. |
| `Smtp__From` | Para enviar correo | `secretaria@esba.edu.ar` | Remitente institucional. |
| `Smtp__FromDisplayName` | No | (ya viene de appsettings) | Nombre visible. |
| `Smtp__User` | Para enviar correo | — | Usuario SMTP. **Secreto.** |
| `Smtp__Password` | Para enviar correo | — | Contraseña SMTP. **Secreto.** |
| `Institucion__*` | No | (defaults de appsettings.json) | Datos del membrete/constancias; solo si difieren del default. |

Volúmenes mínimos del stack: el de `DataProtection__KeysPath` (`/keys`) y el de logs
(12.5). Dockerfile y stack: 12.6.

## 12.2 — Usuario Firebird de mínimos privilegios ⬜

## 12.3 — Autorización por políticas (`MNUOPC` → `[Authorize(Policy=…)]`) 🔴 ⬜

## 12.4 — Sesión única en middleware 🟡 ⬜

## 12.5 — Serilog + manejo global de excepciones 🔴 ⬜

## 12.6 — Dockerfile multi-stage + stack Portainer 🔶 parcial

- **`Dockerfile`** (raíz del repo, 2026-07-02): multi-stage sobre `sdk:10.0` →
  `aspnet:10.0`. Decisiones:
  - Restore como capa separada (solo los `.csproj` + `Directory.Build.props`) para
    cachear dependencias.
  - El runtime instala `libfontconfig1` + `fonts-liberation`: QuestPDF (SkiaSharp)
    resuelve la "Arial" de los reportes vía fontconfig → Liberation Sans
    (métricamente compatible). Sin esto los PDFs revientan en el contenedor.
  - `DataProtection__KeysPath=/keys` ya viene como default de la imagen; `/keys` se
    crea con owner del usuario no-root (`$APP_UID`, UID 1654) y se monta como volumen.
  - Corre como usuario **no-root** en el puerto **8080** (default de la imagen).
  - `.dockerignore` deja pasar solo `src/`, `Directory.Build.props` y
    **`.editorconfig`** (el legacy Delphi no viaja al contexto). El `.editorconfig`
    es obligatorio: sin él los analizadores corren con otra config (CA1716 sobre el
    namespace `Components.Shared`) y `TreatWarningsAsErrors` rompe el publish.
- **Verificado 2026-07-02** (podman, imagen 364 MB):
  - Sin `ConnectionStrings__Esba` → falla rápido: "Falta la cadena de conexión 'Esba'". ✅
  - Con la cadena apuntando al Firebird del host (`host.containers.internal` en
    podman; en Docker/Portainer es `172.17.0.1` o `host.docker.internal`) → sirve
    `/login` (200) y un POST de login con antiforgery válido consulta `USUARIOS` y
    devuelve el error esperado de credenciales — conectividad a la base confirmada
    de punta a punta. ✅
  - Sin volumen en `/keys` la app igual arranca (el path existe en la imagen) pero
    loguea el warning de claves no persistidas: **el stack debe montar el volumen**.
- **Pendiente**: stack de Portainer (compose nuevo — el `docker_compose.yml` de la
  raíz es del proyecto .NET viejo: imagen `alumnos.dotnet_1.1`, env vars
  `MailConfiguracion__*`/`ConnectionStrings__DefaultConnection` que este sistema no
  lee). Dato a conservar de ese archivo: Firebird corre en el **host** y el
  contenedor le llega por `172.17.0.1:3050`.
- **Pendiente**: revisar `UseHttpsRedirection`/HSTS detrás del proxy (el contenedor
  sirve HTTP plano en 8080; TLS termina afuera).
