# Mejoras Arquitectónicas — Migración ESBA a .NET + Blazor

> Documento normativo para el nuevo desarrollo. Define las **reglas de juego obligatorias** para la reescritura del sistema ESBA (gestión académica) desde Delphi XE2 + VCL + Firebird hacia una **aplicación web moderna .NET**.
> Complementa a [delphi_structure.md](./delphi_structure.md), que documenta el sistema legacy.

**Convención de este documento:**
- 🔴 **OBLIGATORIO** — regla de cumplimiento estricto; un PR que la viole no se mergea.
- 🟡 **RECOMENDADO** — default del proyecto; apartarse requiere justificación escrita en el PR.
- ⚪ **OPCIONAL** — a criterio del desarrollador.

---

## 1. Arquitectura Destino

### 1.1 Stack tecnológico

| Capa | Tecnología | Justificación |
|---|---|---|
| Runtime | **.NET 8 LTS** (o superior LTS vigente al iniciar) | Soporte largo, multiplataforma (el legacy era solo Win32). |
| Frontend | **Blazor Web App** (modo `InteractiveServer` como default) | Un solo lenguaje (C#) en todo el stack; render server-side mantiene la lógica y la conexión a Firebird en el servidor, igual que el modelo cliente/servidor actual pero sin distribuir ejecutables. |
| Base de datos | **Firebird 4/5** (migrar el `esba.gdb` legacy con `gbak` + upgrade de ODS) | Se conserva el motor para no migrar datos y esquema a la vez. La migración de datos es un proyecto separado. |
| ORM principal | **Entity Framework Core** con provider `FirebirdSql.EntityFrameworkCore.Firebird` | CRUD, change-tracking, LINQ tipado, migraciones de esquema. |
| Micro-ORM de lectura | **Dapper** (mismo `FbConnection`) | Listados, reportes y llamadas a SP `XXX_*` donde EF Core agrega overhead sin beneficio. |
| Validación | **FluentValidation** | Ver §2.4. |
| Reportes PDF | **QuestPDF** | Reemplaza Gnostice eDocEngine + dibujo GDI manual. Genera PDF en servidor, se descarga/previsualiza en el navegador. |
| Exportación Excel | **ClosedXML** | Reemplaza la automatización OLE de `FuncionesExcel.pas`. No requiere Excel instalado. |
| Correo | **MailKit** | Reemplaza Indy (`IdSMTP` + OpenSSL). |
| Logging | **Serilog** (sink a archivo + consola) | Reemplaza el `Application.OnException` global del DataModule. |
| Autenticación | **ASP.NET Core Identity** adaptado a la tabla `USUARIOS` (o cookie auth custom) | Reemplaza el login de `sesion.pas` y el cifrado reversible `EncriptoCadena2`. Ver §2.7. |

### 1.2 Estructura de solución (Clean Architecture)

🔴 **OBLIGATORIO** — la solución se organiza en cuatro proyectos con dependencias en una sola dirección (hacia el dominio):

```
Esba.sln
│
├── src/
│   ├── Esba.Domain/             ← sin dependencias externas (ni EF, ni Blazor)
│   │   ├── Entities/            (Alumno, Materia, Cursada, Comision, MesaExamen,
│   │   │                         Docente, Usuario, PermisoExamen, Analitico, ...)
│   │   ├── Enums/               (EstadoAlumno, TipoExamen, Cuatrimestre, ...)
│   │   ├── ValueObjects/        (Dni, LibroMatriz, Promedio, ...)
│   │   └── Common/              (Result<T>, OperationStatus, DomainException)
│   │
│   ├── Esba.Application/        ← depende solo de Domain
│   │   ├── Abstractions/        (IAlumnoRepository, IReportePdfService,
│   │   │                         IExcelExportService, IEmailService, IUnitOfWork)
│   │   ├── Features/            (casos de uso agrupados por área funcional:
│   │   │                         Alumnos/, Academica/, Asistencias/, Examenes/,
│   │   │                         Certificados/, Administracion/)
│   │   ├── Validators/          (FluentValidation, uno por comando/DTO)
│   │   └── DTOs/
│   │
│   ├── Esba.Infrastructure/     ← depende de Application + Domain
│   │   ├── Persistence/         (EsbaDbContext, configuraciones de entidad,
│   │   │                         repositorios EF Core)
│   │   ├── Queries/             (lecturas Dapper para listados y reportes)
│   │   ├── StoredProcedures/    (wrappers tipados de los SP XXX_*)
│   │   ├── Reports/             (QuestPDF)
│   │   ├── Excel/               (ClosedXML)
│   │   └── Email/               (MailKit)
│   │
│   └── Esba.Web/                ← Blazor Web App; depende de Application
│       │                          (e Infrastructure solo en Program.cs para DI)
│       ├── Components/
│       │   ├── Layout/          (shell, menú por carrera, login)
│       │   ├── Shared/          (componentes genéricos reutilizables — ver §3.3)
│       │   └── Pages/           (una carpeta por área funcional, espejo de
│       │                         Application/Features)
│       └── Program.cs           (composición de DI, auth, middleware)
│
└── tests/
    ├── Esba.Domain.Tests/
    ├── Esba.Application.Tests/
    └── Esba.IntegrationTests/   (contra Firebird en contenedor o BD de prueba)
```

**Reglas de dependencia (🔴 OBLIGATORIO):**
1. `Esba.Domain` no referencia ningún paquete NuGet de infraestructura. Cero `using FirebirdSql...`, `using Microsoft.EntityFrameworkCore...`.
2. `Esba.Application` define **interfaces**; `Esba.Infrastructure` las implementa. La capa web nunca instancia infraestructura directamente: todo entra por DI.
3. Ningún componente `.razor` referencia `EsbaDbContext`, `FbConnection` ni SQL. Los componentes solo hablan con servicios/casos de uso de `Esba.Application`.
4. Está prohibido el equivalente moderno del "god datamodule" `TCustomerData`: **no existe ninguna clase estática ni singleton con la conexión compartida**. Las conexiones viven en el scope del request/circuito y las administra el contenedor de DI.

### 1.3 Acceso a datos: EF Core + Dapper (estrategia híbrida)

El legacy mezcla SQL dinámico por concatenación (`FuncionesDB.ExecScript`), datasets en memoria (kbmMemTable) y SP seleccionables `XXX_*`. La estrategia destino separa por tipo de operación:

| Operación | Herramienta | Regla |
|---|---|---|
| CRUD transaccional (ABM de alumnos, materias, comisiones, usuarios, docentes…) | **EF Core** | 🔴 Toda escritura pasa por el `DbContext` con entidades tipadas. Reemplaza los `ExecScript` con strings concatenados. |
| Listados, grillas, reportes (el grueso de `modulovariable` y `Impresiones.pas`) | **Dapper** con `AsNoTracking` conceptual (proyección directa a DTOs de lectura) | 🟡 Lecturas de solo display no pasan por el change tracker. SQL parametrizado siempre. |
| Stored procedures `XXX_*` | **Dapper** (`SELECT * FROM XXX_PROC(@p1, @p2)`) envuelto en clases tipadas en `Infrastructure/StoredProcedures/` | 🔴 Cada SP legacy se envuelve en un método C# con parámetros y DTO de retorno tipados. Prohibido invocar SP con SQL crudo desde Application o Web. |
| Transacciones multi-paso | `IUnitOfWork` sobre la transacción de EF Core (compartiendo conexión con Dapper vía `DbContext.Database.GetDbConnection()`) | 🔴 Una transacción por caso de uso, abierta y cerrada en la capa Application. Nunca transacciones de larga vida como las ~15 del DataModule legacy. |

**Reglas adicionales de datos:**

- 🔴 **SQL injection erradicado**: prohibida toda concatenación de strings para construir SQL. El legacy concatena `.Text` de controles directamente (ej. `FrmEsba.ModificarAlumnoClick`); en el nuevo sistema todo valor viaja como parámetro (`@param` en Dapper, LINQ en EF Core). Un PR con SQL interpolado se rechaza.
- 🔴 **El patrón `ERRCOD`/`ERRMSG` de los SP se modela como `Result<T>`** en `Esba.Domain.Common`:
  ```csharp
  public sealed record Result<T>
  {
      public OperationStatus Status { get; init; }   // Ok | Warning | NeedsConfirmation | Error
      public string? Message { get; init; }
      public T? Value { get; init; }
  }
  ```
  El mapeo respeta la semántica legacy: `2` → `Error` (rollback), `1` → `NeedsConfirmation` (la UI pregunta antes de confirmar), `0` con mensaje → `Warning` (continúa), sin código → `Ok`. Así la lógica que hoy vive en `ExecScriptMsg` queda explícita y testeable.
- 🟡 **Lógica de los SP `XXX_*`**: en la fase 1 se conservan y se invocan vía wrapper (riesgo mínimo). En fases posteriores, la lógica de cada SP se porta a C# (Domain/Application) con tests de equivalencia, y el SP se retira. Cada wrapper lleva un comentario `// TODO-migrar` con prioridad.
- 🔴 **Esquema versionado**: el DDL de Firebird (extraído con `isql -x` de la base real, ya que hoy no está en el repo) se incorpora al repositorio desde el día 1, y todo cambio posterior se hace con **migraciones de EF Core**. Nunca más esquema solo en producción.
- 🔴 **Connection string en `appsettings.json`** + user secrets / variables de entorno en producción. Se elimina el patrón `Esba_cnf.ini` con `sysdba/masterkey` en texto plano. La cuenta de aplicación tiene su propio usuario Firebird con permisos mínimos (no `SYSDBA`).
- ⚪ Identificadores: confirmar contra el DDL real si existen generadores; preferir `SEQUENCE`/generators de Firebird mapeados en EF Core sobre el `GenID` cliente del legacy.

### 1.4 Diagrama de flujo de una operación

```
[Componente .razor]
   │  bind + submit
   ▼
[Caso de uso en Esba.Application]  ── FluentValidation (falla → errores a la UI)
   │
   ├── lectura → [Query Dapper]      → DTO de lectura
   └── escritura → [Repositorio EF]  → entidades Domain → SaveChanges (transacción)
   │
   ▼
Result<T>  → el componente lo traduce a Snackbar / diálogo de confirmación / refresh
```

---

## 2. Buenas Prácticas de Código (erradicación de vicios legacy)

### 2.1 Separación de lógica de negocio y UI

**Vicio legacy:** los formularios VCL contienen SQL, reglas de negocio, formateo e impresión en los handlers `OnClick` (ej. `FrmEsba.pas` con búsqueda, carga de padrón, permisos y apertura de 60 formularios en una sola unidad).

🔴 **Reglas:**
1. Un componente `.razor` solo puede contener: markup, binding, estado de presentación (qué tab está activa, qué fila está seleccionada) y llamadas a servicios de Application. **Nada de reglas de negocio, SQL, cálculos de dominio ni armado de reportes.**
2. Si un `@code` supera ~150 líneas o contiene lógica condicional de negocio, se extrae a un caso de uso en Application o a un componente hijo.
3. Cálculos de dominio (promedio del analítico, cuatrimestre a letras, vencimientos, validación de DNI/libro matriz) viven en `Esba.Domain` como métodos de entidad o servicios de dominio puros, **con tests unitarios**. Son los sucesores de `FuncionesText`, `FuncionesVariant` y de la lógica dispersa en formularios.
4. La regla de los tres lugares: si un dato se valida/calcula igual en dos pantallas, la tercera aparición obliga a moverlo a Domain/Application (en el legacy, esto produjo las "Funciones*" globales y código duplicado tipo `constanciaalumnos` vs `constanciaalumnos2`).

### 2.2 Async/await obligatorio

**Vicio legacy:** UI congelada salvo el único hack de `TFISQLThread` para cargar el padrón de alumnos.

🔴 **Reglas:**
1. **Toda operación de I/O es asíncrona de punta a punta**: métodos de repositorio, queries Dapper (`QueryAsync`), EF Core (`SaveChangesAsync`, `ToListAsync`), generación de PDF, envío de mail.
2. Prohibido `Task.Result`, `.Wait()`, `.GetAwaiter().GetResult()` y `async void` (excepto event handlers donde Blazor lo exige). Estos patrones causan deadlocks y son el equivalente moderno de congelar la UI.
3. Los métodos asíncronos terminan en sufijo `Async` y aceptan `CancellationToken` desde la capa Application hacia abajo.
4. Operaciones largas (carga del padrón completo, exportaciones masivas, envío de correo por comisión) muestran indicador de progreso en la UI y no bloquean el circuito; las realmente pesadas (mail masivo) se despachan a un servicio en background (`IHostedService` / canal de trabajo) con notificación al usuario al terminar.

### 2.3 Inyección de Dependencias

**Vicio legacy:** estado global (`CodUsu`, `Superv`, `Rector` en `FuncionesConfiguracion.pas`), singleton `CustomerData` accedido desde todos los formularios, funciones globales sueltas.

🔴 **Reglas:**
1. **Cero estado global mutable.** Ni clases estáticas con estado, ni singletons de negocio.
2. Todo servicio se registra en `Program.cs` y se recibe **por constructor** (o `@inject` en componentes). Lifetimes:
   - `DbContext` y repositorios → **Scoped** (en Blazor Server, usar `IDbContextFactory<EsbaDbContext>` para evitar contextos compartidos entre renders concurrentes del mismo circuito).
   - Servicios sin estado (validadores, generadores de PDF/Excel, email) → **Scoped** o **Transient**.
   - Configuración (`IOptions<SmtpSettings>`, etc.) → patrón Options sobre `appsettings.json`. Reemplaza `Esba_cnf.ini` / `CnfMail.pas` / `TablaConfiguraciones`.
3. El usuario logueado y sus flags (`Superv`, `Rector`, `Secretaria`) se modelan como **claims** del `ClaimsPrincipal` y se consultan vía `AuthenticationStateProvider` / `[Authorize]`, nunca como variables globales.
4. Las dependencias se declaran contra **interfaces** de `Esba.Application.Abstractions`; los componentes y casos de uso no conocen las clases concretas de Infrastructure.

### 2.4 Validaciones con FluentValidation

**Vicio legacy:** validaciones repartidas entre handlers de formularios, `FuncionesText`, y constraints de Firebird traducidos a posteriori por `uMensajesError.pas` con un diccionario `.ini`.

🔴 **Reglas:**
1. Cada comando/DTO de escritura tiene **exactamente un validador** `AbstractValidator<T>` en `Esba.Application/Validators/`. La validación corre **siempre en el servidor** dentro del caso de uso; el binding de UI puede reusarla para feedback inmediato, pero la UI nunca es la única barrera.
2. Las validaciones se escriben **antes** de tocar la base: formato de DNI, rangos de notas (1–10), fechas de mesa dentro del cuatrimestre, unicidad consultada vía repositorio. La violación de constraint de BD pasa a ser el **último recurso** (defensa en profundidad), no el mecanismo principal como en el legacy.
3. Las violaciones de FK/constraint que igual lleguen desde Firebird se interceptan en un punto único de Infrastructure y se traducen a `Result` con mensaje amigable — sucesor del diccionario de `uMensajesError.pas`, pero versionado en código (no en `.ini`).
4. Mensajes de validación en castellano, centralizados, asociados al campo (FluentValidation `WithName`/`WithMessage`) para que la UI los muestre junto al control correspondiente.

### 2.5 Manejo de errores y logging

- 🔴 Excepciones no controladas: middleware/`ErrorBoundary` global que loguea con Serilog (con `CorrelationId` por request/circuito) y muestra un error genérico. Nunca el stack trace al usuario. Reemplaza `Application.OnException` del DataModule.
- 🔴 Errores **esperables de negocio** (validación, ERRCOD de SP) viajan como `Result<T>`, no como excepciones. Las excepciones quedan para lo excepcional.
- 🟡 Auditoría: las operaciones sensibles (cambio de DNI/libro matriz, notas, regularizaciones — hoy `LOG_CURSADA`) registran usuario, fecha y valores anteriores mediante interceptor de EF Core, generalizando el patrón de log parcial del legacy.

### 2.6 Calidad transversal

- 🔴 `Nullable` habilitado (`<Nullable>enable</Nullable>`) y `TreatWarningsAsErrors` en todos los proyectos.
- 🔴 `.editorconfig` compartido con las convenciones estándar de .NET (PascalCase público, `_camelCase` para campos privados, `var` cuando el tipo es evidente). Analizadores (`Microsoft.CodeAnalysis.NetAnalyzers`) en nivel `latest-recommended`.
- 🔴 Tests: todo servicio de dominio y caso de uso con lógica condicional lleva tests unitarios (xUnit + NSubstitute/Moq). La paridad funcional con el legacy se verifica con tests de integración contra una base Firebird de prueba (los SP `XXX_*` se testean comparando salida vieja vs wrapper nuevo).
- 🟡 Sin código muerto desde el inicio: los artefactos identificados como obsoletos en el legacy (`MessajeError.pas`, `FuncionesCuotas.pas`, backups `bkp/`) **no se migran**. Lo que no esté referenciado, no entra.
- 🟡 Idioma: entidades y conceptos de dominio en español (coinciden con la BD y el vocabulario institucional: `Alumno`, `Cursada`, `MesaExamen`); infraestructura y patrones técnicos en inglés (`Repository`, `Validator`, `Result`).

### 2.7 Seguridad (correcciones obligatorias respecto del legacy)

| Problema legacy | Regla destino |
|---|---|
| Contraseñas con cifrado reversible casero (`EncriptoCadena2`) | 🔴 Hash con **PBKDF2/bcrypt** vía ASP.NET Core Identity (o `PasswordHasher<T>`). Migración: re-hash en el primer login exitoso de cada usuario. |
| Credenciales `sysdba/masterkey` en `.ini` plano | 🔴 Usuario Firebird dedicado de mínimos privilegios; secretos fuera del repo (variables de entorno / user secrets). |
| SQL por concatenación → inyección | 🔴 Ver §1.3 — solo SQL parametrizado/LINQ. |
| Permisos por usuario en `BARRA_SEGU` chequeados ad-hoc en la UI | 🔴 **Autorización por políticas** (`[Authorize(Policy = "...")]`) evaluada en el servidor por área funcional/carrera; el menú se filtra por las mismas políticas (la UI oculta, el servidor **deniega**). |
| Sesión única por usuario (`seciones.pas` con UID en `USUARIOS.UID`) | 🟡 Conservar la regla de negocio si la institución la requiere: token de sesión por usuario validado en el middleware de autenticación; el login nuevo invalida el circuito anterior con aviso. |

---

## 3. Modernización de UI/UX

### 3.1 Framework de componentes

🟡 **RECOMENDADO: MudBlazor** como librería única de componentes.

Razones frente a las alternativas:
- **MudBlazor**: open-source, Material Design, `MudDataGrid` con sort/filtro/paginación/edición inline (cubre DBGrid + kbmMemTable + DevExpress de una vez), `MudDialog`, `MudAutocomplete`, theming centralizado. Comunidad grande y sin costo de licencia — relevante tras la experiencia legacy de quedar atado a FIBPlus/Gnostice/kbmMemTable comerciales.
- **Radzen Blazor**: alternativa válida (grid y reportes fuertes); elegirla solo si en el spike inicial `MudDataGrid` no cubre algún caso de grilla complejo.
- **Tailwind CSS**: no es una librería de componentes; ⚪ puede usarse para ajustes finos de layout **además** de MudBlazor, pero no como reemplazo (implicaría construir grillas y diálogos a mano).

🔴 **Regla:** una sola librería de componentes en todo el proyecto. Prohibido mezclar MudBlazor + Radzen + Bootstrap ad-hoc — el legacy ya mostró el costo de acumular DevExpress + RxLib + VCL Styles + fixes manuales.

### 3.2 Del shell VCL al layout web

| Concepto legacy | Equivalente destino |
|---|---|
| `FrmEsba` (MDI-like, barra dxBar dinámica persistida en `.bar` por usuario) | `MainLayout.razor` con `MudAppBar` + `MudDrawer`: **menú general por área funcional** (Alumnos, Académica, Asistencias, Exámenes, Certificados, Administración — espejo de `Application/Features`), con opciones habilitadas según permisos. ⚠️ *Decisión 2026-06-12: NO agrupar por carrera — ese diseño existió en el legacy y fue descartado por molesto con muchas carreras. El centro de la pantalla es el buscador global de alumnos (sin importar carrera) con acciones sobre el alumno seleccionado; la carrera se elige al entrar a cada opción que la necesite (la lista permitida viene de `YYY_BARRA_SEGU(codusu, programa)`, que se envuelve como wrapper en Etapa 2.B).* La personalización por usuario (`.bar`/`.not`) se guarda en BD como preferencias, no en archivos locales. |
| Formularios modales (Busqueda, MensajeError, confirmaciones) | `MudDialog` / `IDialogService`. El flujo `NeedsConfirmation` del `Result<T>` (§1.3) se resuelve con un diálogo de confirmación estándar único para toda la app. |
| Navegación abriendo formularios hijos desde botones | Rutas Blazor (`@page "/alumnos/{Id}"`) — cada pantalla tiene URL: linkeable, navegable con atrás/adelante, abrible en otra pestaña. |
| Skins VCL (`.vsf`) | `MudTheme` centralizado (paleta institucional, modo claro/oscuro). Un solo lugar define colores y tipografía. |
| Grilla principal de alumnos cargada en thread a kbmMemTable | `MudDataGrid<AlumnoListItemDto>` con **`ServerData` (paginación y filtro en servidor vía Dapper)** — no se carga el padrón completo en memoria; la búsqueda global pasa a ser un `MudAutocomplete` con búsqueda incremental server-side. |

### 3.3 Componentes genéricos reutilizables (sucesores de `Modulo Variable/`)

El legacy ya tenía la idea correcta (listado genérico + parámetros dinámicos); se moderniza así, en `Esba.Web/Components/Shared/`:

| Legacy | Componente destino | Descripción |
|---|---|---|
| `modulovariable.pas` (listado genérico + Excel + imprimir) | `EsbaListView<TItem>` | Componente genérico: `MudDataGrid` server-side + toolbar estándar con **Exportar Excel** (ClosedXML) y **PDF** (QuestPDF). Toda pantalla "Listado de…" lo instancia con su query y columnas. |
| `parametros.pas` (filtros generados en runtime desde un array) | `EsbaFilterPanel` | Panel de filtros declarativo: recibe una definición tipada (campo, tipo, origen de combo) y renderiza los controles. Los combos cargan por servicio, no por SQL embebido. |
| `modulovariable_grf.pas` (grilla + TeeChart) | `EsbaChartView` | `MudChart` (o ApexCharts.Blazor si se necesita cross-tab complejo) junto al `EsbaListView`. |
| `Busqueda.pas` (diálogo de búsqueda en grilla) | `EsbaLookupDialog<TItem>` / `MudAutocomplete` | Para selección de alumno/materia/docente: autocomplete server-side como primera opción; diálogo de búsqueda avanzada para casos complejos. |
| `Imprimir.pas` (TGmPreview) | Vista previa nativa del navegador | Los reportes QuestPDF se devuelven como PDF inline (`Content-Disposition: inline`): el navegador es el visor. Sin componente de preview propio. |

🔴 **Regla:** antes de crear una pantalla de listado/reporte nueva, se usa `EsbaListView` + `EsbaFilterPanel`. Crear una grilla a medida requiere justificación en el PR.

### 3.4 Diseño responsivo y UX

- 🔴 **Responsive obligatorio**: todas las pantallas usan el grid de MudBlazor (`MudGrid`/`MudItem` con breakpoints `xs/sm/md/lg`). Objetivo mínimo: uso cómodo en desktop (caso principal: administración) y tablet (caso real: carga de asistencias/notas en aula). Las pantallas fijas en píxeles del VCL no tienen equivalente: nada de tamaños absolutos.
- 🔴 **Formularios**: `MudForm`/`EditForm` con los mensajes de FluentValidation mostrados junto a cada campo en el momento (no un `MessageDlg` al final, como el legacy). Botón de guardado deshabilitado durante el submit + indicador de progreso.
- 🟡 **Feedback no bloqueante**: confirmaciones de éxito con `Snackbar`; los diálogos modales se reservan para decisiones (`NeedsConfirmation`) y errores bloqueantes — traducción directa de la semántica `ERRCOD` 0/1/2.
- 🟡 **Operaciones masivas** (regularización por comisión, permisos masivos, mail por comisión): patrón de selección con checkboxes en la grilla + barra de acciones + resumen previo ("se aplicará a N alumnos") + resultado detallado por fila (qué falló y por qué), en lugar del procesamiento opaco del legacy.
- 🟡 **Accesibilidad**: labels asociados a inputs, navegación por teclado en grillas y formularios (los usuarios administrativos vienen de VCL y dependen del teclado), contraste AA.
- ⚪ Foto del alumno, notas adjuntas y demás binarios: almacenamiento en disco del servidor o BLOB según volumen, servidos por endpoint autenticado.

### 3.5 Mapa de migración de dependencias

Resumen de reemplazos (toda la columna legacy queda **prohibida** en el nuevo código):

| Legacy (comercial/Win32) | Destino (NuGet, multiplataforma) |
|---|---|
| FIBPlus | `FirebirdSql.Data.FirebirdClient` + EF Core provider + Dapper |
| kbmMemTable | DTOs + paginación server-side (no hay "dataset en memoria") |
| DevExpress ExpressBars / dxNavBar | MudBlazor (`MudAppBar`, `MudDrawer`, `MudNavMenu`) |
| RxLib (editores fecha/moneda) | `MudDatePicker`, `MudNumericField` |
| Gnostice eDocEngine + GDI manual | QuestPDF |
| TeeChart | MudChart / ApexCharts.Blazor |
| Excel OLE (`ExcelXP`) | ClosedXML |
| Indy SMTP | MailKit |
| VCL Styles (`.vsf`) | MudTheme |
| `.ini` (`Esba_cnf.ini`, `Esba_prg.ini`) | `appsettings.json` + Options + preferencias de usuario en BD |
| UPX | N/A (deploy web: publish + reverse proxy) |

---

## 4. Orden de trabajo sugerido (fases)

1. **Fundaciones**: solución con los 4 proyectos, conexión a Firebird de prueba, extracción y versionado del DDL real (`isql -x`), scaffolding de entidades core (`ALUMNOS`, `CARRERA`, `MATERIAS`, `CURSADA`, `USUARIOS`), autenticación con re-hash de contraseñas, shell de navegación con permisos.
2. **Vertical slice de referencia**: migrar completa una sola área (sugerida: **ABM de Alumnos**, sucesora de `FrmAltaAlumno` + grilla principal) aplicando todas las reglas de este documento. Esa slice queda como patrón canónico para el resto.
3. **Componentes genéricos**: `EsbaListView`, `EsbaFilterPanel`, `EsbaLookupDialog`, export Excel/PDF — desbloquean en cadena todas las pantallas "Listado de…".
4. **Áreas funcionales** en orden de dependencia: Académica (materias/comisiones/cursada) → Asistencias → Exámenes/mesas/notas → Constancias y certificados → Administración (usuarios, docentes, configuración, correo).
5. **Retiro de SP `XXX_*`**: portar la lógica PSQL a C# con tests de equivalencia, un SP por vez.

Cada fase termina con la funcionalidad verificada contra el comportamiento del sistema Delphi en paralelo (misma base de datos durante la transición).

---

*Documento vivo: toda excepción a una regla 🔴 requiere actualizar este archivo con la decisión y su justificación.*
