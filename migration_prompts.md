# Prompts Reutilizables por Etapas — Migración ESBA (Delphi → .NET + Blazor)

> Guía de prompts listos para copiar y pegar en Claude Code durante la migración efectiva.
> Cada prompt asume que en el contexto del repositorio existen [delphi_structure.md](./delphi_structure.md) (documentación del legacy) y [migration_improvements.md](./migration_improvements.md) (reglas obligatorias del nuevo sistema), y los referencia explícitamente para que el modelo trabaje bajo esas reglas sin tener que repetirlas.

**Cómo usar esta guía:**
1. Elegí el prompt de la etapa que corresponda.
2. Reemplazá los placeholders `<ASÍ>` con los valores concretos (nombre de tabla, archivo `.pas`, etc.).
3. Pegá el prompt tal cual en la sesión de Claude Code, idealmente con los archivos fuente relevantes ya presentes en el repo (DDL extraído, fuentes Delphi en `Esba.Delphi XE2/ESBA/`).
4. Verificá el resultado contra la **checklist de aceptación** que acompaña a cada prompt antes de commitear.

**Convención de placeholders:**

| Placeholder | Ejemplo |
|---|---|
| `<TABLA>` | `ALUMNOS`, `CURSADA`, `MESAS` |
| `<ENTIDAD>` | `Alumno`, `Cursada`, `MesaExamen` |
| `<ARCHIVO_PAS>` | `Formulario Alta de alumno/FrmAltaAlumno.pas` |
| `<ARCHIVO_DFM>` | `Formulario Alta de alumno/FrmAltaAlumno.dfm` |
| `<AREA>` | `Alumnos`, `Academica`, `Asistencias`, `Examenes`, `Certificados`, `Administracion` |
| `<SERVICIO>` | `InscripcionMateriasService`, `AltaAlumnoHandler` |
| `<SP>` | `XXX_CAMBIA_DNI_LM`, `XXX_PROMEDIO_GRAL` |

---

## Etapa 1 — Modelado de Datos (Firebird → EF Core + DTOs)

### Prompt 1.A — Entidad EF Core desde una tabla Firebird

```text
Estás trabajando en la migración del sistema ESBA descrita en delphi_structure.md
(sistema legacy) y migration_improvements.md (reglas obligatorias del nuevo sistema).
Respetá estrictamente las reglas 🔴 de ese documento, en especial §1.2 (Clean
Architecture), §1.3 (acceso a datos) y §2.6 (calidad transversal).

TAREA: Modelar la tabla Firebird <TABLA> en el nuevo sistema .NET.

Entrada:
- DDL de la tabla (extraído de la base real con isql -x):
  <PEGAR AQUÍ EL DDL: CREATE TABLE, constraints, índices, triggers y generadores asociados>
- Queries de Delphi que usan esta tabla (buscalas en los fuentes bajo
  "Esba.Delphi XE2/ESBA/" con grep de "<TABLA>", especialmente en FuncionesDB,
  DataModule.pas y los formularios listados en delphi_structure.md §2.4),
  para inferir qué columnas se usan realmente, con qué semántica y qué
  combinaciones de filtros son frecuentes.

Generá:
1. La entidad de dominio en src/Esba.Domain/Entities/<ENTIDAD>.cs:
   - Nombres en español PascalCase (Alumno, no ALUMNOS); propiedades con tipos
     C# correctos (DateOnly para DATE, decimal para NUMERIC, string? según
     nullabilidad del DDL).
   - Cero referencias a EF Core o Firebird (regla 🔴 §1.2.1).
   - Si una columna codifica un conjunto cerrado de valores (estados, tipos),
     proponé un enum en Esba.Domain/Enums/ y justificalo citando dónde viste
     los valores en el código Delphi.
   - Si detectás un concepto con invariantes propias (DNI, libro matriz,
     promedio), proponé un Value Object en Esba.Domain/ValueObjects/.
2. La configuración EF Core en
   src/Esba.Infrastructure/Persistence/Configurations/<ENTIDAD>Configuration.cs
   (IEntityTypeConfiguration<T>): mapeo a la tabla/columnas físicas con
   ToTable/HasColumnName, claves, relaciones, longitudes máximas, y la
   estrategia de identidad (sequence/generator de Firebird si existe en el DDL;
   si no encontrás generador, dejá un comentario // TODO-confirmar-identidad).
3. El registro en EsbaDbContext (DbSet + ApplyConfiguration).
4. DTOs en src/Esba.Application/DTOs/<AREA>/:
   - <ENTIDAD>ListItemDto: solo las columnas que las grillas legacy muestran
     (deducilas de los SELECT de los formularios de listado).
   - <ENTIDAD>DetailDto: la ficha completa.
   - Crear<ENTIDAD>Command y Actualizar<ENTIDAD>Command: solo campos editables
     por el usuario (excluí PK, campos calculados y de auditoría).
5. Una tabla en Markdown "columna Firebird → propiedad C# → tipo → notas" para
   revisión humana, marcando con ⚠️ toda decisión dudosa (tipos ambiguos,
   columnas que parecen sin uso, posibles enums).

Restricciones:
- No inventes columnas ni relaciones que no estén en el DDL o en las queries.
- Si una FK apunta a una tabla aún no migrada, declarala con la propiedad de
  navegación comentada y un // TODO-migrar <TABLA_DESTINO>.
- Nullable enable: anotá la nullabilidad real según el DDL, no supongas.
```

