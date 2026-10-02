# Hito 18 — Acciones sobre el alumno: copiar, mover y borrar

**Estado:** ✅ 2026-09-30
**Etapas cubiertas:** 2 (wrappers 2.B + handlers con `Result<T>` y FluentValidation), 3
(diálogos en el buscador), 4 (tests unitarios y de equivalencia contra los SP). La Etapa 1
no aplica: no hay modelo nuevo, los tres SP operan sobre `ALUMNOS` y su historial.
**Documentos rectores:** `migration_improvements.md` §1.3 (wrappers, ERRCOD → `Result`),
§2.3 (claims en vez de `CodUsu`/`Superv`), §2.7 (el servidor deniega, la UI oculta), §3.2.

## 1. Alcance

Hueco detectado 2026-09-30: tres opciones del menú Alumno de `FrmEsba` que llaman a SP
con el patrón `FERRCOD`/`FERRMSG` de confirmación y no figuraban en la hoja de ruta:

- **Copiar alumno a la carrera…** (`dxBarButton29Click` → `XXX_COPIA_ALUMNO`).
- **Mover alumno a la carrera…** (`dxBarButton30Click` → `XXX_MUEVE_ALUMNO`).
- **Borrar alumno** (`dxBarButton43Click` → `XXX_BORRA_ALUMNO`): eliminación física con
  todo el historial, solo supervisores.

Los tres SP están versionados en `db/schema/procedures/` y **se conservan** (regla 🔴
§1.3); cada uno tiene su wrapper con `// TODO-migrar` de prioridad baja.

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| Panel `parametros` "Car.Destino" (combo `YYY_CARRE_SEGU`) | `CambiarCarreraAlumnoDialog.razor` (parámetro `Mover`) | Autocomplete de las carreras permitidas al usuario, sin la de origen. |
| `XXX_COPIA_ALUMNO(CARDDE, CARHTA, COD_ALU, CODUSU)` | `ICopiaAlumnoProcedure` / `CopiaAlumnoProcedure` | El SP inserta y devuelve FERRCOD=1: dos fases (rollback en la previsualización, commit al confirmar), como `PaseLibreProcedure`. |
| `XXX_MUEVE_ALUMNO(…)` | `IMueveAlumnoProcedure` / `MueveAlumnoProcedure` | Ídem; FERRCOD=2 si existe en destino o tiene CURSADA/ANALITIC en origen. |
| `XXX_BORRA_ALUMNO(CARRE, CODALU, USUARIO)` | `IBorraAlumnoProcedure` / `BorraAlumnoProcedure` | Ídem; FERRCOD=2 si `USUARIOS.SUPERV='N'`. |
| Bloque `If FERRCOD=2 … ELSE If FERRCOD=1 and MessageDlg(…)=mrYes …` repetido en los tres handlers | `ProcedimientoConConfirmacion` (Infrastructure, interno) + `Result.DesdeErrCod` | Un solo punto para la semántica 2/1/0 y la transacción. |
| `FuncionesConfiguracion.CodUsu` / `Superv` | claims `CodigoUsuario()` / `EsSupervisor()` → `CambiarCarreraAlumnoCommand.CodigoUsuario`, `BorrarAlumnoCommand.EsSupervisor` | `BorrarAlumnoHandler` deniega a no supervisores antes de tocar la base (además del control del SP). |
| `NombreUsuario + ', ' + FERRMSG` en los diálogos | mensaje del SP tal cual | El nombre del operador delante del mensaje no aporta en una app con sesión. |
| Panel "ALUMNO SELECCIONADO" de `FrmEsba` | `AccionesAlumnoDialog.razor`: botones "Copiar a otra carrera", "Mover a otra carrera" y "Borrar alumno" (este último solo visible a supervisores) | Si la acción cambia el padrón, el diálogo cierra con `true` y `Home` recarga la grilla con los mismos filtros. |

## 3. Decisiones

- **Dos fases sin transacción de larga vida** (mismo patrón del hito 7): los SP ya escriben
  antes de pedir confirmación, así que la previsualización ejecuta y revierte, y la
  confirmación vuelve a ejecutar y commitea. Entre ambas no hay estado en el servidor.
- **Validación previa en Application**: destino distinto del origen, usuario válido y el
  gate de supervisor para borrar, con mensaje propio; el legacy delegaba todo al SP.
- **Sin política de autorización nueva**: `[Authorize]` a secas como el resto; el
  enforcement fino por `MNUOPC` es del hito 12.3. El gate de supervisor del borrado está
  en el handler porque es una regla de negocio del SP, no una política de menú.

## 4. Verificación (checklists de `migration_prompts.md`)

- **2.A**: trazabilidad ✅ · un validador por comando y `Result<T>` ✅ · una transacción
  por invocación, abierta y cerrada en el wrapper ✅ · `CodUsu`/`Superv` por claims ✅ ·
  `async` + `CancellationToken` ✅.
- **2.B**: parámetros y RETURNS 1:1 con el PSQL ✅ · semántica ERRCOD encapsulada en
  `ProcedimientoConConfirmacion` ✅ · `// TODO-migrar` con resumen en cada wrapper ✅ ·
  tests de equivalencia ✅.
- **3.A**: cero SQL/Infrastructure en los `.razor` ✅ · semántica `Result` completa
  (Ok/Warning → snackbar y cierre; NeedsConfirmation → segundo paso; Error → alerta) ✅ ·
  botón deshabilitado durante el envío ✅.
- **4.A/4.B**: `CambiarCarreraAlumnoHandlerTests` (7) y `BorrarAlumnoHandlerTests` (5)
  cubren validaciones, gate de supervisor, delegación previsualizar/confirmar y
  propagación del `CancellationToken`; `AccionesAlumnoEquivalenciaTests`
  (Category=Integration, 5) compara wrapper vs SELECT directo al SP en los caminos de
  error (no mutantes) y de previsualización (rollback en ambos), verificando que el
  padrón queda intacto — en verde el 2026-09-30.

## 5. Pendientes

- La consulta que elige la muestra de los tests de equivalencia (alumno con cursadas y
  carrera donde no existe) tarda ~10 s en la base local; si molesta en el ciclo, se
  puede fijar una muestra por variable de entorno.
