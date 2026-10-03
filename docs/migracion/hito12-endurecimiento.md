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

### 12.3.0 — Perfiles de acceso: secretaría / docente / alumno 🔶 parcial (2026-10-02)

**Decisión** (2026-10-02): la aplicación tendrá tres perfiles de usuario. Se resuelven
con **un solo login y una sola cookie**, un claim de **tipo** en la identidad y, en el
siguiente incremento, **áreas de URL distintas** (`/` secretaría, `/docente`, `/portal`)
con su propio layout y su propia policy. El redirect post-login es solo UX: **el servidor
decide por policy en la URL**. Descartados: "redirect con las mismas URLs" (vicio tipo
`Superv` global desparramado en los `.razor`) y sitios/logins separados (duplican
Application, DataProtection, re-hash, sesión única).

**Identidad**: una fila en `USUARIOS` por persona que se loguea, cualquiera sea su perfil.
Migración `db/schema/migrations/2026-10-02_usuarios_tipo_vinculo.sql` (aplicada en dev y
producción el 2026-10-02):

| Columna | Tipo | Significado |
|---|---|---|
| `TIPO` | `CHAR(3) NOT NULL DEFAULT 'SEC'` | `SEC` secretaría (filas previas), `DOC` docente, `ALU` alumno |
| `CODPROFES` | `CHAR(3)` | docente vinculado (`DOCENTES.CODPROFES`), obligatorio si `TIPO='DOC'` |
| `ALU_CARRE` / `ALU_COD_ALU` | `VARCHAR(6)` / `CHAR(11)` | alumno vinculado (PK de `ALUMNOS`), obligatorios si `TIPO='ALU'` |

Sin FK ni CHECK (el Delphi sigue escribiendo la tabla): la regla tipo↔vínculo la aplica el
validador y el handler verifica existencia/actividad en `DOCENTES`/`ALUMNOS` y que un
docente tenga a lo sumo **un** usuario activo.

**Hecho en este incremento:**

- Dominio: `TipoUsuario` + `TipoUsuarioCodigo` (única correspondencia enum ↔ `CHAR(3)`;
  blanco = secretaría, desconocido = excepción); `Usuario.Tipo/CodigoDocente/AlumnoCarrera/
  AlumnoCodigo` y `Usuario.UsaEscritorio` (= secretaría).
- Application: `CrearUsuarioCommand`/`ActualizarUsuarioCommand` con tipo y vínculo;
  reglas compartidas `TipoYVinculoUsuarioRules` (solo secretaría puede ser supervisor);
  `VinculoUsuario.ResolverAsync` (normaliza, descarta el vínculo que no corresponde al
  tipo, verifica); `IUsuarioRepository.ExisteVinculoDocenteAsync`; `SesionIniciadaDto`
  expone tipo y vínculo.
- **Convivencia con el escritorio Delphi** (`PasswordEscritorio`): el Delphi valida
  `PASSWD` sin mirar `TIPO`, así que docentes y alumnos reciben en `PASSWD` un sentinela
  `#WEB#` + 32 caracteres aleatorios por usuario (indescifrable a nada tipeable) y
  **nunca** se les sincroniza la contraseña real; solo secretaría sincroniza `PASSWD`
  (alta, cambio, blanqueo y re-hash del login). Cambio de tipo: secretaría→docente
  bloquea `PASSWD` de inmediato; docente→secretaría no puede reconstruir `PASSWD`
  (solo hay hash) → fuerza `CAMPASS='S'` y devuelve `Warning`; el próximo cambio de
  contraseña lo sincroniza.
- Web: `UsuarioFormDialog` con selector de tipo (Alumno visible pero deshabilitado hasta
  el portal), autocomplete de docente (`IDocentesQuery.ListarActivosAsync`) y switch de
  supervisor solo para secretaría; grilla de usuarios con columnas Tipo/Vinculado a y
  filtro por tipo (`UsuariosQuery` hace `LEFT JOIN DOCENTES`). Claims `esba:tipo`,
  `esba:docente`, `esba:alumno` + extensiones `TipoDeUsuario()`/`CodigoDocente()`.
