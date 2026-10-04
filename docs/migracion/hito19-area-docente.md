# Hito 19 — Área docente: precarga de notas por comisión y por mesa

> Estado: **COMPLETO ✅ 2026-10-04.** Esquema aplicado en dev y producción (2026-10-03/04),
> shell del área (12.3.1), precarga por comisión y por mesa del lado docente, y lado
> secretaría (pendientes, "Tomar valores", reabrir, efectivizar). El circuito
> alta → cursado → **precarga del docente** → regularización / final queda cerrado.

## Lado secretaría — hecho (2026-10-04)

| Pieza | Artefactos |
|---|---|
| Listado de pendientes | `/academica/precargas-docentes` (`PrecargasDocentes.razor`, menú Académica › Precargas de docentes): dos pestañas (Comisiones / Mesas) con `EsbaListView` + `EsbaFilterPanel` (carrera, estado —por defecto **Finalizadas**—, docente/materia); `IPrecargasDocenteQuery` / `PrecargasDocenteQuery` (Dapper, server-side; finalizadas primero, luego borradores, luego efectivizadas; respeta las carreras permitidas del usuario). Acciones: **Abrir** (navega a la pantalla real con la comisión/mesa en la URL) y **Reabrir** |
| Regularización por comisión | `RegularizacionComision.razor` acepta `?carrera&cutuco&cuatrimestre&materia` y busca sola; si hay precarga muestra el panel (docente, estado, alumnos con valores, observaciones) con **Tomar valores del docente** (vuelca los campos de la variante sobre la grilla, que sigue editable) y **Reabrir**; al procesar la regularización ofrece **marcar la precarga como efectivizada** (`EfectivizarCargaComisionHandler`) |
| Notas de finales | `CargaNotasFinales.razor` acepta `?carrera&mesa`; mismo panel; "Tomar valores" pone la nota del docente en el **llamado vigente** del alumno con la fecha de la mesa (los **ausentes no reciben nota**: secretaría decide); al procesar la mesa ofrece efectivizar (`EfectivizarCargaMesaHandler`) |
| Volcado | `PrecargaAplicador` (Application): un método por variante (terciaria / bachillerato / secundario / CNA / mesa), solo alumnos con detalle, solo los campos que edita cada variante; devuelve cuántos se aplicaron, cuántos no tenían precarga y cuántos ausentes |
| Efectivizar | `EfectivizarCargaComision/MesaHandler`: solo secretaría, desde borrador o finalizada, no dos veces (`AutorizacionCargaDocente.MotivoNoPuedeEfectivizar`). **No toca CURSADA**: la regularización/final real ya la hicieron los handlers de los hitos 15/14; esto cierra el circuito de la precarga (sale de pendientes, el docente la ve como procesada) |
| Tests | `EfectivizarCargaHandlersTests`, `PrecargaAplicadorTests`; roundtrips de comisión y mesa extendidos con el listado de secretaría y el efectivizar contra Firebird real; smoke HTTP de las tres pantallas con precargas finalizadas reales |

**Decisión de diseño:** efectivizar es un paso explícito (diálogo sí/no tras procesar), no
automático: secretaría puede haber procesado la comisión sin tomar los valores del docente,
y en ese caso decide ella si la precarga se da por consumida. Equivalencia "efectivizar =
cargar a mano" no hace falta como test aparte: los valores pasan por los mismos handlers y
validadores que la carga manual (la grilla queda editable después de tomarlos).

## Precarga por mesa — hecho (2026-10-04)

Mismo patrón que la comisión, con `AutorizacionCargaDocente` compartida (las dos cabeceras
implementan `ICargaDocente`: titular + estado):

| Capa | Artefactos |
|---|---|
| Domain | `CargaMesaDocente` (cabecera: `CodigoDocente` = `MESAS.TITULAR`, estado, auditoría) y `CargaMesaDocenteDetalle` (`Nota`, `Ausente`, `Observaciones`, `PermisoIndice`) |
| Infrastructure | `CargaMesaDocenteConfiguration` (+ detalle, `AUSENTE` 'S'/'N' con `FbConverters.SiNo`), `CargaMesaDocenteRepository`, `CargaMesaDocenteQuery` (MESAS + tribunal + carga; alumnos = `PERMEXA` de la mesa ⨝ `ALUMNOS` activos ⨝ `CURSADA` para la condición actual) |
| Application | `ClaveMesa`, `CargaMesaDocenteDto`/`AlumnoCargaMesaDto`, `GuardarCargaMesaCommand` (+ validador: nota vacía o en [1,10] como `CargaNotasFinalValidator`; ausente excluye nota), `Guardar`/`Finalizar`/`ReabrirCargaMesaHandler` |
| Web | `/docente/mesas/{carre}/{mesa}` (`CargaMesa.razor`): fila por alumno con permiso (condición, nota, ausente, observación), Enter baja a la nota del siguiente alumno salteando ausentes; acceso desde la home. |
| Tests | `CargaMesaHandlersTests` (validador + handlers), `CargaMesaRoundtripTests` (mesa real con ≥2 permisos activos y sin carga previa) |

