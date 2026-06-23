namespace Esba.Domain.Entities;

/// <summary>
/// Tabla DOCENTES: profesores. PK: CODPROFES (código manual de 3 caracteres).
/// Un docente está activo mientras <see cref="FechaBaja"/> es null (el combo
/// legacy de cargacomisiones filtra FECHA_BAJ IS NULL).
/// El ABM (hito 10.2) mapea el subconjunto "esencial" de columnas (identificación,
/// documento, contacto, domicilio, fechas, licencia); el resto de la tabla
/// (antigüedad docente, títulos DOC_TITULOS, sexo/género/nacionalidad, obra social)
/// queda como deuda y permanece sin mapear.
/// </summary>
public class Docente
{
    /// <summary>CODPROFES CHAR(3): código del docente (PK), ingresado por el usuario.</summary>
    public required string Codigo { get; set; }

    /// <summary>DOCENTE VARCHAR(80): apellido y nombre.</summary>
    public string? Nombre { get; set; }

    /// <summary>TIPODOC CHAR(3): tipo de documento (DNI, LE, etc.).</summary>
    public string? TipoDocumento { get; set; }

    /// <summary>NRODOCUM CHAR(8): número de documento.</summary>
    public string? NumeroDocumento { get; set; }

    /// <summary>FEC_NAC DATE: fecha de nacimiento.</summary>
    public DateOnly? FechaNacimiento { get; set; }

    /// <summary>DI_ECCION CHAR(30): domicilio.</summary>
    public string? Direccion { get; set; }

    /// <summary>PISO CHAR(2).</summary>
    public string? Piso { get; set; }

    /// <summary>DEPTO CHAR(2).</summary>
    public string? Departamento { get; set; }

    /// <summary>COD_POST CHAR(4): código postal.</summary>
    public string? CodigoPostal { get; set; }

    /// <summary>LOCALIDAD CHAR(30).</summary>
    public string? Localidad { get; set; }

    /// <summary>TELEFONO_P VARCHAR(20): teléfono particular.</summary>
    public string? TelefonoParticular { get; set; }

    /// <summary>TELEFONO_M VARCHAR(20): teléfono de mensajes.</summary>
    public string? TelefonoMensajes { get; set; }

    /// <summary>INTERNO CHAR(4): interno telefónico.</summary>
    public string? Interno { get; set; }

    /// <summary>FECHA_ING DATE: fecha de ingreso.</summary>
    public DateOnly? FechaIngreso { get; set; }

    /// <summary>FECHA_BAJ DATE: fecha de baja; null = docente activo.</summary>
    public DateOnly? FechaBaja { get; set; }

    /// <summary>LICENCIA CHAR(1) 'S'/'N': el docente está en licencia.</summary>
    public bool EnLicencia { get; set; }

    /// <summary>LICENFECH DATE: fecha de la licencia.</summary>
    public DateOnly? FechaLicencia { get; set; }

    /// <summary>true si el docente está dado de baja (FECHA_BAJ no nula).</summary>
    public bool EstaDeBaja => FechaBaja is not null;
}
