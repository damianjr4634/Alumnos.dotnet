using Esba.Application.Abstractions;
using Esba.Domain.Enums;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Resolución del vínculo de un usuario según su tipo, compartida por alta y
/// modificación: normaliza los códigos, descarta los que no corresponden al tipo
/// y verifica contra DOCENTES / ALUMNOS que el destino exista y esté activo, y que
/// el docente no tenga ya otro usuario web. Devuelve el vínculo listo para
/// persistir o un mensaje de error de negocio.
/// </summary>
internal sealed record VinculoUsuario(string? CodigoDocente, string? AlumnoCarrera, string? AlumnoCodigo, string? Error)
{
    public static async Task<VinculoUsuario> ResolverAsync(
        TipoUsuario tipo,
        string? codigoDocente,
        string? alumnoCarrera,
        string? alumnoCodigo,
        int? codigoUsuarioExcluido,
        IUsuarioRepository usuarios,
        IDocenteRepository docentes,
        IAlumnoRepository alumnos,
        CancellationToken ct)
    {
        switch (tipo)
        {
            case TipoUsuario.Docente:
            {
                var codigo = codigoDocente!.Trim().ToUpperInvariant();
                var docente = await docentes.ObtenerPorCodigoAsync(codigo, ct).ConfigureAwait(false);
                if (docente is null)
                {
                    return Fallo($"No existe el docente con código '{codigo}'.");
                }

                if (docente.EstaDeBaja)
                {
                    return Fallo($"El docente '{codigo}' está dado de baja: no se le puede asignar un usuario.");
                }

                if (await usuarios.ExisteVinculoDocenteAsync(codigo, codigoUsuarioExcluido, ct).ConfigureAwait(false))
                {
                    return Fallo($"El docente '{codigo}' ya tiene un usuario activo.");
                }

                return new VinculoUsuario(codigo, null, null, null);
            }

            case TipoUsuario.Alumno:
            {
                var carrera = alumnoCarrera!.Trim().ToUpperInvariant();
                var codigo = alumnoCodigo!.Trim();
                var alumno = await alumnos.ObtenerAsync(carrera, codigo, ct).ConfigureAwait(false);
                if (alumno is null)
                {
                    return Fallo($"No existe el alumno {carrera}-{codigo}.");
                }

                // TODO-migrar (portal de alumnos): unicidad alumno ↔ usuario y
                // política ante alumnos dados de baja, a definir con el portal.
                return new VinculoUsuario(null, carrera, codigo, null);
            }

            default:
                // Secretaría: sin vínculo. Lo que haya venido se descarta.
                return new VinculoUsuario(null, null, null, null);
        }
    }

    private static VinculoUsuario Fallo(string mensaje) => new(null, null, null, mensaje);
}