**Universo de alumnos de una mesa** = `PERMEXA` de la mesa con `ALUMNOS.BAJA='N'`, igual en la
home (cantidad de inscriptos) y en la carga. Secretaría, al efectivizar, usará su propio
candidato (`XXX_MESAS_ALUMNOS` por tipo de examen, hito 14) y cruzará por alumno+materia.

## Precarga por comisión — hecho (2026-10-03)

| Capa | Artefactos |
|---|---|
| Domain | `EstadoCargaDocente` + `EstadoCargaDocenteCodigo` (BOR/FIN/EFE, fail-closed); entidades `CargaComisionDocente` (cabecera: estado, titular, auditoría, `Finalizar/Reabrir/Efectivizar/RegistrarModificacion`) y `CargaComisionDocenteDetalle` (mismos campos que CURSADA, `EstaVacio`) |
| Infrastructure | `CargaComisionDocenteConfiguration` (+ detalle, FK cascada), `CargaComisionDocenteRepository` (EF), `CargaComisionDocenteQuery` (Dapper: COMARM + carga + alumnos cursando/recursando activos con valores de CURSADA y del detalle), `AreaDocenteQuery` (home) |
| Application | `ClaveComision`, `ActorCargaDocente` (de los claims: usuario, es secretaría, CODPROFES), `CargaComisionDocenteDto`/`AlumnoCargaComisionDto`, `GuardarCargaComisionCommand` (+ `GuardarCargaComisionValidator`: mismas reglas que la regularización, notas [1,10] o 99), `CambiarEstadoCargaComisionCommand`; `AutorizacionCargaDocente` (reglas de quién puede qué), `GuardarCargaComisionHandler` (crea cabecera con el titular del momento, upsert de detalles, ignora alumnos ajenos a la comisión, fila vacía nueva no se crea), `FinalizarCargaComisionHandler` (BOR→FIN), `ReabrirCargaComisionHandler` (solo secretaría; FIN/EFE→BOR, Warning si venía de EFE) |
| Web | `/docente/comisiones/{carre}/{cutuco}/{codMat}/{cuaAnio}` (`CargaComision.razor`): grilla editable por alumno (1°/Rec1/2°/Rec2/3°, horas, inasistencias, justificadas, observación), observaciones generales, "Guardar borrador" y "Finalizar carga" (confirmación; guarda y después cambia el estado). Sin detalle, la fila arranca con lo que CURSADA tiene hoy. Solo lectura si no es titular o la carga está FIN/EFE (el servidor igual deniega). Acceso desde la home (`/docente`, click en la fila o botón) |
| Tests | Unitarios: `GuardarCargaComisionValidatorTests`, `CargaComisionHandlersTests` (titularidad, estados, secretaría siempre, sin commit en error), `EstadoCargaDocenteCodigoTests`. Integración: `CargaComisionRoundtripTests` (comisión real con cursantes activos y sin carga previa: guardar → relectura Dapper → finalizar → titular bloqueado → reabrir → estado visible en la home; limpieza por cascada). Smoke HTTP de la página con un docente temporal |

**Campos por variante** (corrección 2026-10-04, `EsquemaCargaCursado` en dominio): la grilla
del docente pide exactamente lo que secretaría edita en la regularización de esa carrera
(misma decisión de variante que `RegularizacionComision.razor`): **terciaria** (TIPO=TER) 1°
y 2° parcial, recuperatorio, horas, inasistencias, justificadas · **bachillerato** (BAC) 1° y
2° bimestre, recuperatorio, "a regularizar" (`REGULAR`), horas, inasistencias, justificadas ·
**secundario** (333/650) 1°, 2° y 3° trimestre, horas, inasistencias (diciembre/marzo son de
secretaría) · **CNA** nota final (`FINAL1`) · otras carreras: precarga no disponible. Para
`REGULAR` y `FINAL1` hizo falta la migración
`2026-10-04_doc_carga_comision_det_regular_final.sql` (la primera versión copiaba las columnas
de CURSADA, con dos recuperatorios que ninguna pantalla usa; `RECUP2` queda sin uso).

**Universo de alumnos** de una comisión = CURSADA con `CONDICION` CURSANDO/RECURSANDO y
`ALUMNOS.BAJA='N'`, igual en la home (cantidad) y en la carga (filas). Detectado y
corregido en el smoke: una comisión de 2021 cuyo único cursante está de baja mostraba 1 en la
home y ninguna fila en la carga.

## Qué es

Los usuarios de tipo **docente** (`USUARIOS.TIPO='DOC'`, vinculados a `DOCENTES.CODPROFES`)
entran a un área propia con dos opciones:

