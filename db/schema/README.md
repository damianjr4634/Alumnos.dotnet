# Esquema Firebird — Sistema ESBA (legacy)

DDL extraído de la base de datos real, cumpliendo la regla 🔴 de
[migration_improvements.md §1.3](../../migration_improvements.md) (esquema versionado desde el día 1).

## Procedencia

| Dato | Valor |
|---|---|
| Fecha de extracción | 2026-06-12 |
| Base origen | `/var/firebird/esba_restore.gdb` (restore local de `esba.gdb` de producción) |
| Servidor | Firebird 3.0.11 (SuperServer, localhost) |
| Comando | `isql-fb -x -u sysdba localhost:/var/firebird/esba_restore.gdb` |

## Contenido

- `00_full_ddl.sql` — extracto completo tal cual lo genera `isql -x`: dominios, 25 generadores,
  tablas, constraints, índices, vistas, 46 triggers, stubs y cuerpos de los 104 stored procedures,
  excepción `E_CUSTOM_ERR` y comentarios de columnas.
- `procedures/<NOMBRE>.sql` — un archivo por stored procedure (cuerpo completo `ALTER PROCEDURE`
  envuelto en `SET TERM`), para trazabilidad individual durante la migración (Etapa 2.B, wrappers
  tipados) y la fase 5 (retiro de SPs).

## Familias de procedimientos

| Prefijo | Cantidad aprox. | Rol |
|---|---|---|
| `XXX_*` | mayoría | Lógica de negocio invocada desde el cliente Delphi (ver tabla en [delphi_structure.md §3](../../delphi_structure.md)). Muchos devuelven `ERRCOD`/`ERRMSG` o `FERRCOD`/`FERRMSG` → se mapean a `Result<T>`. |
| `XXX_ADM_*` | ~15 | Módulo administrativo/cuotas (cuenta corriente, recibos, pagos). **Fuera de alcance** (decisión 2026-06-12): no se migran. |
| `WEB_*` | ~9 | Backend del portal web del alumno (login, inasistencias, notas, permisos, sanciones). `WEB_NET_*` sugiere una integración .NET previa. |
| `INFORME_*` | ~11 | Reportes/planillas. |
| `AAAAAAAA`, `AAAAAAAA2`, `AAA_*` | 3 | Scripts de mantenimiento puntual para arreglar información (remapeo de códigos CISE22, bajas masivas). **No se migran** (decisión 2026-06-12). |

## Otras observaciones del esquema

- Las tablas `AspNetUsers`, `AspNetRoles`, etc. son de una integración ASP.NET Identity **vieja:
  se pueden pisar** (decisión 2026-06-12); no condicionan la autenticación nueva.
- Tablas `$$$CURSADA`, `$$$PERMEXA`, `BKP_*`, `AAA_CAMBIOS`, generador `BORRAR`: procesos internos
  para arreglar información. **No se toman en cuenta** en el modelo nuevo (decisión 2026-06-12).
- Regenerar este extracto: repetir el comando de la tabla de procedencia y volver a correr el split
  de procedimientos.
