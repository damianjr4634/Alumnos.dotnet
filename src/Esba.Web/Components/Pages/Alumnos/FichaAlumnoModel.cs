using Esba.Application.DTOs.Alumnos;
using Esba.Domain.Enums;

namespace Esba.Web.Components.Pages.Alumnos;

/// <summary>
/// Modelo de presentación de la ficha: espejo mutable de los commands para el
/// binding bidireccional de MudForm (los commands son records init-only).
/// Sin lógica de negocio: solo estado de formulario y mapeos (§2.1).
/// Los nombres de propiedad coinciden 1:1 con los commands para reutilizar el
/// validador FluentValidation campo por campo.
/// </summary>
public sealed class FichaAlumnoModel
{
    public string TipoDocumento { get; set; } = "DNI";

    public string NumeroDocumento { get; set; } = string.Empty;

    public string? Apellido { get; set; }

    public string? Nombre { get; set; }

    public string? DocumentoExpedidoPor { get; set; }

    public Sexo? Sexo { get; set; }

    public string? Nacionalidad { get; set; }

    public EstadoCivil? EstadoCivil { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string? LugarNacimiento { get; set; }

    public string? ProvinciaNacimiento { get; set; }

    public string? Domicilio { get; set; }

    public string? Localidad { get; set; }

    public short? CodigoPostal { get; set; }

    public string? CaracteristicaTelefono { get; set; }

    public string? Telefono { get; set; }

    public string? Celular { get; set; }

    public string? Mail { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public string? ColegioPrimario { get; set; }

    public string? AnioPrimario { get; set; }

    public string? TituloPrimario { get; set; }

    public string? ColegioSecundario { get; set; }

    public string? AnioSecundario { get; set; }

    public string? TituloSecundario { get; set; }

    public string? InstitucionTerciaria { get; set; }

    public string? AnioTerciario { get; set; }

    public string? TituloTerciario { get; set; }

    public string? Empresa { get; set; }

    public string? Rubro { get; set; }

    public string? Cargo { get; set; }

    public string? Antiguedad { get; set; }

    public string? DomicilioLaboral { get; set; }

    public string? TelefonoLaboral { get; set; }

    public string? InternoLaboral { get; set; }

    public bool TieneCertificadoEnTramite { get; set; }

    public DateOnly? FechaCertificadoEnTramite { get; set; }

    public SituacionDni? SituacionDni { get; set; }

    public bool PresentoCa { get; set; }

    public bool Baja { get; set; }

    public string? Observaciones { get; set; }

    public bool? PresentoFoto { get; set; }

    public bool? PresentoAptoFisico { get; set; }

    public DateOnly? FechaAptoFisico { get; set; }

    public string? UsuarioWeb { get; set; }

    public bool? PresentoPartidaNacimiento { get; set; }

    public bool? PresentoLibreta { get; set; }

    public DateOnly? FechaLibreta { get; set; }

    public EstadoAlumno? Estado { get; set; }

    public Genero? Genero { get; set; }

    public bool? NominaPase { get; set; }

    public static FichaAlumnoModel Desde(AlumnoDetailDto d)
    {
        ArgumentNullException.ThrowIfNull(d);
        return new FichaAlumnoModel
        {
            TipoDocumento = d.Codigo.Length >= 3 ? d.Codigo[..3].Trim() : d.Codigo,
            NumeroDocumento = d.Codigo.Length > 3 ? d.Codigo[3..].Trim() : string.Empty,
            Apellido = d.Apellido,
            Nombre = d.Nombre,
            DocumentoExpedidoPor = d.DocumentoExpedidoPor,
            Sexo = d.Sexo,
            Nacionalidad = d.Nacionalidad,
            EstadoCivil = d.EstadoCivil,
            FechaNacimiento = d.FechaNacimiento,
            LugarNacimiento = d.LugarNacimiento,
            ProvinciaNacimiento = d.ProvinciaNacimiento,
            Domicilio = d.Domicilio,
            Localidad = d.Localidad,
            CodigoPostal = d.CodigoPostal,
            CaracteristicaTelefono = d.CaracteristicaTelefono,
            Telefono = d.Telefono,
            Celular = d.Celular,
            Mail = d.Mail,
            FechaIngreso = d.FechaIngreso,
            ColegioPrimario = d.ColegioPrimario,
            AnioPrimario = d.AnioPrimario,
            TituloPrimario = d.TituloPrimario,
            ColegioSecundario = d.ColegioSecundario,
            AnioSecundario = d.AnioSecundario,
            TituloSecundario = d.TituloSecundario,
            InstitucionTerciaria = d.InstitucionTerciaria,
            AnioTerciario = d.AnioTerciario,
            TituloTerciario = d.TituloTerciario,
            Empresa = d.Empresa,
            Rubro = d.Rubro,
            Cargo = d.Cargo,
            Antiguedad = d.Antiguedad,
            DomicilioLaboral = d.DomicilioLaboral,
            TelefonoLaboral = d.TelefonoLaboral,
            InternoLaboral = d.InternoLaboral,
            TieneCertificadoEnTramite = d.TieneCertificadoEnTramite,
            FechaCertificadoEnTramite = d.FechaCertificadoEnTramite,
            SituacionDni = d.SituacionDni,
            PresentoCa = d.PresentoCa,
            Baja = d.Baja,
            Observaciones = d.Observaciones,
            PresentoFoto = d.PresentoFoto,
            PresentoAptoFisico = d.PresentoAptoFisico,
            FechaAptoFisico = d.FechaAptoFisico,
            UsuarioWeb = d.UsuarioWeb,
            PresentoPartidaNacimiento = d.PresentoPartidaNacimiento,
            PresentoLibreta = d.PresentoLibreta,
            FechaLibreta = d.FechaLibreta,
            Estado = d.Estado,
            Genero = d.Genero,
            NominaPase = d.NominaPase,
        };
    }

    public CrearAlumnoCommand ACrearCommand(string codigoCarrera) => new()
    {
        TipoDocumento = TipoDocumento,
        NumeroDocumento = NumeroDocumento,
        CodigoCarrera = codigoCarrera,
        Apellido = Apellido,
        Nombre = Nombre,
        DocumentoExpedidoPor = DocumentoExpedidoPor,
        Sexo = Sexo,
        Nacionalidad = Nacionalidad,
        EstadoCivil = EstadoCivil,
        FechaNacimiento = FechaNacimiento,
        LugarNacimiento = LugarNacimiento,
        ProvinciaNacimiento = ProvinciaNacimiento,
        Domicilio = Domicilio,
        Localidad = Localidad,
        CodigoPostal = CodigoPostal,
        CaracteristicaTelefono = CaracteristicaTelefono,
        Telefono = Telefono,
        Celular = Celular,
        Mail = Mail,
        FechaIngreso = FechaIngreso,
        ColegioPrimario = ColegioPrimario,
        AnioPrimario = AnioPrimario,
        TituloPrimario = TituloPrimario,
        ColegioSecundario = ColegioSecundario,
        AnioSecundario = AnioSecundario,
        TituloSecundario = TituloSecundario,
        InstitucionTerciaria = InstitucionTerciaria,
        AnioTerciario = AnioTerciario,
        TituloTerciario = TituloTerciario,
        Empresa = Empresa,
        Rubro = Rubro,
        Cargo = Cargo,
        Antiguedad = Antiguedad,
        DomicilioLaboral = DomicilioLaboral,
        TelefonoLaboral = TelefonoLaboral,
        InternoLaboral = InternoLaboral,
        TieneCertificadoEnTramite = TieneCertificadoEnTramite,
        FechaCertificadoEnTramite = FechaCertificadoEnTramite,
        SituacionDni = SituacionDni,
        PresentoCa = PresentoCa,
        Observaciones = Observaciones,
        PresentoFoto = PresentoFoto,
        PresentoAptoFisico = PresentoAptoFisico,
        FechaAptoFisico = FechaAptoFisico,
        UsuarioWeb = UsuarioWeb,
        PresentoPartidaNacimiento = PresentoPartidaNacimiento,
        PresentoLibreta = PresentoLibreta,
        FechaLibreta = FechaLibreta,
        Estado = Estado ?? EstadoAlumno.NoSeSabe,
        Genero = Genero,
        NominaPase = NominaPase,
    };

    public ActualizarAlumnoCommand AActualizarCommand(string codigoCarrera, string codigo) => new()
    {
        Codigo = codigo,
        CodigoCarrera = codigoCarrera,
        Apellido = Apellido,
        Nombre = Nombre,
        DocumentoExpedidoPor = DocumentoExpedidoPor,
        Sexo = Sexo,
        Nacionalidad = Nacionalidad,
        EstadoCivil = EstadoCivil,
        FechaNacimiento = FechaNacimiento,
        LugarNacimiento = LugarNacimiento,
        ProvinciaNacimiento = ProvinciaNacimiento,
        Domicilio = Domicilio,
        Localidad = Localidad,
        CodigoPostal = CodigoPostal,
        CaracteristicaTelefono = CaracteristicaTelefono,
        Telefono = Telefono,
        Celular = Celular,
        Mail = Mail,
        FechaIngreso = FechaIngreso,
        ColegioPrimario = ColegioPrimario,
        AnioPrimario = AnioPrimario,
        TituloPrimario = TituloPrimario,
        ColegioSecundario = ColegioSecundario,
        AnioSecundario = AnioSecundario,
        TituloSecundario = TituloSecundario,
        InstitucionTerciaria = InstitucionTerciaria,
        AnioTerciario = AnioTerciario,
        TituloTerciario = TituloTerciario,
        Empresa = Empresa,
        Rubro = Rubro,
        Cargo = Cargo,
        Antiguedad = Antiguedad,
        DomicilioLaboral = DomicilioLaboral,
        TelefonoLaboral = TelefonoLaboral,
        InternoLaboral = InternoLaboral,
        TieneCertificadoEnTramite = TieneCertificadoEnTramite,
        FechaCertificadoEnTramite = FechaCertificadoEnTramite,
        SituacionDni = SituacionDni,
        PresentoCa = PresentoCa,
        Baja = Baja,
        Observaciones = Observaciones,
        PresentoFoto = PresentoFoto,
        PresentoAptoFisico = PresentoAptoFisico,
        FechaAptoFisico = FechaAptoFisico,
        UsuarioWeb = UsuarioWeb,
        PresentoPartidaNacimiento = PresentoPartidaNacimiento,
        PresentoLibreta = PresentoLibreta,
        FechaLibreta = FechaLibreta,
        Estado = Estado ?? EstadoAlumno.NoSeSabe,
        Genero = Genero,
        NominaPase = NominaPase,
    };

    /// <summary>Etiquetas de UI para los enums cuyo nombre C# no alcanza.</summary>
    public static string Etiqueta(EstadoAlumno estado) => estado switch
    {
        EstadoAlumno.Cursando => "Cursando",
        EstadoAlumno.Pase => "Pase",
        EstadoAlumno.Egresado => "Egresado",
        EstadoAlumno.NoSeSabe => "No se sabe",
        _ => estado.ToString(),
    };

    public static string Etiqueta(SituacionDni situacion) => situacion switch
    {
        Domain.Enums.SituacionDni.ActualizadoOchoAnios => "8 años",
        Domain.Enums.SituacionDni.ActualizadoDieciseisAnios => "16 años",
        Domain.Enums.SituacionDni.MayorDeEdad => "Mayor de edad",
        Domain.Enums.SituacionDni.SinDocumento => "Sin documento",
        _ => situacion.ToString(),
    };
}
