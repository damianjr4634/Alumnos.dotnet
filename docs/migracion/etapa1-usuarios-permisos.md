# Etapa 1 — Mapeo USUARIOS y BARRA_SEGU (Firebird → Domain/EF Core)

> Fuentes: DDL real + `sesion.pas` (login), `AltaUsuario.pas` (alta), `CambioPassword.pas`,
> `seciones.pas` (sesión única), `PermisosPorUsuario.pas` (BARRA_SEGU), `FrmEsba.pas:2818`
> (blanqueo de contraseña). Datos verificados: 42 usuarios, sin NULLs en SUPERV/CAMPASS.

## USUARIOS → `Esba.Domain.Entities.Usuario`

| Columna | Propiedad | Tipo C# | Notas |
|---|---|---|---|
| CODUSU (PK) | Codigo | int | trigger `USUARIOS_BI0` con `GEN_ID(G_USUARIOS)` — `TODO-confirmar-identidad` |
| NOMBRE | NombreUsuario | string | NOT NULL, es el nombre de login |
| PASSWD | PasswordHash | string | NOT NULL. Legacy: cifrado reversible `EncriptoCadena2`; `'/'` = blanqueada (con CAMPASS='S'). Destino: PBKDF2 con re-hash en primer login (§2.7) |
| NOMUSU | Nombres | string? | nombres reales |
| APELLIDO | Apellido | string? | |
| CARGO | Cargo | string? | |
| SUPERV | EsSupervisor | bool | 'S'/'N', trigger default 'N'. Supervisor ve todo sin filtro BARRA_SEGU |
| CAMPASS | DebeCambiarPassword | bool | 'S'/'N': fuerza cambio de contraseña en el próximo login |
| UID | SesionUid | string? | sesión única (`seciones.pas`): el login nuevo lo pisa e invalida la sesión anterior |
| IMGFIRMA | ImagenFirma | string? | archivo de firma (CARPETA_FIRMAS) para constancias |

## BARRA_SEGU → `Esba.Domain.Entities.PermisoUsuario`

| Columna | Propiedad | Tipo C# | Notas |
|---|---|---|---|
| CODUSU (PK1) | CodigoUsuario | int | navegación `Usuario` (sin FK física, como todo el esquema) |
| BAROPC (PK2) | CodigoOpcion | string | código de CARRERA (filtro del padrón) o de BARRA_OPC (menú) — `// TODO-migrar BARRA_OPC` |

## Flujos legacy relevantes para la autenticación (Fase 1 pendiente)

1. **Login** (`sesion.pas`): busca por NOMBRE, descifra PASSWD con `EncriptoCadena2(-1)` y compara.
   Si `CAMPASS='S'` fuerza el cambio de contraseña antes de entrar.
2. **Sesión única** (`seciones.pas`): al loguear se genera un UID y se guarda en `USUARIOS.UID`;
   cada transacción valida que el UID local siga siendo el de la base (detecta login concurrente).
3. **Blanqueo por administrador** (`FrmEsba:2818`): `PASSWD='/'` + `CAMPASS='S'`.
4. **Cambio de contraseña** (`CambioPassword.pas`): `PASSWD=EncriptoCadena2(nueva,1)` + `CAMPASS='N'`.

Estos cuatro flujos definen los casos de uso de la autenticación nueva: login con verificación
dual (hash nuevo o, si falla, EncriptoCadena2 → re-hash y migra), cambio forzado, blanqueo, y
sesión única opcional (regla 🟡 §2.7).

## DTOs

`UsuarioListItemDto`, `CrearUsuarioCommand` (la contraseña viaja en claro solo hasta el caso de
uso, que la hashea), `ActualizarUsuarioCommand` (sin contraseña: el cambio/blanqueo son flujos
aparte, como en el legacy) y `AsignarPermisosUsuarioCommand` en `DTOs/Administracion/`.

## Verificación

- `dotnet build` 0 warnings; test de integración que materializa los usuarios reales con
  `Include(Permisos)` en verde.
