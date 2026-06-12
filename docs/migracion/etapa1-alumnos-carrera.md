# Etapa 1 — Mapeo CARRERA y ALUMNOS (Firebird → Domain/EF Core)

> Tabla de revisión humana exigida por el Prompt 1.A de [migration_prompts.md](../../migration_prompts.md).
> Fuentes: DDL real ([db/schema/00_full_ddl.sql](../../db/schema/00_full_ddl.sql)) + análisis de
> `FrmEsba.pas` (padrón y SELECT de detalle) y `FrmAltaAlumno.pas/.dfm` (alta/edición).
> ⚠️ = decisión dudosa que conviene validar con el usuario.

## Decisiones generales

- **Sin FK físicas**: la base legacy no declara ninguna FK entre sus tablas (las únicas son de las
  AspNet* viejas). La relación `Alumno → Carrera` se modela solo en EF Core para joins; no se
  agregan constraints a la base.
- **CHAR(1) con espacio en blanco**: los datos contienen blanco además de NULL para "sin dato";
  todos los conversores de lectura tratan blanco como `null` (validado contra la base real).
- **Charset `ISO8859_1`** en la connection string: las columnas son `CHARACTER SET NONE` y el
  legacy escribió bytes ISO-8859-1. ⚠️ Confirmar con datos con acentos/ñ reales.
- Conversores reutilizables en `FbConverters`: `SiNo` ('S'/'N' ↔ bool), `Asterisco` ('*'/null ↔
  bool), y enums por carácter.

## CARRERA → `Esba.Domain.Entities.Carrera`

| Columna | Propiedad | Tipo C# | Notas |
|---|---|---|---|
| CARRE (PK) | Codigo | string | VARCHAR(6) |
| DESCARRE | Nombre | string? | |
| TIPO | Tipo | string? | tipo de carrera: bachiller, bachiller a distancia, terciaria (confirmado) |
| CUATRIM | Cuatrimestres | short? | cantidad de cuatrimestres del plan |
| RESOLUCION | Resolucion | string? | |
| CAMINO | Camino | string? | ⚠️ parece path legacy de plantillas |
| DESCARRE2 | NombreAlternativo | string? | usado en impresiones |
| CUATRIM2 | Cuatrim2 | string? | ⚠️ semántica a confirmar |
| RESOLU2 | Resolucion2 | string? | |
| RECTOR / DNIRECTOR | Rector / DniRector | string? | constancias |
| SECRETARIA / DNISECRET | Secretaria / DniSecretaria | string? | constancias |
| USUARIO | Usuario | short? | ⚠️ ¿último usuario que modificó? |
| CASO | — no mapeada | — | columna muerta confirmada (2026-06-12) |
| INSTITUT / CARACT | Instituto / Caracteristica | string? | |
| DIRESTU / DNIDIRESTU | DirectorEstudios / DniDirectorEstudios | string / string? | DIRESTU NOT NULL |
| GRUPO | Grupo | string | FK implícita a CARRE_GRP — `// TODO-migrar CARRE_GRP` |
| ORDEN / IMAGEN / DESCORT | Orden / Imagen / NombreCorto | short? / int? / string? | menú legacy |
| DISTANCIA | EsADistancia | bool | 'S'/'N'; FrmEsba lo muestra como "MODALIDAD" |
| DESACT | Desactivada | bool | trigger CARRERA_BI0 default 'N'; filtro "en desuso" del padrón |
| ORDENINF | OrdenInforme | short | NOT NULL |
| DICTAMEN | Dictamen | string? | |
| REGSOLAPA | RegSolapa | short | sin uso (confirmado); se mantiene mapeada solo por ser NOT NULL sin default — se persiste 0 |
| DURACION | Duracion | string? | |
| IDIOMA | Idioma | string | NOT NULL |

## ALUMNOS → `Esba.Domain.Entities.Alumno`

PK compuesta **(CARRE, COD_ALU)**. Identidad de `INDICE`: generador `G_ALUMNOS` vía trigger
`ALUMNOS_BI0` → mapeado `ValueGeneratedOnAdd` con `// TODO-confirmar-identidad` (verificar que el
provider recupere el valor tras INSERT).