**Checklist de aceptación 1.A:**
- [ ] La entidad compila en `Esba.Domain` sin ningún `using` de EF/Firebird.
- [ ] Cada columna del DDL está mapeada o explícitamente descartada con justificación.
- [ ] La identidad (generator/sequence) está confirmada contra el DDL o marcada `TODO-confirmar-identidad`.
- [ ] Los DTOs de lectura no incluyen campos que ninguna pantalla legacy muestra.

### Prompt 1.B — Query de lectura Dapper desde un SELECT de Delphi

```text
Contexto: migración ESBA según delphi_structure.md y migration_improvements.md
(§1.3: lecturas de listados van por Dapper con SQL parametrizado, proyectadas
a DTOs de lectura; prohibida toda concatenación de strings).

TAREA: Migrar a una query Dapper la consulta de listado que el legacy arma en
<ARCHIVO_PAS> (función/handler: <NOMBRE_DEL_HANDLER>).

Pasos:
1. Localizá en el fuente Delphi el SQL (suele estar concatenado en strings,
   a veces con .Text de controles intercalado — eso es el filtro dinámico).
2. Reconstruí el SQL completo y limpio para Firebird 4/5, reemplazando cada
   valor concatenado por un parámetro @nombrado. Documentá en un comentario
   qué control de UI alimentaba cada parámetro.
3. Generá:
   - El DTO de lectura en src/Esba.Application/DTOs/<AREA>/ (record, propiedades
     init-only, solo columnas del SELECT).
   - La interfaz del lado Application (en Esba.Application/Abstractions/) con el
     método XxxAsync(filtros..., CancellationToken ct).
   - La implementación Dapper en src/Esba.Infrastructure/Queries/ usando
     QueryAsync<TDto>, conexión obtenida por DI, soporte de paginación
     server-side (FIRST/SKIP u OFFSET/FETCH de Firebird) y orden estable.
4. Si la query original usaba un SP XXX_*, NO lo reescribas: envolvelo según el
   Prompt 2.B (wrapper tipado en Infrastructure/StoredProcedures/).

Validación: incluí al final el SQL final formateado y una lista de los
parámetros con su tipo, para que pueda probarlo a mano en isql.
```

**Checklist de aceptación 1.B:**
- [ ] Ningún valor de usuario viaja interpolado en el SQL.
- [ ] El método es `async`, termina en `Async` y acepta `CancellationToken`.
- [ ] La paginación se hace en el servidor Firebird, no en memoria.

---

## Etapa 2 — Lógica de Negocio (.pas / DataModule → Servicios asincrónicos)

### Prompt 2.A — Extraer lógica de un formulario/unidad Delphi a un caso de uso

