# Hito 6 — ABM de Materias y Comisiones (cierra Académica)

**Estado:** ✅ 2026-06-15
**Etapas cubiertas:** 1 (DOCENTES, queries), 2 (handlers + wrappers SP), 3 (formularios Blazor), 4 (tests).
**Documentos rectores:** `migration_improvements.md` §1.3 (datos/SP/transacciones), §2.1 (UI),
§3.3/§3.4 (componentes y UX), §2.4 (validación).

## 1. Alcance

Cierra el área Académica con el ABM completo de **materias** y **comisiones**, el
modelo mínimo de **DOCENTES** para el join, y resuelve la deuda transversal de
**inscripción masiva por cuatrimestre**. Se construyó en tres incrementos
commiteados (1/3 materias, 2/3 DOCENTES+listado comisiones, 3/3 ABM comisiones) +
la inscripción masiva.

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `altamodifmaterias.pas` (ABM materias) | `CrearMateriaHandler`/`ActualizarMateriaHandler` + `MateriaFormDialog` | Baja lógica (ESTADO 'B'/'Y'), código LPad a 2, correlativas unidas por '-' como multiselects; regla promoción⊕aprueba-sin-final. |
| `cargacomisiones.pas` (ABM comisiones) | `CrearComisionHandler`/`ActualizarComisionHandler`/`EliminarComisionHandler` + `ComisionFormDialog` | Grilla días×bloques en vez del StringGrid; CUTUCO/materia/cuat-año son la clave (fija en edición). |
| `XXX_VALIDO_COMISION` (pre-chequeo duplicado) | `IValidoComisionProcedure` (wrapper 2.B) | Llamado en el alta antes de insertar. |
| `XXX_VAL_COMISION` (post-insert: cuatrimestre + superposición) | `ComisionRepository.GuardarYValidarAsync` | Ejecutado en la MISMA transacción que el insert (EF + Dapper comparten conexión); rollback si FERRCOD=2. |
| Grilla de bloques (PRIMERO/PRISEG/UNICO…) | `Domain/Academica/BloqueHorario` + `HorarioComision` | Lógica pura: codifica/decodifica el set de bloques marcados por día. |
| `DOCENTES` (combo de docente) | entidad `Docente` (mínima) + `DocentesQuery.ListarActivosAsync` | Solo CODPROFES/DOCENTE/FECHA_ING/FECHA_BAJ; el ABM de profesores es hito 10. |
| listado de comisiones (`FormActivate`) | `ComisionesQuery.BuscarAsync` + `ListadoComisiones` | Server-side (join MATERIAS+DOCENTES), export Excel/PDF (hito 5). |
| inscripción masiva (`XXX_INSC_CUAT_16032023`) | `InscribirCuatrimestreCompletoHandler` + `InscripcionMasivaCuatrimestreProcedure` + `InscripcionMasivaDialog` | Ver §3. |

## 3. Patrón de dos fases (inscripción masiva)

El SP `XXX_INSC_CUAT_16032023` inserta en CURSADA y devuelve FERRCOD/FERRMSG
(0 ok, 1 override de supervisor con errores, 2 error duro). Para no sostener una
transacción de larga vida entre la decisión del usuario (§1.3), el wrapper expone
`EjecutarAsync(parametros, confirmar)`:

- **Previsualizar** (`confirmar=false`): ejecuta el SP y hace **rollback**; devuelve
  el detalle (FERRMSG) de lo que ocurriría. Nada se persiste.
- **Confirmar** (`confirmar=true`): vuelve a ejecutar el SP y **commitea** (salvo
  FERRCOD=2). La UI solo lo llama tras la confirmación del usuario.

El handler resuelve INSTITUTO/CARACT desde la carrera (van a CURSADA).

## 4. Decisiones

- **Transacción del post-SP de comisiones en Infrastructure**: `XXX_VAL_COMISION`
  lee la fila recién insertada, así que el insert + SP + commit/rollback se
  orquestan en `ComisionRepository` (no en un IUnitOfWork genérico) para compartir
  la conexión EF/Dapper. Es el caso previsto en §1.3.
- **Edición de comisión con clave fija**: el legacy permitía cambiar la clave
  (CUTUCO/materia/cuat-año); se restringió a editar docente/horario/titularidad
  (cambiar la clave = baja+alta), evitando el cambio de PK en EF.
- **DOCENTES parcial**: mapeo de solo 4 columnas; el esquema se versiona por DDL
  (no migraciones EF), así que el mapeo parcial es seguro. ABM completo en hito 10.

## 5. Verificación

- `dotnet build Esba.slnx` → 0 warnings (TreatWarningsAsErrors).
- `dotnet test` → **169 verdes** (Domain 37, Application 79, Integration 53):
  - Domain: `BloqueHorario` (codificar/decodificar/armar slots).
  - Application: validadores y handlers de materia, comisión e inscripción masiva (NSubstitute).
  - Integration (Firebird real): query de materias/comisiones/docentes; roundtrip de
    `GuardarYValidarAsync` (commit en éxito, rollback en FERRCOD=2); roundtrip de
    previsualización masiva (rollback ⇒ CURSADA sin cambios).

## 6. Checklists

- 2.A/2.B (lógica + wrappers): trazabilidad presente, una transacción por caso de
  uso, ERRCOD encapsulado en los wrappers, `// TODO-migrar` en cada SP.
- 3.A/3.B (UI): formularios MudBlazor sin SQL ni lógica de negocio, `Result<T>`
  (Ok/Warning/NeedsConfirmation/Error) manejado; listados con `EsbaListView`.
- 4.A/4.B (tests): reglas con caso positivo/negativo, mapeo ERRCOD por rama,
  equivalencia contra la base real.
