using System.Globalization;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Esba.Domain.Common;
using Esba.Domain.Enums;
using FluentValidation;

namespace Esba.Application.Features.Certificados;

/// <summary>
/// Emite la constancia de alumno (CTT / Pase / Analítico), sucesor de
/// Impresion_Analitico_Pase de constanciaalumnos2.pas. Es de solo lectura (no
/// abre transacción): valida con el SP de chequeo según el tipo, junta el párrafo
/// y las materias adeudadas, y delega el armado del PDF al servicio de reporte.
/// </summary>
public sealed class GenerarConstanciaAlumnoHandler
{
    private readonly IValidator<GenerarConstanciaCommand> _validator;
    private readonly ICertificadoEnTramiteProcedure _certificadoEnTramite;
    private readonly IPaseAlumnoProcedure _pase;
    private readonly IParrafoConstanciaProcedure _parrafo;
    private readonly IConstanciaMateriasProcedure _materias;
    private readonly IConstanciasQuery _carreras;
    private readonly IConstanciaReportService _reporte;
    private readonly TimeProvider _tiempo;

    public GenerarConstanciaAlumnoHandler(
        IValidator<GenerarConstanciaCommand> validator,
        ICertificadoEnTramiteProcedure certificadoEnTramite,
        IPaseAlumnoProcedure pase,
        IParrafoConstanciaProcedure parrafo,
        IConstanciaMateriasProcedure materias,
        IConstanciasQuery carreras,
        IConstanciaReportService reporte,
        TimeProvider tiempo)
    {
        _validator = validator;
        _certificadoEnTramite = certificadoEnTramite;
        _pase = pase;
        _parrafo = parrafo;
        _materias = materias;
        _carreras = carreras;
        _reporte = reporte;
        _tiempo = tiempo;
    }

    /// <summary>
    /// Valida la solicitud y corre el chequeo de negocio (SP) sin generar el PDF.
    /// La página lo usa para resolver el diálogo de confirmación antes de abrir el
    /// reporte. El valor es el cuatrimestre tope a considerar (FCUATRI).
    /// </summary>
    public Task<Result<int>> ValidarAsync(GenerarConstanciaCommand command, bool confirmado, CancellationToken ct) =>
        ChequearAsync(command, confirmado, ct);

    /// <summary>
    /// Re-valida (la autoridad es el servidor, §2.7) y genera el PDF de la constancia.
    /// </summary>
    public async Task<Result<byte[]>> GenerarPdfAsync(GenerarConstanciaCommand command, bool confirmado, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var chequeo = await ChequearAsync(command, confirmado, ct).ConfigureAwait(false);
        if (!chequeo.IsSuccess)
        {
            return new Result<byte[]> { Status = chequeo.Status, Message = chequeo.Message };
        }

        var cuatrimestreTope = chequeo.Value;

        var carrera = await _carreras.ObtenerDatosCarreraAsync(command.CodigoCarrera, ct).ConfigureAwait(false);
        if (carrera is null)
        {
            return Result.Error<byte[]>("La carrera no existe.");
        }

        var parrafo = await _parrafo
            .ObtenerAsync(command.CodigoAlumno, command.CodigoCarrera, command.Tipo.ToCodigoSp(), ct)
            .ConfigureAwait(false);

        var materiasDto = await _materias
            .ListarAsync(command.CodigoAlumno, command.CodigoCarrera, ct)
            .ConfigureAwait(false);

        var adeuda = MateriasAdeudadasCalculator.Calcular(
            materiasDto.Select(m => new MateriaConstancia
            {
                Cuatrimestre = m.Cuatrimestre,
                Sigla = m.Sigla ?? string.Empty,
                Condicion = m.Condicion,
                Nota = m.Nota,
            }).ToList(),
            cuatrimestreTope,
            carrera.EsCarreraPorAnio);

        var model = ConstruirModelo(command, carrera, parrafo, adeuda);
        var pdf = _reporte.GenerarConstanciaAlumno(model);

        return chequeo.Status == OperationStatus.Warning
            ? Result.Warning(pdf, chequeo.Message ?? string.Empty)
            : Result.Ok(pdf);
    }

    private async Task<Result<int>> ChequearAsync(GenerarConstanciaCommand command, bool confirmado, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        // PASE valida con XXX_IMPRIME_PASE; CTT y ANALITICO con XXX_IMPRIME_CTT.
        if (command.Tipo == TipoConstancia.Pase)
        {
            var pase = await _pase.VerificarAsync(command.CodigoAlumno, command.CodigoCarrera, ct).ConfigureAwait(false);
            return pase.IsSuccess ? Result.Ok(0) : new Result<int> { Status = pase.Status, Message = pase.Message };
        }

        var certificado = await _certificadoEnTramite
            .VerificarAsync(command.CodigoAlumno, command.CodigoCarrera, ct)
            .ConfigureAwait(false);

        // NeedsConfirmation = la carrera tiene título intermedio: si el operador no
        // confirmó, se le pregunta; si confirmó, se emite con el tope FCUATRI.
        if (certificado.Status == OperationStatus.NeedsConfirmation && confirmado)
        {
            return Result.Ok(certificado.Value);
        }

        return certificado;
    }

    private ConstanciaAlumnoModel ConstruirModelo(
        GenerarConstanciaCommand command,
        CarreraConstanciaDto carrera,
        string parrafo,
        string adeuda)
    {
        var hoy = DateOnly.FromDateTime(_tiempo.GetLocalNow().DateTime);
        var idioma = string.IsNullOrWhiteSpace(carrera.Idioma) ? null : carrera.Idioma!.Trim();

        return new ConstanciaAlumnoModel
        {
            Titulo = command.Tipo.ToTitulo(),
            Parrafo = parrafo,
            MateriasQueAdeuda = "* Materias que adeuda: " + adeuda,
            IdiomaLinea = idioma is null ? null : "* Idioma extranjero cursado: " + idioma,
            ParrafoCierre =
                "A pedido del interesado se extiende la presente constancia, sin raspadura ni " +
                $"enmiendas, en Buenos Aires, a los {hoy.Day} días del mes de {TextoCastellano.MesEnLetras(hoy.Month)} " +
                $"de {hoy.Year.ToString(CultureInfo.InvariantCulture)}, para ser presentado ante: {command.AnteQuien?.Trim()}",
            NotasLegales =
            [
                "La presente Resolución Ministerial permite el ingreso a Universidades a Nivel Nacional.-",
                "La presente constancia tiene validez por 90 días.-",
                "La presente certificación carecerá de valor si no estuviera firmada por las autoridades competentes.",
            ],
            Secretaria = carrera.Secretaria,
            Rector = carrera.Rector,
            IncluirMembrete = command.IncluirMembrete,
            Instituto = carrera.Instituto,
            Caracteristica = carrera.Caracteristica,
            NombreCarrera = carrera.Nombre,
        };
    }
}
