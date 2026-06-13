# Etapas 2+3 — Inscripción de Materias (InscripcionDeMaterias.pas → casos de uso + Blazor)

## Tabla de trazabilidad (Prompt 2.A)

| Método/elemento Delphi | Artefacto C# | Notas |
|---|---|---|
| `GrabaMateriaClick` (INSERT INTO CURSADA) | `InscribirEnMateriaHandler` | CUTUCO = 900+turno×10+comisión (el trigger resuelve el 9), NREG=0, APELLIDO/MATRIZ del alumno + INSTITUT/CARAC de la carrera, igual que el legacy. Duplicado validado explícitamente (el legacy dependía de la PK) |
| `GrabaMateriaClick` (UPDATE en modo Modificacion) | `ModificarInscripcionHandler` | ⚠️ Desviación: NO permite cambiar la materia (el legacy actualizaba la PK; EF no actualiza PKs) — para cambiar materia se elimina y se inscribe de nuevo |
| `eliminacursadaClick` (DELETE) | `EliminarInscripcionHandler` | Confirmación en la UI (`ShowMessageBoxAsync`); el trigger limpia RECURSA |
| Validación "Complete una condicion" | `InscribirEnMateriaValidator`/`ModificarInscripcionValidator` | Condiciones del combo de alta: CURSANDO, P/EQUIVALEN, RECURSANDO + rangos de turno/comisión (0-9) y formato CAA |
| `DataModule.IBInscMaterias` (SELECT CURSADA+MATERIAS) | `ICursadaQuery.ListarPorAlumnoAsync` (Dapper) | |
| `FuncionesText.Cuatrimestre` → SP `XXX_NUMCUATANIO` | `ICuatrimestreVigenteProcedure` (wrapper 2.B) | Default del cuatrimestre/año en el alta. `// TODO-migrar`: portable a C# con los ciclos lectivos ya modelados |
| `ConjuntoClick` → SP `XXX_INSC_CUAT_16032023` | ⚠️ **PENDIENTE** (registrado en deuda de la hoja de ruta) | Inscripción masiva por cuatrimestre con flujo FERRCOD=1 → confirmar → commit/rollback con la transacción abierta: requiere el patrón de dos fases (preview con rollback + re-ejecución confirmada) |
| `USUARIO = CodUsu` (número) | `Usuario` = nombre de usuario | ⚠️ Desviación deliberada: el legacy guardaba el número en unas pantallas y el nombre en otras; el sistema nuevo unifica al nombre |
| Combos/foco/alto del form | — descartados (solo-UI) | |

## UI

`/alumnos/{carre}/{codigo}/cursada` (`CursadaAlumno.razor`): grilla de cursada con condición
como chip, turno/comisión derivados del CUTUCO, notas y faltas (columnas secundarias ocultas
en pantallas chicas) + `InscripcionCursadaDialog` (alta/modificación, cuatrimestre por defecto
del wrapper). Botón real "Inscripción de materias" en el panel del buscador.

**Deuda del hito absorbida**: botón "Cambiar DNI/LM" en la ficha (`CambioDniLmDialog` sobre el
wrapper de `XXX_CAMBIA_DNI_LM`; si cambia el documento navega a la ruta nueva).

## Verificación

- 86 tests en verde. Nuevos: 10 unitarios de handlers (duplicado, inexistentes, rangos,
  no-commit en error) + 3 de integración, incluido el **roundtrip contra la base real**:
  inscribir → la query lo muestra y el trigger denormalizó APELLIDO → eliminar → la base
  queda como estaba.
- Checklists 2.A/3.A: validador único por escritura ✅ · `Result<T>` ✅ · una transacción por
  caso de uso (verificado por tests) ✅ · usuario por parámetro ✅ · async+ct ✅ · cero
  SQL/DbContext en `.razor` ✅ · responsive ✅ · `Result` en UI (Snackbar/Alert/confirmación) ✅