- **Compuerta temporal** en `POST /auth/login`: solo entra `TIPO='SEC'`; docentes y
  alumnos reciben "El acceso para docentes y alumnos todavía no está habilitado". Sin
  ella, un docente recién dado de alta vería todas las pantallas de secretaría (la UI aún
  no filtra por tipo y el servidor aún no deniega). Se retira al cerrar 12.3.1.
- Tests: unitarios de validadores/handlers/login/`PasswordEscritorio`/`TipoUsuarioCodigo`;
  integración `UsuarioTipoRoundtripTests` (EF ↔ Dapper, unicidad del vínculo, cambio de
  tipo) contra Firebird real.

### 12.3.1 — Policies por perfil, shell por perfil y área `/docente` ✅ 2026-10-03

- **Policies** en `EsbaPolicies.Configurar` (registradas con `AddAuthorization`):
  `Secretaria` (por aserción sobre `TipoDeUsuario()`: una cookie emitida antes del claim
  `esba:tipo` sigue valiendo como secretaría, igual que las filas previas a la migración),
  `Docentes` y `Alumnos` (por `RequireClaim`), `Supervisores` sin cambios.
- **Aplicación por carpeta** (`_Imports.razor`): `Pages/{Academica,Administracion,Alumnos,
  Asistencias,Certificados,Examenes,Ministerio}` exigen `Secretaria`; `Pages/Docente`
  exige `Docentes`; `Home` (`/`) exige `Secretaria` por atributo. Los endpoints de
  reportes (`/constancias/*`, `/actas/*`, `/asistencias/carpeta/*`, `/examenes/*/pdf`,
  `/ministerio/*`) pasan de `RequireAuthorization()` a `RequireAuthorization(Secretaria)`.
  `CambiarPassword` se movió a `Pages/Cuenta/` (compartida por todos los perfiles, solo
  `[Authorize]`).
- **Acceso denegado**: `Routes.razor` distingue en `NotAuthorized` entre sin sesión
  (→ login) y con sesión sin policy (→ `/acceso-denegado`, layout neutro, botón "Ir a mi
  inicio" según perfil); `AccessDeniedPath` apunta al mismo lugar para los endpoints.
- **Shell por perfil**: `EsbaRootLayout` (providers Mud + tema + modo oscuro, cascadea
  `TemaEstado`) → `PerfilLayout` (layout por defecto: elige `MainLayout` o
  `DocenteLayout` por el claim de tipo) → shells concretos. `MenuUsuario` y
  `BotonModoOscuro` compartidos. Páginas comunes (`/cambiar-password`, `/not-found`)
  salen con el shell del perfil sin declarar layout.
- **Login**: redirige por tipo (`/` secretaría, `/docente` docente; `CAMPASS` antes).
  Se retiró la compuerta de 12.3.0; queda una solo para alumnos (no hay portal).
- **`/docente`** (`InicioDocente`): comisiones donde es titular (`COMARM.CODPROFES`, con
  selector de período, horario, cantidad de alumnos cursando/recursando) y mesas donde
  es titular (`MESAS.TITULAR`, últimas 6 semanas por defecto, con inscriptos), ambas con
  el estado de su precarga (`DOC_CARGA_*`, hito 19) como chip. Alcance = `CODPROFES`
  del claim, nunca por URL. `IAreaDocenteQuery` (Dapper) + `CuatrimestreAnio` (dominio:
  etiqueta y orden cronológico de `CUA_ANIO`).
- **Verificación**: smoke test HTTP con usuarios temporales de ambos perfiles (login →
  redirect por tipo; secretaría entra a `/` y `/administracion/usuarios`, denegada en
  `/docente`; docente entra a `/docente` con las dos secciones renderizadas, denegado en
  `/`, `/academica/materias` y el endpoint de constancias; `/cambiar-password` sale con
  el shell de cada perfil). Integración: `AreaDocenteQueryTests`.

**Pendiente de 12.3 propiamente dicho:** policies por `MNUOPC`/carrera a partir de
`BARRA_SEGU` y filtrado del menú de secretaría por las mismas. Portal de alumnos
(`/portal`): futuro; `NOMBRE VARCHAR(15)` puede quedar corto si el alumno ingresa con el
mail.

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