```text
Contexto: migración ESBA. Leé delphi_structure.md (para entender el rol del
archivo legacy) y migration_improvements.md; aplicá obligatoriamente §1.2
(capas), §1.3 (transacciones por caso de uso, Result<T>), §2.1 (separación
UI/negocio), §2.2 (async de punta a punta), §2.3 (DI, cero estado global) y
§2.4 (FluentValidation).

TAREA: Extraer la lógica de negocio de <ARCHIVO_PAS> (clase <CLASE_DELPHI>) y
convertirla en casos de uso C# en src/Esba.Application/Features/<AREA>/.

Procedimiento:
1. ANÁLISIS PREVIO (mostralo antes de escribir código):
   - Listá cada handler/método del .pas que contenga lógica de negocio
     (validaciones, SQL de escritura, cálculos, llamadas a SP XXX_*).
   - Clasificá cada uno: [validación] / [escritura CRUD] / [lectura] /
     [llamada a SP] / [cálculo de dominio] / [solo UI - descartar].
   - Identificá dependencias del god-datamodule (CustomerData.*) y del estado
     global (CodUsu, Superv, Rector de FuncionesConfiguracion): cada una debe
     mapearse a un servicio inyectado o a claims del usuario (§2.3.3).
2. Por cada operación de escritura, generá:
   - Un Command (record) + su AbstractValidator<T> en Application/Validators/
     con TODAS las validaciones que el legacy hacía en el handler (rangos,
     obligatoriedad, formatos de FuncionesText) más las implícitas en
     constraints del DDL si lo tenés disponible.
   - Un Handler/Servicio asincrónico que: valida → ejecuta dentro de UNA
     transacción vía IUnitOfWork → devuelve Result<T> de Esba.Domain.Common.
   - Mapeo de la semántica ERRCOD/ERRMSG si interviene un SP: 2→Error,
     1→NeedsConfirmation, 0+mensaje→Warning, sin código→Ok (§1.3).
3. Los cálculos puros de dominio (promedios, cuatrimestre a letras,
   vencimientos) van a Esba.Domain como métodos de entidad o servicios de
   dominio sin dependencias, NO al handler.
4. Interfaces nuevas que necesites (repositorios, servicios de infraestructura)
   se declaran en Esba.Application/Abstractions/; implementación mínima en
   Infrastructure si no existe.
5. Entregá al final una TABLA DE TRAZABILIDAD: "método Delphi → artefacto C#
   → notas", incluyendo los métodos descartados por ser solo-UI y los puntos
   donde NO pudiste determinar el comportamiento legacy (marcalos con ⚠️ y una
   pregunta concreta, no asumas en silencio).

Prohibiciones (un PR que las viole se rechaza, ver migration_improvements.md):
- SQL por concatenación; Task.Result/.Wait(); async void; estado estático
  mutable; lógica de negocio que quede para "resolver después en el .razor".
```

**Checklist de aceptación 2.A:**
- [ ] Existe la tabla de trazabilidad método Delphi → artefacto C#.
- [ ] Cada escritura tiene exactamente un validador y devuelve `Result<T>`.
- [ ] Una sola transacción por caso de uso, abierta/cerrada en Application.
- [ ] `CodUsu`/`Superv`/etc. llegan por claims o parámetro, nunca por estado global.
- [ ] Toda I/O es `async` con `CancellationToken`.

### Prompt 2.B — Wrapper tipado de un stored procedure `XXX_*`

```text
Contexto: migración ESBA, regla 🔴 de migration_improvements.md §1.3: cada SP
legacy se envuelve en un método C# tipado en
src/Esba.Infrastructure/StoredProcedures/; prohibido invocarlo con SQL crudo
desde Application o Web. En esta fase el SP se CONSERVA (no portar su lógica).

TAREA: Crear el wrapper tipado del procedimiento <SP>.

Entrada:
- Código PSQL del SP (extraído de la base con isql -x):
  <PEGAR AQUÍ EL CREATE PROCEDURE COMPLETO>
- Sitios de llamada en Delphi: <ARCHIVOS_PAS_QUE_LO_USAN> (ver tabla de SPs en
  delphi_structure.md §3 para el propósito aparente).

Generá:
1. Un record de parámetros de entrada y un record de fila de salida, con tipos
   C# fieles a los tipos PSQL declarados.
2. La interfaz I<NombreLegible>Procedure en Esba.Application/Abstractions/
   (nombre de negocio, ej. ICambioDniLibroMatrizProcedure para XXX_CAMBIA_DNI_LM).
3. La implementación Dapper: SELECT * FROM <SP>(@p1, @p2, ...) con QueryAsync,
   CancellationToken, y conexión por DI.
4. Si el SP devuelve ERRCOD/ERRMSG (o FERRCOD/FERRMSG): mapeá la salida a
   Result<T> respetando la semántica 2/1/0 documentada en
   migration_improvements.md §1.3 — ese mapeo va en el wrapper, no en cada
   llamador.
5. Comentario // TODO-migrar con tu estimación de prioridad y un resumen de
   2-3 líneas de qué hace la lógica PSQL internamente (para la fase 5 de
   retiro de SPs).
6. Esqueleto del test de equivalencia en tests/Esba.IntegrationTests/: dado un
   set de parámetros, la salida del wrapper debe coincidir con la ejecución
   directa del SP en la base de prueba.
```

