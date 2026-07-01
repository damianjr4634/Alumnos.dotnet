# Hito 15 — Regularización de materias (terciarias + bachillerato)

**Estado:** 🟡 parcial — terciarias ✅ 2026-06-30; bachillerato ✅ 2026-07-01. Secundario
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

## 4. Bachillerato (incremento 2, 2026-07-01)

Alcance: la carrera **`BAC`** (única para la que el legacy corre el ladder de notas —
`_POSTVAL` tiene guard `CARRE='BAC'`). CNA (`TIPO=BAC`), `197916` (`TIPO=BAD`) y el
secundario `333/650` quedan diferidos: muestran el aviso "se migra en un incremento posterior".

Bachillerato es un flujo de **dos fases** que el dominio fusiona (sin staging):
1. **Faltas** (`XXX_REGULARIZACION_MAT_BAC`): `%inasist = round(INASIST×100/TOT_HORAS)`
   → ≤25% sigue, 26-40% **CONSEJO**, >40% **LIBRES**; rescate a RECURSANDO por tabla `RECURSA`.
2. **Notas** (`XXX_REGULARIZACION_MAT_POSTVAL`): ladder sobre 2 bimestres + promedio (TP_EVA3)
   + recuperatorio + nota "a regular" → REGULAR / A/REGULAR / PREVIO / LIBRES / CONSEJO + nota
   definitiva (FINAL1). **Interactivo**: ante CONSEJO devuelve las opciones Consejo/Regular/Libre
   (FBUTTONS) y recalcula con `PASO`.

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `_BAC` + `_POSTVAL` (condición) | `CalculoCondicionRegularizacionBachiller` (Domain) | Fusión faltas + ladder + PASO. Devuelve `RequiereDecision` ante CONSEJO. Portado 1:1, con equivalencia. |
| Rama BAC de `XXX_REGULARIZACION` (commit) | `RegularizacionRepository.ConfirmarBachilleratoAsync` | UPDATE CURSADA (con REGULAR/FECHA1/FINAL1); si queda REGULAR → CURSADA_HST + DELETE + ANALITIC (nota FINAL1, fecha FECHA1, CONDICION='REGULAR'). |
| `PASO`/`FBUTTONS` (diálogo por fila) | Selector inline Consejo/Regular/Libre en `RegularizacionEditorBachiller` | Confirmar se bloquea hasta resolver las filas en CONSEJO (mejor para el lote, §3.4). |
| `BtnLibre` (override manual) | Acción "Libre" por fila (solo CURSANDO/RECURSANDO) → `ForzarLibre` | Fuerza CONDICION=LIBRE con notas/horas en 99, sin ladder. |
| Router `XXX_REGULARIZACION_MAT` por `CARRERA.TIPO` | Branch en las páginas (`CARRE='BAC'` → editor bachillerato) | |

**Particularidades:** el ladder de faltas usa **solo INASIST** (no JUSTIF+INASIST como
terciarias); la rama BAC del commit **no persiste TP_EVA3/PROM** en CURSADA (se replica el
legacy); el analítico lleva la condición real (`REGULAR`) y la fecha del operador (FECHA1),
no la de TBL_CUAT.

## 5. Verificación

- `dotnet build` → **0 warnings**.
- `dotnet test` → **535 verdes**: Domain 188 (+16 terciario, +16 bachillerato),
  Application 253 (+5 terciario, +7 bachillerato), Integration 94 (+1 terciario, +1 bachillerato).
- Equivalencia (Firebird real):
  - `CalculoCondicionRegularizacionTerciaria` vs `XXX_REGULARIZACION_MAT_TERC` (6 escenarios).
  - `CalculoCondicionRegularizacionBachiller` vs `XXX_REGULARIZACION_MAT_BAC` + `_POSTVAL`
    (9 escenarios de notas/faltas + CONSEJO + CONSEJO/Regular), poblando `"$$$CURSADA"`
    con los derivados TP_EVA3/FINAL1 y comparando condición y nota final.

## 6. Pendiente (próximos incrementos)

- **Secundario 333/650** (`_333`, 3 trimestres + exámenes dic/mar) y **CNA** (`_BAC` de faltas
  sin notas: `TIPO=BAC` pero no corre `_POSTVAL`; "solo nota final"). También `197916` (`TIPO=BAD`,
  solo faltas) queda a definir.
- **Equivalencia del commit** (`RegularizacionRepository` vs las ramas TER/BAC de
  `XXX_REGULARIZACION`): el volcado es estructuralmente idéntico al de `CargaFinalRepository`
  (ya con equivalencia); falta el test dedicado — requiere un alumno con materia que apruebe
  directo en la base de prueba.
- **Autocompletado de faltas** desde `XXX_CONT_FALTAS` (asistencias) cuando CURSADA está en 0:
  hoy las faltas se leen de CURSADA y el usuario las edita, en ambas ramas. // TODO-migrar prefill.
- **Botones "A previa"** del formulario por-alumno (333/650): fuera del alcance de bachillerato.
