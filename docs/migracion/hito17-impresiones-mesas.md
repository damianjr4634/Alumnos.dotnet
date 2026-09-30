# Hito 17 — Impresiones de mesas: citación a profesores y parte diario

**Estado:** ✅ 2026-09-29
**Etapas cubiertas:** 1 (queries Dapper sobre MESAS/DOCENTES/MATERIAS/PERMEXA/CARRERA), 2
(handler + formatos en dominio), 3 (dos pantallas + endpoints PDF + menú Planillas), 4
(tests unitarios, smoke de PDF y equivalencia contra el SQL legacy).
**Documentos rectores:** `migration_improvements.md` §1.3, §2.1, §2.2, §2.3, §3.4.

## 1. Alcance

Hueco detectado 2026-09-29: los dos reportes de `Impresiones.pas` colgados del submenú
**Mesas** del legacy no figuraban en la hoja de ruta:

- **"Impresion citaciones"** (`dxBarButton20Click` → `Imp_Mesas_citacion`): carta a cada
  docente convocado a mesa en un rango de fechas, en original y duplicado, con la tabla de
  mesas a integrar, espacio de notificación, firma de la autoridad elegida (rector/a,
  secretaria o dir. de estudios, con imagen opcional) y el texto del Art. 53 del
  Reglamento General.
- **Parte diario** (`dxBarButton21Click` → `Imp_Mesas_ParteDiario`): por cada fecha de
  examen del rango, las mesas agrupadas por carrera con hora, materia, tribunal,
  comisiones, cantidad de alumnos con permiso y aula.

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| Panel `parametros` de la citación (docente desde/hasta, fechas, carreras multi, firma R/S/D, imagen S/N) | `CitacionDocentes.razor` (`/examenes/citacion-docentes`) + `GenerarCitacionDocentesCommand` | Carreras por multiselección; las autoridades salen de la carrera elegida en "Autoridades de la carrera" (se propone la primera filtrada). El legacy las tomaba de `FuncionesConfiguracion` (carrera activa, global). |
| Panel del parte diario (carrera opcional, fechas) | `ParteDiarioMesas.razor` (`/examenes/parte-diario`) + `GenerarParteDiarioMesasCommand` | |
| `SqlDatos` de `Imp_Mesas_citacion` (docentes DISTINCT por titular/vocal1/vocal2) | `IImpresionesMesasQuery.ObtenerDocentesCitadosAsync` | JOIN en vez de LEFT JOIN: el legacy imprimía una carta vacía "Señor Profesor:" para las mesas sin tribunal. |
| `SqlDatos2` (mesas por docente, ejecutado en bucle) | `ObtenerMesasCitacionAsync` (una consulta, agrupada en el handler) | Orden por HORA numérica (el legacy la ordenaba como texto: "9:30" después de "18:30"); materia sin truncar a 30. |
| `SqlDatos` de `Imp_Mesas_ParteDiario` | `ObtenerParteDiarioAsync` | Trae COMI2 (el legacy repetía COMI3), los tres nombres del tribunal sin truncar a 15 y el nombre de la carrera para el subtítulo. |
| `FuncionesConfiguracion.Rector/Secretaria/DirEst` | `ObtenerAutoridadesAsync` (CARRERA.RECTOR/SECRETARIA/DIRESTU) | |
| `SUBSTRING(HORA…)||':'||…`, `COMI1/COMI3/COMI3`, `D1-D2-D3` | `FormatoMesa.Hora/Comisiones/Docentes` (dominio) | Funciones puras con tests. |
| Rótulos R/S/D → "RECTOR/A", "SECRETARIA", "DIR. DE ESTUDIOS" | `FirmanteCitacion` + `.Cargo()` | |
| Textos fijos de `Imp_Mesas_citacion_enc/_pie` | `CitacionDocentesTextos` | Erratas corregidas ("Superiodidad", "dirigime", "debera"). |
| `CARPETA_FIRMAS\firma_recto/secre/direc.jpg` | `Institucion:FirmaRectorPath/FirmaSecretariaPath/FirmaDirectorEstudiosPath` | Vacías por defecto: sin archivo se imprime solo nombre y cargo (equivale a "Imagen Firma: No"). |
| Dibujo GDI de la citación (membrete, original/duplicado, tabla, pie) | `CitacionDocentesPdfService` (QuestPDF, A4) | Membrete = `Institucion:MembreteConstanciaPath` (el legacy siempre lo cargaba). Copias en hojas separadas; la tabla repite su encabezado si desborda. |
| Dibujo GDI del parte diario | `ParteDiarioMesasPdfService` (QuestPDF, A4) | Una hoja por fecha; subtítulo por carrera con código y nombre. |

## 3. Decisiones

- **Carrera de firma explícita** en vez de la carrera activa global (§2.3): el operador la
  elige; la página la precarga con la primera carrera filtrada.
- **Imágenes de firma por configuración**, no por usuario: son assets institucionales
  (una por cargo), a diferencia de la firma del correo por comisión (deuda del hito 12).
- **Rango de docentes**: un solo extremo se rechaza con mensaje (el legacy lo ignoraba en
  silencio).
- **Sin Excel**: el legacy no lo tenía para estos reportes.

## 4. Verificación (checklists de `migration_prompts.md`)

- **1.B**: SQL parametrizado ✅ (listas de carreras por expansión de Dapper) · `Async` +
  `CancellationToken` ✅.
- **2.A**: trazabilidad ✅ · un validador por comando y `Result<T>` ✅ · sin globales
  (autoridades por consulta, fecha por `TimeProvider`) ✅ · solo lectura.
- **3.A/3.B**: cero SQL/Infrastructure en los `.razor` ✅ · `MudGrid` con breakpoints ✅ ·
  rutas propias + `[Authorize]` + entradas en Planillas ✅ · no usa `EsbaListView`: son
  generadores de PDF, no listados (misma justificación que carpetas y hito 16).
- **4.A/4.B**: `FormatoMesaTests` (15 casos), `GenerarImpresionesMesasHandlerTests` (12:
  validaciones, carrera de firma inexistente, sin datos, agrupación por docente, firmante
  por opción, cortes por fecha/carrera), `ImpresionesMesasPdfServiceTests` (3 smoke) y
  `ImpresionesMesasQueryEquivalenciaTests` (Category=Integration: docentes citados, mesas
  por docente y parte diario contra el SQL legacy sobre la base local) — en verde el
  2026-09-29.

## 5. Pendientes

- Cargar las imágenes de firma reales en `wwwroot/plantillas/` y apuntar las tres rutas de
  `Institucion` en la configuración del stack (hito 12.6).