**Checklist de aceptación 2.B:**
- [ ] Parámetros y columnas de salida coinciden 1:1 con el PSQL.
- [ ] La semántica ERRCOD/ERRMSG queda encapsulada en el wrapper.
- [ ] Existe el `// TODO-migrar` con resumen de la lógica interna.

---

## Etapa 3 — Interfaz Blazor (.dfm → componente .razor)

### Prompt 3.A — Formulario de alta/edición

```text
Contexto: migración ESBA a Blazor Web App (InteractiveServer) con MudBlazor
como única librería de componentes. Leé migration_improvements.md y aplicá
obligatoriamente §2.1 (el .razor solo tiene markup, binding, estado de
presentación y llamadas a Application), §3.2 (equivalencias VCL→web), §3.4
(responsive con MudGrid y breakpoints, validación junto al campo, botón
deshabilitado durante submit) y la regla 🔴 de §1.2: el componente no toca
DbContext, FbConnection ni SQL.

TAREA: Transformar el formulario Delphi <ARCHIVO_DFM> (+ su <ARCHIVO_PAS>)
en un componente Razor en src/Esba.Web/Components/Pages/<AREA>/<NOMBRE>.razor.

Procedimiento:
1. Leé el .dfm y armá el INVENTARIO DE CONTROLES: control VCL → propósito →
   equivalente MudBlazor. Mapa de referencia mínimo:
   - TEdit/TDBEdit → MudTextField; TDateEdit (RxLib) → MudDatePicker;
     TCurrencyEdit → MudNumericField; TComboBox/TDBLookupCombo → MudSelect o
     MudAutocomplete (server-side si la lista es grande, §3.2);
     TCheckBox → MudCheckBox; TRadioGroup → MudRadioGroup;
     TPageControl/TTabSheet → MudTabs/MudTabPanel; TGroupBox/TPanel → MudPaper
     o MudCard; TDBGrid embebida → MudDataGrid server-side;
     TButton de guardar/cancelar → MudButton; diálogo Busqueda →
     EsbaLookupDialog/MudAutocomplete (§3.3).
   - Controles puramente decorativos (TBevel, TShape, labels sueltos de
     layout): descartar, lo resuelve el layout.
2. Generá el componente .razor:
   - @page con ruta REST-like (ej. /alumnos/nuevo, /alumnos/{Id:int}) y
     [Authorize] con la política del área (sucesora de BARRA_SEGU, §2.7).
   - Layout responsivo: MudGrid/MudItem con breakpoints xs=12 sm=6 md=4 —
     NADA de posiciones absolutas; agrupá los campos en secciones lógicas
     según los GroupBox/Tabs originales.
   - MudForm enlazado al Command de la Etapa 2, reutilizando el validador
     FluentValidation para feedback inmediato por campo (§2.4 y §3.4).
   - Inyección (@inject) SOLO de servicios de Esba.Application + ISnackbar +
     IDialogService.
   - Manejo del Result<T>: Ok → Snackbar de éxito + navegación; Warning →
     Snackbar warning; NeedsConfirmation → MudDialog de confirmación estándar
     y reintento confirmado; Error → mensaje en la UI sin stack trace.
   - Estado de envío: botón Guardar deshabilitado + MudProgressCircular
     mientras se procesa; modo alta vs edición resuelto por la ruta (el legacy
     reusaba el mismo form, conservá esa economía).
3. Si el @code supera ~150 líneas o aparece lógica condicional de negocio,
   extraela a Application o a un componente hijo (§2.1.2) — decilo
   explícitamente si te pasa.
4. Entregá también el inventario de controles del paso 1 como tabla Markdown,
   marcando con ⚠️ los controles cuyo comportamiento legacy no pudiste
   determinar desde el .dfm/.pas.

NO migres: lógica de negocio que encuentres en los handlers (eso es Etapa 2 —
si encontrás lógica aún no migrada, listala como pendiente, no la metas en el
componente).
```

**Checklist de aceptación 3.A:**
- [ ] Cero SQL, `DbContext` o clases de Infrastructure en el `.razor`.
- [ ] Todos los campos usan `MudGrid` con breakpoints; sin tamaños absolutos.
- [ ] Errores de validación aparecen junto a cada campo, no en un diálogo final.
- [ ] La semántica `Result<T>` (Ok/Warning/NeedsConfirmation/Error) está implementada.
- [ ] La página tiene ruta propia y `[Authorize]` con política.

### Prompt 3.B — Pantalla de listado (sucesoras de `modulovariable`)

