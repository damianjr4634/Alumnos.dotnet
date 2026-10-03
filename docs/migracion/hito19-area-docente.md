# Hito 19 — Área docente: precarga de notas por comisión y por mesa

> Estado: **diseño cerrado, esquema aplicado en dev y producción (2026-10-03)** y shell
> del área listo (12.3.1 ✅: policies por perfil, `DocenteLayout`, `/docente` con las
> comisiones y mesas del titular y el estado de su precarga; ver
> `hito12-endurecimiento.md`). Pendientes: las pantallas de precarga (Etapas 1–4 abajo).

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

## Plan de implementación (Etapas 1→4)

1. **Etapa 1**: entidades `CargaComisionDocente`/`CargaComisionDocenteDetalle`,
   `CargaMesaDocente`/`CargaMesaDocenteDetalle` + enum `EstadoCargaDocente` (BOR/FIN/EFE) +
   configuraciones EF; queries Dapper: comisiones del docente (`COMARM` por `CODPROFES` +
   ciclo), alumnos de la comisión (`CURSADA` cursando/recursando, prefill de la carga si
   existe), mesas del docente (`MESAS.TITULAR`), alumnos con permiso (`PERMEXA`).
2. **Etapa 2**: `GuardarCargaComisionHandler` (crea la cabecera al primer guardado; rechaza
   si no es el titular o si el estado no es `BOR` y el usuario no es secretaría),
   `FinalizarCargaHandler`, `ReabrirCargaHandler` (solo secretaría), `EfectivizarCarga*Handler`
   (solo secretaría; delega en los handlers de los hitos 14/15). Mismo juego para mesas.
3. **Etapa 3**: `/docente` (home: mis comisiones y mis mesas), `/docente/comisiones/{...}`,
   `/docente/mesas/{carre}/{mesa}` con `DocenteLayout`; en secretaría, columna "Precarga" en
   regularización por comisión y en carga de notas de finales + acciones reabrir/efectivizar.
4. **Etapa 4**: unitarios de handlers/validadores (titularidad, estados, permisos de
   secretaría); integración: roundtrip EF↔Dapper y equivalencia "efectivizar = mismo resultado
   que cargar a mano".

## Pendientes / a confirmar

- Qué ciclo lectivo muestra el docente por defecto (asumo el vigente de `TBL_CUAT`, con
  selector).
- Si un alumno aparece en `CURSADA` después de creada la carga (inscripción tardía), la fila
  de detalle se crea al vuelo al guardar (no hay detalle hasta que el docente escribe algo).
- Notificación a secretaría al finalizar (hoy: lista de pendientes; correo opcional más
  adelante).
