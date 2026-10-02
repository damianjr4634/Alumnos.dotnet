using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de XXX_COPIA_ALUMNO: copia la fila de ALUMNOS a otra carrera (sin libro
/// matriz, certificado en trámite ni usuario web; el usuario que copia queda como
/// último modificador). El SP valida que el alumno no exista en la carrera destino
/// (altas o bajas → FERRCOD=2) y, si inserta, devuelve FERRCOD=1 pidiendo
/// confirmación. Como ya hizo el INSERT, se ejecuta con el patrón de dos fases sin
/// transacción de larga vida: <c>confirmar=false</c> ejecuta y hace rollback
/// (devuelve el mensaje de confirmación); <c>true</c> commitea.
/// </summary>
public interface ICopiaAlumnoProcedure
{
    Task<Result<string>> EjecutarAsync(CambioCarreraAlumnoParametros parametros, bool confirmar, CancellationToken ct);
}

/// <summary>
/// Wrapper de XXX_MUEVE_ALUMNO: cambia la carrera de la fila de ALUMNOS. Además de
/// las validaciones de existencia en destino, el SP rechaza alumnos con CURSADA o
/// ANALITIC en la carrera origen (FERRCOD=2). Mismo patrón de dos fases que la copia.
/// </summary>
public interface IMueveAlumnoProcedure
{
    Task<Result<string>> EjecutarAsync(CambioCarreraAlumnoParametros parametros, bool confirmar, CancellationToken ct);
}

/// <summary>
/// Wrapper de XXX_BORRA_ALUMNO: elimina físicamente al alumno con todo su historial
/// (ANALITIC, CURSADA, PERMEXA, FALTAS y ALUMNOS). El SP verifica en USUARIOS que el
/// usuario sea supervisor (si no, FERRCOD=2) y devuelve FERRCOD=1 pidiendo
/// confirmación. Mismo patrón de dos fases.
/// </summary>
public interface IBorraAlumnoProcedure
{
    Task<Result<string>> EjecutarAsync(string codigoCarrera, string codigoAlumno, int codigoUsuario, bool confirmar, CancellationToken ct);
}

/// <summary>Parámetros comunes de XXX_COPIA_ALUMNO y XXX_MUEVE_ALUMNO (CARDDE, CARHTA, COD_ALU, CODUSU).</summary>
public sealed record CambioCarreraAlumnoParametros
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoCarreraOrigen { get; init; }

    public required string CodigoCarreraDestino { get; init; }

    public required int CodigoUsuario { get; init; }
}
