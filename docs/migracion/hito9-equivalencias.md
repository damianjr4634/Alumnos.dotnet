# Hito 9.3 — Equivalencias

Migración del ABM de equivalencias (`Equivalencia.pas`) y las impresiones de
equivalencia (bachiller y terciaria). **Decisión de alcance (usuario, 2026-06-18):**
9.3 cubre solo la **solapa Equivalencia** del form legacy (alta en ANALITIC con
`CONDICION='EQUIVALENCIA'`); las solapas **pase-regular** y **pase-final** del mismo
form se difieren al **hito 14** (notas de finales / actas A-REGULAR), donde encajan por
dominio.

Sub-commits: **9.3a** (modelado de ANALITIC) · **9.3b** (alta de equivalencia) ·
**9.3c** (impresiones bachiller + terciaria).

---

## Hito 9.3a — Modelado de ANALITIC (Etapa 1)

**Estado:** ✅ 2026-06-18.

`ANALITIC` (histórico académico/analítico) no estaba modelado y es la tabla donde
escribe la grabación de equivalencias. Primera tabla con **DDL versionado** del repo
(hasta ahora `db/schema/` solo versionaba `procedures/`).

- `db/schema/tables/ANALITIC.sql`: `CREATE TABLE` + generador `G_ANALITIC` + índices +
  triggers, extraído de la base real con `isql -x`.
- Domain `Analitico` (PK `CARRE, COD_ALU, COD_MAT`) + `AnaliticoConfiguration` + DbSet.
- Comportamiento de triggers documentado y respetado: `INDICE` lo asigna `ANALITIC_BI0`
  (`GEN_ID(G_ANALITIC)`); `ANALITIC_BIU` rellena `ACTINT`/`ACTDGE` con `LPAD(...,15,'0')`
  (el padding lo hace la base, no C#); `BI0` impide que una materia esté en CURSADA y
  ANALITIC a la vez. `INDICE`/`ULTMOD` marcados `ValueGenerated`.
- Test de mapeo EF contra Firebird real (200 filas, `INDICE` no-cero).

## Hito 9.3b — Alta de equivalencia (Etapas 2+3)

**Estado:** ✅ 2026-06-18.

Alta de una equivalencia de materia, sucesor de `GrabaMateriaClick` (página 0 de
`Equivalencia.pas`): inserta en ANALITIC con `CONDICION='EQUIVALENCIA'`.

### Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `XXX_INSC_VALMAT(...,'A')` (gate de duplicado) | `IValidacionMateriaProcedure` / `ValidacionMateriaProcedure` (2.B) | FERRCOD 2→Error. TIPO 'A' no chequea correlatividades (eso es 'I'). |
| `XXX_NUMERO_EQUIVALENCIA` (próximo número) + `XXX_GRABA_NUMEQUI` (confirma) | `IEquivalenciaNumeracionProcedure` / `EquivalenciaNumeracionProcedure` (2.B) | El nº vive en TBLEQUIVA; TER comparte secuencia. |
| `INSERT INTO ANALITIC (...) VALUES (...)` con `CONDICION='EQUIVALENCIA'` | `CrearEquivalenciaHandler` + `IAnaliticoRepository` + `IUnitOfWork` | Una transacción por caso de uso; sin SQL concatenado. |
| Radio Interna/D.G.E.G.P. → `ACTINT`/`ACTDGE` | `TipoActuacionEquivalencia` + `CrearEquivalenciaCommand` | Interna: número **autoritativo del servidor** (XXX_NUMERO_EQUIVALENCIA), no se confía en el cliente. D.G.E.G.P.: número que tipea el operador. |
| `Copy(Actuacion,1,len-3)+Copy(len-1,2)` (saca el separador) | `CrearEquivalenciaHandler.SinSeparador` | Quita el `/`; el trigger `BIU` rellena a 15 ceros. |
| Checkboxes Constancia/Analítico → `A_C` | `DocumentoEquivalencia` ('C'/'A', opcional) | |
| `FEQDOCE/FEQMATE/FEQCARRE/FEQINST` (origen) | campos `*Origen` del command | `FEQINST` = institución de origen (= `INSTITUT`). |
| Popup desde el form del alumno | `EquivalenciaDialog` lanzado desde la cursada del alumno | Botón "Cargar equivalencia" en `CursadaAlumno`. |

### Decisiones

- **Numeración interna autoritativa**: el handler vuelve a pedir el número a
  `XXX_NUMERO_EQUIVALENCIA` y, si es nuevo, lo confirma con `XXX_GRABA_NUMEQUI` tras
  grabar (igual que el legacy: solo interna + número nuevo). La UI solo lo muestra.
- **Modo "por cuatrimestre/año"** del legacy (alta masiva de todas las materias de un
  cuatrimestre como equivalencia, INSERT...SELECT sobre MATERIAS): **diferido** — el alta
  individual es el caso primario. // TODO-migrar dentro de 9.3 si se pide.

### Verificación

- `dotnet build` → 0 warnings.
- `dotnet test` → verde. Application 122 (+6 handler, +5 validator); Domain 84.
- Integration (Firebird real) → +2: equivalencia de `XXX_INSC_VALMAT` y
  `XXX_NUMERO_EQUIVALENCIA` (wrapper vs SP directo). `XXX_GRABA_NUMEQUI` no se testea en
  integración (escribe TBLEQUIVA); lo cubre el unitario del handler.

## Hito 9.3c — Impresiones de equivalencia

**Estado:** ⬜ (próximo): bachiller (`XXX_IMPRESION_EQ_BAC`) y terciaria
(`XXX_CONSTANCIA_TERCIARIA`, wrapper ya existe), reportes QuestPDF reusando
`ReporteConstanciaLayout`.
