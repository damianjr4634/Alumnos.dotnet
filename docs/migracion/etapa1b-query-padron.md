# Etapa 1.B — Query Dapper del padrón de alumnos

> Migración del SELECT que `FrmEsba.pas:527-553` armaba **concatenando `.Text` de controles**
> (el caso emblemático de SQL injection del legacy). Implementación:
> [AlumnosQuery.cs](../../src/Esba.Infrastructure/Queries/AlumnosQuery.cs); contrato:
> [IAlumnosQuery](../../src/Esba.Application/Abstractions/IAlumnosQuery.cs).

## Mapeo control legacy → parámetro

| Control de FrmEsba | Parámetro / propiedad del filtro | Notas |
|---|---|---|
| `TxtBusqueda` ("apellido" o "apellido:nombre") | `@Apellido`, `@Nombre`, `@Texto`, `@TextoMayusculas` | el término también matchea mail (case-insensitive) y código |
| `TxtBusqueda` con prefijo `_` (`_CARRE`) | `CodigoCarrera` → `@CodigoCarrera` | el parseo del atajo queda en la UI |
| Parámetro `ACodAlu` (volver de otra pantalla) | `@CodigoAlumno` | búsqueda exacta, ignora el resto |
| `chbBuscarBajas` | `@Baja` ('S'/'N') | el padrón muestra solo activos o solo bajas |
| `cbCarreDesuso` | fragmento `AND C.DESACT = 'N'` en el JOIN | sin tilde se excluyen carreras en desuso |
| Globals `Superv`/`CodUsu` (FuncionesConfiguracion) | `EsSupervisor` / `@CodigoUsuario` | vendrán de claims; no supervisor → JOIN `BARRA_SEGU` |
| — (nuevo) | `@Skip`, `@Take` | paginación server-side; el legacy cargaba todo a kbmMemTable |

**No incluido a propósito**: el `LEFT JOIN XXX_OBSERV_PANTA(A.INDICE)` del legacy (color/mensaje
por alumno para la ficha rápida). Es un SP `XXX_*` → va por wrapper tipado (Prompt 2.B), no
embebido en la query de listado. `// TODO-migrar XXX_OBSERV_PANTA`

## SQL final (caso típico: búsqueda por texto, usuario no supervisor)

```sql
SELECT TRIM(A.COD_ALU)  AS Codigo,
       TRIM(A.CARRE)    AS CodigoCarrera,
       TRIM(A.MATRIZ)   AS Matriz,
       TRIM(A.APELLIDO) AS Apellido,
       TRIM(A.NOM_APE)  AS Nombre,
       TRIM(A.MAIL)     AS Mail,
       IIF(A.BAJA = 'S', TRUE, FALSE) AS Baja,
       TRIM(C.DESCARRE)   AS NombreCarrera,
       TRIM(C.RESOLUCION) AS Resolucion,
       IIF(C.DISTANCIA = 'S', 'DISTANCIA', 'PRESENCIAL') AS Modalidad
FROM ALUMNOS A
JOIN CARRERA C ON C.CARRE = A.CARRE AND C.DESACT = 'N'
JOIN BARRA_SEGU S ON S.BAROPC = A.CARRE AND S.CODUSU = :CodigoUsuario
WHERE ((A.APELLIDO CONTAINING :Apellido AND A.NOM_APE CONTAINING :Nombre)
       OR UPPER(A.MAIL) CONTAINING :TextoMayusculas
       OR A.COD_ALU CONTAINING :Texto)
  AND A.BAJA = :Baja
ORDER BY C.DESCARRE, A.APELLIDO, A.NOM_APE, A.CARRE, A.COD_ALU
OFFSET :Skip ROWS FETCH NEXT :Take ROWS ONLY
```

El total para la paginación se calcula con `SELECT COUNT(*)` sobre el mismo FROM/WHERE.

## Parámetros (para probar en isql)

| Parámetro | Tipo | Ejemplo |
|---|---|---|
| `:CodigoUsuario` | INTEGER | `1` (omitir el JOIN si supervisor) |
| `:Apellido` | VARCHAR | `'PEREZ'` |
| `:Nombre` | VARCHAR | `'JUAN'` (solo si el texto tenía `:`) |
| `:TextoMayusculas` | VARCHAR | `'PEREZ'` |
| `:Texto` | VARCHAR | `'PEREZ'` |
| `:Baja` | CHAR(1) | `'N'` |
| `:Skip` / `:Take` | INTEGER | `0` / `50` |

Variantes: con `@CodigoAlumno` la condición de texto se reemplaza por `A.COD_ALU = :CodigoAlumno`;
con `@CodigoCarrera`, por `A.CARRE = :CodigoCarrera`.

## Checklist de aceptación 1.B

- [x] Ningún valor de usuario viaja interpolado en el SQL (solo se concatenan fragmentos constantes).
- [x] El método es `async`, termina en `Async` y acepta `CancellationToken`.
- [x] La paginación se hace en el servidor Firebird (`OFFSET/FETCH`), no en memoria.

## Verificación

5 tests de integración contra la base real (`Category=Integration`): página+total, equivalencia
de búsqueda por apellido contra un alumno real, paginación estable y sin solapamiento, filtro de
bajas, y guarda de no-supervisor sin código de usuario.
