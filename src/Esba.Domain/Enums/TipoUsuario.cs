namespace Esba.Domain.Enums;

/// <summary>
/// USUARIOS.TIPO CHAR(3): perfil de acceso web (columna agregada por .NET,
/// migración 2026-10-02_usuarios_tipo_vinculo.sql). Un solo login y una sola
/// tabla de credenciales para los tres perfiles; el tipo decide el área de la
/// aplicación y las políticas de autorización (12.3 ampliado). La traducción
/// enum ↔ código de la base vive en <see cref="TipoUsuarioCodigo"/>.
/// </summary>
public enum TipoUsuario
{
    /// <summary>'SEC': personal de secretaría (comportamiento histórico del sistema). Default de las filas existentes.</summary>
    Secretaria = 0,

    /// <summary>'DOC': docente, vinculado a DOCENTES.CODPROFES.</summary>
    Docente = 1,

    /// <summary>'ALU': alumno, vinculado a ALUMNOS (CARRE, COD_ALU). Portal futuro.</summary>
    Alumno = 2,
}

/// <summary>Códigos de 3 letras que persiste USUARIOS.TIPO. Único lugar que conoce la correspondencia.</summary>
public static class TipoUsuarioCodigo
{
    public const string Secretaria = "SEC";
    public const string Docente = "DOC";
    public const string Alumno = "ALU";

    public static string ACodigo(TipoUsuario tipo) => tipo switch
    {
        TipoUsuario.Secretaria => Secretaria,
        TipoUsuario.Docente => Docente,
        TipoUsuario.Alumno => Alumno,
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de usuario desconocido."),
    };

    /// <summary>
    /// NULL o blanco se leen como secretaría (es el DEFAULT de la columna y el
    /// significado de las filas previas a la migración). Un código no reconocido
    /// es un dato corrupto: se falla fuerte en vez de otorgar un perfil al azar.
    /// </summary>
    public static TipoUsuario DesdeCodigo(string? codigo)
    {
        var limpio = codigo?.Trim().ToUpperInvariant();
        return limpio switch
        {
            null or "" or Secretaria => TipoUsuario.Secretaria,
            Docente => TipoUsuario.Docente,
            Alumno => TipoUsuario.Alumno,
            _ => throw new InvalidOperationException($"USUARIOS.TIPO con valor desconocido '{codigo}'."),
        };
    }

    public static string Etiqueta(TipoUsuario tipo) => tipo switch
    {
        TipoUsuario.Secretaria => "Secretaría",
        TipoUsuario.Docente => "Docente",
        TipoUsuario.Alumno => "Alumno",
        _ => tipo.ToString(),
    };
}