```text
Contexto: migración ESBA. migration_improvements.md §3.3 establece la regla 🔴:
toda pantalla de listado/reporte usa los componentes genéricos EsbaListView<TItem>
+ EsbaFilterPanel (sucesores de modulovariable.pas y parametros.pas); crear una
grilla a medida requiere justificación.

TAREA: Migrar la pantalla de listado <ARCHIVO_PAS> (que en el legacy invoca a
modulovariable/parametros con <DESCRIBIR PARÁMETROS O PEGAR EL ARRAY Param>)
a una página Blazor que instancie EsbaListView + EsbaFilterPanel.

Generá:
1. La definición tipada de filtros para EsbaFilterPanel (campo, tipo de
   control, origen de datos del combo vía servicio — nunca SQL embebido),
   replicando los filtros que el array Param legacy generaba en runtime.
2. La página .razor con: EsbaFilterPanel + EsbaListView<TDto> con ServerData
   conectado a la query Dapper de la Etapa 1.B (paginación/orden/filtro en
   servidor), columnas según la grilla legacy, y toolbar estándar con
   Exportar Excel (ClosedXML) y PDF (QuestPDF) ya provista por EsbaListView.
3. [Authorize] con la política del área y entrada de menú correspondiente.

Si EsbaListView/EsbaFilterPanel aún no existen en el repo, primero proponé su
contrato (parámetros, tipos, callbacks) y implementación mínima viable, y
recién después la página concreta.
```

**Checklist de aceptación 3.B:**
- [ ] La página usa `EsbaListView`/`EsbaFilterPanel` (o justifica por qué no).
- [ ] Paginación, orden y filtros se resuelven en el servidor.
- [ ] Los combos de filtros cargan por servicio inyectado.

---

## Etapa 4 — Testing (xUnit sobre los servicios creados)

### Prompt 4.A — Tests unitarios de un caso de uso/servicio

```text
Contexto: migración ESBA. migration_improvements.md §2.6 exige tests unitarios
(xUnit + NSubstitute) para todo servicio de dominio y caso de uso con lógica
condicional. Los tests viven en tests/Esba.Application.Tests/ (casos de uso)
y tests/Esba.Domain.Tests/ (lógica de dominio pura).

TAREA: Generar la suite de tests unitarios de <SERVICIO> (en
src/Esba.Application/Features/<AREA>/) y de su validador asociado.

Procedimiento:
1. Leé el servicio y su validador, y enumerá ANTES de escribir tests la lista
   de comportamientos a cubrir:
   - Caminos del validador: cada regla con un caso válido y al menos un caso
     inválido por regla (usá FluentValidation.TestHelper:
     TestValidate + ShouldHaveValidationErrorFor).
   - Caminos del handler: éxito (Result.Ok con efectos esperados), cada rama
     de error de negocio, y el mapeo ERRCOD si llama a un SP wrappeado
     (2→Error, 1→NeedsConfirmation, 0+msg→Warning).
   - Comportamiento transaccional: en error/validación fallida NO se llama a
     SaveChanges/Commit del IUnitOfWork; en éxito se llama exactamente una vez.
   - Propagación de CancellationToken a las dependencias.
2. Escribí los tests:
   - xUnit, NSubstitute para todas las interfaces de Abstractions (repos,
     IUnitOfWork, wrappers de SP, IEmailService...). Nada de base de datos real
     en tests unitarios — eso es Esba.IntegrationTests.
   - Nomenclatura: Metodo_Escenario_ResultadoEsperado, en castellano coherente
     con el dominio (ej. Inscribir_AlumnoYaInscripto_DevuelveError).
   - Patrón AAA con secciones claras; un assert lógico por test (varios
     Assert sobre el mismo Result cuentan como uno).
   - Builders/fixtures privados para construir Commands válidos por defecto,
     mutando solo el campo bajo prueba (evita tests frágiles).
3. Si al testear descubrís lógica imposible de cubrir sin tocar infraestructura
   (DateTime.Now directo, estática oculta, new de clase concreta), NO lo
   resuelvas con hacks de test: proponé la refactorización mínima del servicio
   (inyectar TimeProvider, extraer interfaz) y aplicala.
4. Cerrá con un resumen: comportamientos cubiertos, comportamientos
   deliberadamente no cubiertos y por qué, y equivalencias con el legacy que
   convendría verificar además con un test de integración (paridad con SP o
   con el comportamiento del formulario Delphi, §2.6).

Ejecutá dotnet test al final y mostrá el resultado; si algo falla, arreglalo
antes de terminar.
```

