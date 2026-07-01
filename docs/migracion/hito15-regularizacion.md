# Hito 15 — Regularización de materias (incremento 1: terciarias)

**Estado:** 🟡 parcial — terciarias ✅ 2026-06-30. Bachillerato (BAC + POSTVAL), secundario
(333/650) y CNA quedan para incrementos siguientes.
**Etapas cubiertas:** 1 (queries), 2 (dominio + handler + validador), 3 (páginas + menú/acción),
4 (tests unitarios + equivalencia).
**Documentos rectores:** `migration_improvements.md` §1.2, §1.3, §2.1, §2.3, §2.4.
**Decisiones (2026-06-30):** portar a C# como el hito 14 (sin staging); arrancar por terciarias.

## 1. Alcance

Carga de las **notas del cursado** (2 parciales + recuperatorio + faltas) de un alumno en
una materia **terciaria** y resolución de su **condición** (REGULAR / PROMOCIONA / FINAL /
RECURSA / LIBRE / REINCORPORA / se mantiene CURSANDO). Dos variantes, como el legacy:
- **Por alumno** (`/alumnos/{c}/{cod}/regularizacion`, acción del buscador) — sucesora de
  `RegularizacionDeMaterias_nuevo.pas`.
- **Por comisión, en lote** (`/academica/regularizacion-comision`, menú Alumnos › Regularización)
  — sucesora de `RegularizacionDeMateriasXComision_nuevo.pas`.

Cierra la cadena académica: alta → inscripción → cursado/asistencias → **regularización** →
permiso → final → analítico.

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `XXX_REGULARIZACION_MAT_TERC` (condición) | `CalculoCondicionRegularizacionTerciaria` (Domain) | Ladder TP_EVA/TP_EVA2/RECUP + ajuste por faltas + PROMOCIONA/FINAL por flags de MATERIAS y umbral `Regula_NotPromocion`. Portado 1:1, con equivalencia. |
| Rama TER de `XXX_REGULARIZACION` (commit) | `RegularizacionRepository.ConfirmarTerciariaAsync` | UPDATE CURSADA; si aprueba directo (PROMOCIONA/FINAL) → CURSADA_HST + DELETE + ANALITIC, en una transacción. Nota al analítico: promedio (PROMOCIONA) o recuperatorio/promedio (FINAL). |
| Staging `"$$$CURSADA"` (por usuario) | Estado del componente `RegularizacionEditor` | Se erradica el estado global (§2.3), como `"$$$PERMEXA"` en el hito 14. |
| INSERT ... SELECT a `"$$$CURSADA"` | `RegularizacionQuery.ObtenerPorAlumnoAsync` / `ObtenerPorComisionAsync` | SQL parametrizado; por comisión excluye CONDICION='REGULAR' y bajas. |
| `ValidoGrabaciondeMateria` + commit | `ConfirmarRegularizacionHandler` | Valida, resuelve condición (dominio, autoridad del servidor) y vuelca. `Result<T>`. |
| Grilla editable + tabs por carrera | `RegularizacionEditor.razor` (compartido) + páginas por alumno / por comisión | Solo markup/binding; preview de condición en vivo con el dominio. |
| Menú `Alumnos › Regularización` (deshabilitado) | Habilitado → por comisión; acción "Regularización" en el buscador → por alumno | |

## 3. Decisiones y particularidades

- **CUA_ANIO normalizado** (CHAR(3) "124"): se quita la barra que el operador tipee, en query
  y commit (misma corrección que en actas).
- **Centinela `99`** = parcial ausente/no rendido; nota válida = vacía, 1..10 o 99.
- **TOT_HORAS = 0** ⇒ el SP fuerza la condición al fallback (CURSANDO): el port lo replica.
- **INDICE de CURSADA_HST** lo pone su trigger (no se inserta); `LOG_*` los escriben triggers AFTER.
- **Autoridad del servidor**: la condición la calcula el handler, no el cliente (§2.7).

## 4. Verificación

- `dotnet build` → **0 warnings**.
- `dotnet test` → **511 verdes**: Domain 172 (+16 cálculo terciario), Application 246 (+5 handler),
  Integration 93 (+1 equivalencia).
- Equivalencia (Firebird real): `CalculoCondicionRegularizacionTerciaria` vs
  `XXX_REGULARIZACION_MAT_TERC` en 6 escenarios de notas/faltas, poblando `"$$$CURSADA"` y
  comparando la condición resultante.

## 5. Pendiente (próximos incrementos)

- **Bachillerato** (`_BAC` + `_POSTVAL` interactivo con FBUTTONS/PASO) y **secundario 333/650**
  (`_333`, 3 trimestres + exámenes dic/mar) y **CNA** (solo nota final).
- **Equivalencia del commit** (`RegularizacionRepository` vs rama TER de `XXX_REGULARIZACION`):
  el volcado es estructuralmente idéntico al de `CargaFinalRepository` (ya con equivalencia);
  falta el test dedicado — requiere un alumno con materia PROMOCION/APRSFINAL en la base de prueba.
- **Autocompletado de faltas** desde `XXX_CONT_FALTAS` (asistencias) cuando CURSADA está en 0:
  hoy las faltas se leen de CURSADA y el usuario las edita. // TODO-migrar prefill de faltas.
- **Botones "A previa" / "Libre"** del formulario por-alumno (333/650): fuera del alcance terciario.