1. **Comisiones**: los cursos que tiene a cargo (`COMARM.CODPROFES`). Lista a todos los alumnos
   de la comisión y permite **precargar** las notas del cursado, las horas dictadas y las
   inasistencias, igual que la pantalla de regularización, pero **sin regularizar**.
2. **Mesas**: las mesas de examen en las que es **titular** (`MESAS.TITULAR`). Precarga la nota
   de final (o ausente) de cada alumno con permiso.

**Secretaría** lee lo precargado, lo puede corregir, y lo **efectiviza** sobre las tablas reales
con los flujos que ya existen: regularización (hito 15) para las comisiones y carga de notas de
finales (hito 14) para las mesas. La condición del alumno la siguen decidiendo esos handlers;
el área docente no agrega lógica de condición.

## Decisiones (2026-10-03, con el usuario)

| Tema | Decisión |
|---|---|
| Almacenamiento | **Juego de tablas nuevo** con columnas tipadas. Descartados: columnas auxiliares en `CURSADA`/`PERMEXA` (mezcla borrador con verdad; el Delphi las escribe) y JSON (Firebird 4 no tiene tipo JSON: no se filtra, no se valida, no se exporta). Precedente legacy: `WEB_INSCRIPCIONES`/`WEB_PERMEXA` (pedidos web que secretaría procesa). |
| Dónde vive el estado | En la **carga** (una por comisión, una por mesa), no por alumno. |
| Estados | `BOR` borrador (el docente edita) → `FIN` finalizado por el docente (sigue viendo la comisión, no toca nada) → `EFE` efectivizado por secretaría. |
| Quién carga | Solo el **titular** (comisión: `COMARM.CODPROFES`; mesa: `MESAS.TITULAR`). Los vocales no. |
| Inasistencias | **Totales** del cuatrimestre (`TOT_HORAS`, `INASIST`, `JUSTIF`), como la regularización. No fecha a fecha. |
| Después de `FIN` | El docente no cambia nada. Solo si secretaría **reabre** (`FIN`→`BOR`) puede seguir cargando. |
| Poderes de secretaría | **Siempre** puede: modificar los valores cargados, reabrir, efectivizar. |
| Reutilización | Efectivizar = correr `ConfirmarRegularizacion*Handler` / `ConfirmarCargaNotasFinalHandler` con los valores del borrador y marcar `EFE`. Las pantallas actuales de secretaría ganan una columna "Precarga del docente" y un botón "Tomar valores". |

## Esquema

Migración **`db/schema/migrations/2026-10-03_doc_cargas_borrador.sql`** (idempotente; probada
sobre Firebird 4.0: doble ejecución, triggers de identidad, CHECK de estado, unicidad por
comisión, borrado en cascada). Sin FK hacia tablas legacy (el escritorio las reescribe);
sí FK cabecera→detalle.

| Tabla | Clave natural | Contenido |
|---|---|---|
| `DOC_CARGA_COMISION` | `(CARRE, CUTUCO, COD_MAT, CUA_ANIO)` única | cabecera: `CODPROFES` titular, `ESTADO`, `OBSERV`, auditoría (alta / modif / final / reapertura / efectivo: `FEC_*` + `CODUSU_*`) |
| `DOC_CARGA_COMISION_DET` | `(CARGA_ID, COD_ALU)` única | `TP_EVA`, `RECUP`, `TP_EVA2`, `RECUP2`, `TP_EVA3`, `TOT_HORAS`, `INASIST`, `JUSTIF`, `OBSERV`, `CURSADA_INDICE` (ayuda), `FEC_MODIF`/`CODUSU_MODIF` |
| `DOC_CARGA_MESA` | `(CARRE, MESA)` única | cabecera análoga, `CODPROFES` = titular de la mesa |
| `DOC_CARGA_MESA_DET` | `(CARGA_ID, COD_ALU, COD_MAT)` única | `NOTA`, `AUSENTE` ('S'/'N'), `OBSERV`, `PERMEXA_INDICE` (ayuda), `FEC_MODIF`/`CODUSU_MODIF` |

IDs por generador `G_DOC_CARGA_*` + trigger `*_BI0`, como el resto del esquema. Índices
`(CODPROFES, ESTADO)` en ambas cabeceras para "mis cargas" y "pendientes".

El `CODUSU_MODIF` del detalle distingue si el último que tocó la fila fue el docente o
secretaría (ambos son `USUARIOS`).

## Pendientes / a confirmar

- Qué ciclo lectivo muestra el docente por defecto (asumo el vigente de `TBL_CUAT`, con
  selector).
- Si un alumno aparece en `CURSADA` después de creada la carga (inscripción tardía), la fila
  de detalle se crea al vuelo al guardar (no hay detalle hasta que el docente escribe algo).
- Notificación a secretaría al finalizar (hoy: lista de pendientes; correo opcional más
  adelante).
