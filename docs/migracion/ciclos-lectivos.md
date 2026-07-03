# Ciclos lectivos (TBL_CUAT / TBL_TRIM) — ABM de cuatrimestres

Cierra la última opción deshabilitada del grupo Académica (`/academica/cuatrimestres`,
2026-07-02). Sucesor de `Formulario Carga de trimestres/CargadeTrimestres.pas`
(`TFrmCargaTri`), el ABM de las fechas de los ciclos lectivos. La Etapa 1 (entidades
`CicloCuatrimestral`/`CicloTrimestral` + configuración EF) venía del hito 3; acá se
agregaron las Etapas 2–4.

## Trazabilidad legacy → C#

| Legacy (`CargadeTrimestres.pas`) | Artefacto C# |
|---|---|
| `FormActivate` — `SELECT ... FROM TBL_CUAT` / `TBL_TRIM` a la grilla kbmMemTable | `CiclosLectivosQuery` (Dapper) + `ICiclosLectivosQuery.Listar{Cuatrimestrales,Trimestrales}Async` |
| `GrabamesaClick` — `DELETE FROM <tabla>` completo + reinsert fila por fila, sin validación | `GuardarCicloCuatrimestralHandler` / `GuardarCicloTrimestralHandler`: upsert **por año** con FluentValidation (desde < hasta por período, períodos sin superposición, año 1980–2100, duplicado/inexistente según alta/edición) |
| Borrar fila de la grilla en memoria (quedaba fuera del reinsert) | `EliminarCicloLectivoHandler` + confirmación en la pantalla |
| Variable global `modo` (`'CUATRIMESTRAL'`/`'TRIMESTRAL'`) que reconfiguraba el único formulario | Tabs de `Cuatrimestres.razor` + parámetro `ModoTrimestral` de `CicloLectivoFormDialog` |

## Decisiones

- **No se replicó el delete-all + reinsert**: cada guardado toca solo su año, en la
  transacción del caso de uso (`IUnitOfWork`).
- `EliminarCicloLectivoHandler` no tiene validador: la única regla (que el año exista)
  es la verificación de existencia del propio handler.
- Sin tests de integración de equivalencia: no interviene ningún SP `XXX_*` y el
  comportamiento legacy es CRUD puro sin lógica.
- Los errores de validación se muestran en un `MudAlert` al tope del diálogo (mensaje
  unificado del `Result<T>`), igual que el resto de los form-dialogs del sistema.
- `[Authorize]` sin política por área: el enforcement por `MNUOPC` es transversal y
  queda para el hito 12.3, como en todas las pantallas.

## Tests

`CicloLectivoValidatorsTests` (reglas positivo/negativo) y `CicloLectivoHandlersTests`
(alta, duplicado, edición, inexistente, inválido sin tocar repo, eliminación, commit
exactamente una vez) en `tests/Esba.Application.Tests/Academica/`.
