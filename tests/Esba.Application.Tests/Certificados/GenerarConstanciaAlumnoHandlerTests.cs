using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Application.Features.Certificados;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Enums;
using NSubstitute;

namespace Esba.Application.Tests.Certificados;

public class GenerarConstanciaAlumnoHandlerTests
{
    private readonly ICertificadoEnTramiteProcedure _certificado = Substitute.For<ICertificadoEnTramiteProcedure>();
    private readonly IPaseAlumnoProcedure _pase = Substitute.For<IPaseAlumnoProcedure>();
    private readonly IParrafoConstanciaProcedure _parrafo = Substitute.For<IParrafoConstanciaProcedure>();
    private readonly IConstanciaMateriasProcedure _materias = Substitute.For<IConstanciaMateriasProcedure>();
    private readonly IConstanciasQuery _carreras = Substitute.For<IConstanciasQuery>();
    private readonly IConstanciaReportService _reporte = Substitute.For<IConstanciaReportService>();

    private GenerarConstanciaAlumnoHandler Handler() => new(
        new GenerarConstanciaCommandValidator(),
        _certificado, _pase, _parrafo, _materias, _carreras, _reporte, TimeProvider.System);

    private static GenerarConstanciaCommand Comando(TipoConstancia tipo = TipoConstancia.CertificadoEnTramite) => new()
    {
        CodigoCarrera = "ADM",
        CodigoAlumno = "27123456789",
        Tipo = tipo,
        AnteQuien = "Universidad de Buenos Aires",
    };

    private void ConDatosBasicos()
    {
        _carreras.ObtenerDatosCarreraAsync("ADM", Arg.Any<CancellationToken>())
            .Returns(new CarreraConstanciaDto { Nombre = "Administración", Rector = "R", Secretaria = "S" });
        _parrafo.ObtenerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("párrafo");
        _materias.ListarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ConstanciaMateriaDto>());
        _reporte.GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>()).Returns([1, 2, 3]);
    }

    [Fact]
    public async Task Validar_SinAnteQuien_DevuelveError()
    {
        var comando = Comando() with { AnteQuien = "" };

        var resultado = await Handler().ValidarAsync(comando, confirmado: false, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
    }

    [Fact]
    public async Task GenerarPdf_CttConMateriasPendientes_DevuelveErrorSinRenderizar()
    {
        _certificado.VerificarAsync("27123456789", "ADM", Arg.Any<CancellationToken>())
            .Returns(Result.Error<int>("Le faltan materias."));

        var resultado = await Handler().GenerarPdfAsync(Comando(), confirmado: false, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _reporte.DidNotReceive().GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>());
    }

    [Fact]
    public async Task GenerarPdf_CttTituloIntermedioSinConfirmar_DevuelveNeedsConfirmation()
    {
        _certificado.VerificarAsync("27123456789", "ADM", Arg.Any<CancellationToken>())
            .Returns(new Result<int> { Status = OperationStatus.NeedsConfirmation, Message = "Título intermedio", Value = 4 });

        var resultado = await Handler().GenerarPdfAsync(Comando(), confirmado: false, CancellationToken.None);

        Assert.Equal(OperationStatus.NeedsConfirmation, resultado.Status);
        _reporte.DidNotReceive().GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>());
    }

    [Fact]
    public async Task GenerarPdf_CttTituloIntermedioConfirmado_RenderizaUnaVez()
    {
        ConDatosBasicos();
        _certificado.VerificarAsync("27123456789", "ADM", Arg.Any<CancellationToken>())
            .Returns(new Result<int> { Status = OperationStatus.NeedsConfirmation, Message = "Título intermedio", Value = 4 });

        var resultado = await Handler().GenerarPdfAsync(Comando(), confirmado: true, CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        _reporte.Received(1).GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>());
    }

    [Fact]
    public async Task GenerarPdf_CttOk_RenderizaPdf()
    {
        ConDatosBasicos();
        _certificado.VerificarAsync("27123456789", "ADM", Arg.Any<CancellationToken>())
            .Returns(Result.Ok(0));

        var resultado = await Handler().GenerarPdfAsync(Comando(), confirmado: false, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _reporte.Received(1).GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>());
    }

    [Fact]
    public async Task GenerarPdf_PaseAlumnoAproboTodo_DevuelveError()
    {
        _pase.VerificarAsync("27123456789", "ADM", Arg.Any<CancellationToken>())
            .Returns(Result.Error<bool>("No corresponde un pase."));

        var resultado = await Handler().GenerarPdfAsync(Comando(TipoConstancia.Pase), confirmado: false, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _reporte.DidNotReceive().GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>());
    }

    [Fact]
    public async Task GenerarPdf_PaseCorresponde_RenderizaPdf()
    {
        ConDatosBasicos();
        _pase.VerificarAsync("27123456789", "ADM", Arg.Any<CancellationToken>())
            .Returns(Result.Ok(true));

        var resultado = await Handler().GenerarPdfAsync(Comando(TipoConstancia.Pase), confirmado: false, CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        _reporte.Received(1).GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>());
        // El pase usa XXX_IMPRIME_PASE, nunca el de certificado.
        await _certificado.DidNotReceive().VerificarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerarPdf_CarreraInexistente_DevuelveError()
    {
        _certificado.VerificarAsync("27123456789", "ADM", Arg.Any<CancellationToken>()).Returns(Result.Ok(0));
        _carreras.ObtenerDatosCarreraAsync("ADM", Arg.Any<CancellationToken>()).Returns((CarreraConstanciaDto?)null);

        var resultado = await Handler().GenerarPdfAsync(Comando(), confirmado: false, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _reporte.DidNotReceive().GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>());
    }
}