| Columna | Propiedad | Tipo C# | Notas |
|---|---|---|---|
| COD_ALU (PK) | Codigo | string | CHAR(11) = tipo doc (3) + número (8 con ceros). VO `CodigoAlumno` |
| CARRE (PK) | CodigoCarrera | string | navegación `Carrera` sin FK física |
| MATRIZ | Matriz | string? | libro (2) + folio (3). VO `LibroMatriz`. No se blanquea (trigger BU0). Solo editable vía SP `XXX_CAMBIA_DNI_LM` |
| APELLIDO / NOM_APE | Apellido / Nombre | string? | |
| EXT_POR | DocumentoExpedidoPor | string? | "expedido por" (confirmado) |
| SEXO | Sexo | Sexo? | 'F'/'M' (FrmAltaAlumno.CmbSexo) |
| NACIONAL | Nacionalidad | string? | |
| EST_CIV | EstadoCivil | EstadoCivil? | 'S'/'C'/'D'/'V' (primera letra del combo) |
| FEC_NAC / LUG_NAC / PCIA_NAC | FechaNacimiento / LugarNacimiento / ProvinciaNacimiento | DateOnly? / string? | |
| DOMI / LOCALI / COD_POS | Domicilio / Localidad / CodigoPostal | string? / string? / short? | COD_POS NUMERIC(4,0) |
| CAR_TEL / TELE / CELULAR | CaracteristicaTelefono / Telefono / Celular | string? | |
| FEC_ING | FechaIngreso | DateOnly? | |
| CPRIM/APRIM/TPRIM/OPRIM | ColegioPrimario/AnioPrimario/TituloPrimario/ObservacionPrimario | string? | OPRIM ⚠️ sin uso en el alta |
| CSECU/ASECU/TSECU/OSECU | (ídem Secundario) | string? | OSECU ⚠️ sin uso |
| TERCI/ATERCI/TTERCI/OTERCI | (ídem Terciario) | string? | OTERCI ⚠️ sin uso |
| UNIVER/AUNIVER/TUNIVER/OUNIVER | — no mapeadas | — | columnas muertas confirmadas (2026-06-12) |
| ESPEC/AESPEC/TESPEC/OESPEC | — no mapeadas | — | columnas muertas confirmadas (2026-06-12) |
| EMPRE/RUBRO/CARGO/ANTI | Empresa/Rubro/Cargo/Antiguedad | string? | datos laborales |
| DOMI_1/TELE_1/INTER | DomicilioLaboral/TelefonoLaboral/InternoLaboral | string? | |
| MOROSO/MORO1..3/F_MORO | — no mapeadas | — | columnas muertas confirmadas (2026-06-12) |
| PN | Pn | string? | ⚠️ existe checkbox "Pn" pero no se detectó persistencia |
| CTT / FECH_CTT | TieneCertificadoEnTramite / FechaCertificadoEnTramite | bool / DateOnly? | patrón '*' = marcado |
| DNI | SituacionDni | SituacionDni? | dígito '0'..'3' = índice combo (8 años/16 años/mayor/sin doc) |
| CD / RESIDE | — no mapeadas | — | columnas muertas confirmadas (2026-06-12) |
| CR | Cr | string? | usado solo en export a Ministerio |
| CA | PresentoCa | bool | patrón '*' |
| BAJA | Baja | bool | 'S'/'N' NOT NULL; el alta nace 'N' |
| INDICE | Indice | int? | único; lo usa `XXX_OBSERV_PANTA` |
| MAIL / OBSERV / USUARIO | Mail / Observaciones / Usuario | string? | USUARIO = quien modificó (irá por claims) |
| FFOTO/FAPTFIS/FAPTFEC | PresentoFoto/PresentoAptoFisico/FechaAptoFisico | bool?/bool?/DateOnly? | 'S'/'N' |
| FUSUWEB | UsuarioWeb | string? | usuario del portal web |
| FOTO | Foto | byte[]? | BLOB; no se borra (trigger BIU0) |
| FPARNAC | PresentoPartidaNacimiento | bool? | 'S'/'N' |
| ULTMOD | UltimaModificacion | DateTime? | trigger; `ValueGeneratedOnAddOrUpdate` |
| MATRIZ_L / MATRIZ_F | — no mapeadas | — | calculadas (SUBSTRING de MATRIZ); usar VO `LibroMatriz` |
| FLIBRETA / FLIBFEC | PresentoLibreta / FechaLibreta | bool? / DateOnly? | FLIBFEC = fecha libreta (confirmado) |
| BAJAADM / ADMEST / ADMCURSO | — no mapeadas | — | módulo administrativo **fuera de alcance** (decisión 2026-06-12); triggers les dan valor |
| FFECLIB | — no mapeada | — | columna muerta confirmada (2026-06-12) |
| ESTADO | Estado | EstadoAlumno? | 'C'/'P'/'E'/'N' (mapeo explícito de CbEstado) |
| GENERO | Genero | Genero? | INTEGER = índice del combo (0..16) — no reordenar el enum |
| NOMIPASE | NominaPase | bool? | 'S'/'N' — nómina pase (confirmado) |

## Preguntas resueltas (respuestas del usuario, 2026-06-12)

1. Blancos en CHAR(1) → se leen como `null` (vacío no es dato válido). ✔ implementado.
2. UNIVER*/ESPEC*, MOROSO*, CD, RESIDE, FFECLIB, CARRERA.CASO: **muertas, no se mapean**.
   REGSOLAPA tampoco se usa pero queda mapeada por ser NOT NULL sin default.
3. NOMIPASE = nómina pase; EXT_POR = expedido por; FLIBFEC = fecha libreta;
   CARRERA.TIPO = tipo de carrera (bachiller / bachiller a distancia / terciaria).

Pendientes menores: semántica de CUATRIM2, CAMINO, USUARIO (carrera) y el checkbox PN.

## Verificación

- `dotnet build`: 0 warnings / 0 errores.
- Tests unitarios (`Result`, `CodigoAlumno`, `LibroMatriz`): en verde.
- Smoke tests de integración contra la base real (`Category=Integration`): materialización
  completa de `Carreras`, 200 `Alumnos` y join `Alumno.Carrera` — en verde.