**Checklist de aceptación 4.A:**
- [ ] Cada regla del validador tiene al menos un test positivo y uno negativo.
- [ ] El mapeo `Result<T>`/ERRCOD está cubierto rama por rama.
- [ ] Se verifica que en error no hay commit y en éxito hay exactamente uno.
- [ ] `dotnet test` pasa en verde y se muestra la salida.

### Prompt 4.B — Test de integración de equivalencia con el legacy

```text
Contexto: migración ESBA. migration_improvements.md §2.6: la paridad funcional
con el legacy se verifica con tests de integración contra una base Firebird de
prueba; los SP XXX_* se testean comparando salida vieja vs wrapper nuevo.
Los tests viven en tests/Esba.IntegrationTests/.

TAREA: Crear el test de integración de equivalencia para <SERVICIO o WRAPPER DE SP>.

Generá:
1. Si no existe aún, la infraestructura base de la suite: fixture de xUnit
   (IClassFixture/ICollectionFixture) que levante Firebird en contenedor
   (imagen firebirdsql/firebird, vía Testcontainers for .NET) o use la cadena
   de conexión de una BD de prueba desde variable de entorno
   ESBA_TEST_CONNECTION (documentá ambas opciones), aplique el DDL versionado
   del repo y cargue datos semilla mínimos.
2. Datos semilla realistas para el caso (alumno, carrera, materias, etc.) —
   inferí los valores plausibles de los rangos que validan los formularios
   legacy documentados en delphi_structure.md.
3. Los tests de equivalencia:
   - Para un wrapper de SP: misma entrada → comparar la salida del wrapper
     contra el SELECT directo al SP, campo por campo.
   - Para un caso de uso que reemplaza SQL Delphi: ejecutar el caso de uso y
     verificar el estado resultante en la base (filas, valores, registros de
     auditoría tipo LOG_CURSADA) contra el efecto que producía el SQL legacy
     (citá el archivo .pas y el SQL original en un comentario del test).
4. Limpieza: cada test deja la base como la encontró (transacción con rollback
   o respawn del contenedor por colección).

Marcá la clase con un trait/categoría "Integration" para poder excluirla del
ciclo rápido (dotnet test --filter Category!=Integration).
```

**Checklist de aceptación 4.B:**
- [ ] La suite corre tanto con Testcontainers como con `ESBA_TEST_CONNECTION`.
- [ ] El SQL/SP legacy de referencia está citado en el test.
- [ ] Los tests son repetibles (no dependen de datos residuales).

---

## Apéndice — Prompts auxiliares cortos

**Extraer el DDL pendiente (prerrequisito de la Etapa 1):**
```text
Necesito versionar el esquema de la base Firebird legacy (esba.gdb) según la
regla 🔴 de migration_improvements.md §1.3. Generame el script/los comandos
gbak + isql -x exactos para: (1) backupear la base de producción, (2)
restaurarla local con upgrade de ODS a Firebird 5, (3) extraer el DDL completo
(tablas, índices, triggers, generadores y el fuente PSQL de todos los SP XXX_*)
y (4) dejarlo versionado en db/schema/ del repo, separando los SP en un archivo
por procedimiento.
```

**Verificar cumplimiento de reglas antes de un PR:**
```text
Revisá el diff actual contra las reglas 🔴 de migration_improvements.md y
reportá cada violación con archivo:línea y la sección de la regla violada.
Buscá específicamente: SQL interpolado/concatenado, .Result/.Wait()/async void,
estado estático mutable, referencias a EF/Firebird desde Esba.Domain o desde
componentes .razor, escrituras sin validador, lecturas sin CancellationToken,
y librerías de UI distintas de MudBlazor.
```

**Cuando un comportamiento legacy es ambiguo:**
```text
Antes de migrar <ARCHIVO_PAS>, analizá el fuente y armá una lista de PREGUNTAS
DE NEGOCIO: comportamientos que no puedo confirmar leyendo el código (reglas
implícitas en SPs cuyo PSQL no está en el repo, validaciones que dependen de
datos de configuración en la base, flujos que dependen del flag Superv/Rector).
Formato: pregunta concreta + qué viste en el código que la motiva + qué
asumirías por defecto. NO migres nada todavía.
```

---

*Documento vivo, como sus dos compañeros: si durante la migración un prompt produce resultados sistemáticamente malos o le falta contexto, corregilo acá para la próxima vez.*
