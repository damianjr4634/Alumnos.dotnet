# Hito 8 — Exámenes (ABM de mesas + permisos)

**Estado:** ✅ 2026-06-15 (alcance acordado; notas de finales y actas diferidas al hito 14).
**Etapas cubiertas:** 1 (MESAS/MESA_TIPO), 2 (handlers + wrappers SP), 3 (pantallas), 4 (tests).
**Documentos rectores:** `migration_improvements.md` §1.3, §2.1, §3.2/§3.3, §2.4.

## 1. Alcance

ABM de mesas de examen, listado, y permisos de examen (individuales y masivos).
Cuatro incrementos commiteados:
1. Listado de mesas (read).
2. ABM de mesas (entidad + `XXX_VALIDO_MESA`).
3. Permisos individuales (`XXX_MATERIAS_FINALES` + PERMEXA).
4. Permisos masivos.

**Diferido a hito 14** (decisión 2026-06-15): notas de finales y actas — ver §4.

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `MesasExamen.pas` (listado) | `MesasQuery.BuscarAsync` + `ListadoMesas` | Server-side, join MATERIAS + MESA_TIPO. |
| `MesasExamen.pas` (ABM) | entidad `Mesa` (MESAS) + `MesaRepository` + handlers + `MesaFormDialog` | PK (CARRE, MESA) por EF; baja física. |
| `XXX_VALIDO_MESA` | `IValidoMesaProcedure` (2.B) | Pre-chequeo de duplicado en alta. |
| `MESA_TIPO` (combo tipo) | entidad `TipoMesa` + `TipoMesaQuery` | Filtra por `CARRERA.TIPO` (CONTAINING). |
| `PermisoExamen.pas` (individual) | `PermisosExamenRepository` (Dapper) + handlers + `PermisosExamen.razor` | INDICE lo genera el trigger PERMEXA_BI0; clave de negocio COD_ALU+CARRE+MESA+COD_MAT. |
| `XXX_MATERIAS_FINALES` | `IMateriasFinalesProcedure` (2.B) | Materias que el alumno puede rendir (correlatividad de final resuelta por el SP) + mesa. |
| `CargadePermisosMasivo.pas` | `GuardarPermisosMasivoHandler` + `InsertarVariosAsync` + `CargaPermisosMasivo.razor` | Una carrera por vez; lista (alumno → materia → mesa) insertada en bloque. |

## 3. Decisiones

- **MESAS por EF, PERMEXA por Dapper**: MESAS tiene PK limpia (CARRE, MESA) →
  EF. PERMEXA tiene INDICE generado por trigger y clave de negocio compuesta →
  Dapper (insert sin INDICE, alta/baja/listado por clave de negocio).
- **Dapper `DateOnly`**: se agregó un type handler global en `FbConnectionFactory`
  (Firebird devuelve DATE como DateTime); beneficia a todas las queries Dapper.
- **Permisos masivos sin grilla editable dependiente**: en vez del DBGrid con
  combos encadenados del legacy, un editor de fila (alumno → materia → mesa) que
  agrega a una lista y graba en bloque (una transacción).

## 4. Diferido (hito 14): notas de finales y actas

`XXX_CARGA_FINAL` no recibe las notas: las lee de la tabla **permanente y
compartida `"$$$PERMEXA"`** (staging por `USUARIO`), que el form de notas puebla
y edita. Es estado global mutable (anti-patrón §2.3). Se difirió para decidir el
enfoque: portar a C# el cálculo de condición/analítico (preferido) o replicar el
staging con `// TODO-migrar`. Las actas (mesas, A-REGULAR, reincorporación) son
reportes que acompañan a las notas.

## 5. Verificación

- `dotnet build Esba.slnx` → 0 warnings.
- `dotnet test` → **205 verdes** (Domain 37, Application 102, Integration 66):
  - Application: validadores y handlers de mesa, permiso individual y masivo (NSubstitute).
  - Integration (Firebird real): listado de mesas; roundtrip de mesa (EF +
    `XXX_VALIDO_MESA` detecta duplicado); `XXX_MATERIAS_FINALES`; roundtrip de
    PERMEXA (alta/listado/baja) e insert en bloque.
