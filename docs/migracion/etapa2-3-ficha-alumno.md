# Etapas 2+3 — Ficha del Alumno (FrmAltaAlumno → casos de uso + página Blazor)

## Tabla de trazabilidad (Prompt 2.A)

| Método/elemento Delphi (FrmAltaAlumno.pas) | Artefacto C# | Notas |
|---|---|---|
| `grabarClick` (validación `CbEstado.ItemIndex=-1`) | `CrearAlumnoValidator` / `ActualizarAlumnoValidator` (`Estado.IsInEnum`) + reglas nuevas de longitud/formato del DDL (§2.4) | El legacy validaba casi nada; las longitudes pasan de "constraint de BD como única barrera" a validación previa |
| `grabarClick` (INSERT INTO ALUMNOS) | `CrearAlumnoHandler` | `BAJA='N'` al nacer, `USUARIO` del usuario logueado (claim → parámetro), COD_ALU armado con VO `CodigoAlumno` (zero-padding del legacy) |
| `grabarClick` (UPDATE ALUMNOS) | `ActualizarAlumnoHandler` | No toca COD_ALU/CARRE/MATRIZ; foto solo se pisa si viene una nueva (regla del trigger ALUMNOS_BIU0) |
| `documentoKeyPress` (chequeo de duplicado al tipear el documento) | `CrearAlumnoHandler` (duplicado) | "ya existe en altas" / "ya existe en bajas: {nombre}", mismos mensajes |
| `MessageDlg '¿desea guardar?'` | — descartado | La confirmación previa genérica no se replica: el guardado es explícito (botón) y los errores viajan como `Result<T>` |
| Carga de campos en modo edición (vía SELECT de FrmEsba) | `IAlumnosQuery.ObtenerDetalleAsync` → `AlumnoDetailDto` | Implementado con EF (desviación justificada del default Dapper 🟡: reutiliza los conversores CHAR(1) legacy) |
| Combos (CmbSexo, estadocivil, CbGenero, CbEstado, dni) | Enums de Domain + selects en `FichaAlumno.razor` | Mismos valores persistidos (F/M, S/C/D/V, C/P/E/N, índice 0-16, dígito 0-3) |
| Tabs del TPageControl (datos/estudios/laborales/documentación) | `MudTabs` en `FichaAlumno.razor` | Responsive con MudGrid, sin posiciones absolutas (§3.4) |
| `KeyPress` handlers (foco con Enter, solo números) | — descartados (solo-UI) | El browser/MudBlazor resuelven la navegación; documento con `Matches("^[0-9]{1,8}$")` |
| Carga/borrado de foto (`OPDFoto`, stream JPG/BMP) | ⚠️ pendiente — `// TODO-migrar foto-upload` | La ficha muestra la foto existente; subirla requiere endpoint autenticado (§3.4 ⚪) |
| Solapa "Datos por Web" (`TsWeb`, WCmbSexo etc.) | ⚠️ pendiente | Comparación de datos cargados por el alumno vía web — depende del módulo Web |
| Checkbox `Pn` | — no migrado | Sin persistencia detectada en el SQL del legacy (⚠️ confirmado en etapa 1) |

## Cómo se conecta (vertical slice)

Búsqueda (Home) → fila seleccionada → **Ficha** (`/alumnos/{carre}/{codigo}/ficha`) o
**Nuevo alumno** → diálogo de carrera (`SeleccionarCarreraDialog`, patrón canónico
"elegir carrera al entrar") → alta (`/alumnos/{carre}/ficha/nueva`).

La validación por campo reutiliza el MISMO validador FluentValidation del caso de uso
(modelo de presentación → command → `IncludeProperties(propiedad)`), y el servidor
revalida siempre dentro del handler (§2.4.1).

## Checklists

**2.A**: trazabilidad ✅ · un validador por escritura y `Result<T>` ✅ · transacción única
por caso de uso (`IUnitOfWork.SaveChanges` una vez, verificado por tests) ✅ · usuario por
parámetro desde claims ✅ · todo async con `CancellationToken` ✅

**3.A**: cero SQL/DbContext en el `.razor` ✅ · `MudGrid` con breakpoints, sin tamaños
absolutos ✅ · errores de validación junto a cada campo ✅ · semántica `Result<T>`
implementada (Ok→Snackbar, Warning→Snackbar, Error→Alert) ✅ · ruta propia con `[Authorize]`
global ✅ (política por área: pendiente hasta definir el mapa de políticas — `// TODO-migrar`)

## Verificación

- `dotnet build` 0 warnings; 61 tests en verde (17 nuevos: validador + 2 handlers).
- Smoke E2E con la app corriendo: login → ficha de un alumno real (200, datos cargados) →
  página de alta (200).
