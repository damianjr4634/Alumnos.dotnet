using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Application.Features.Certificados;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Certificados;

public class GenerarEquivalenciaBachillerHandlerTests
{
    private readonly IConstanciasQuery _carreras = Substitute.For<IConstanciasQuery>();
    private readonly IEquivalenciaBachillerProcedure _lineas = Substitute.For<IEquivalenciaBachillerProcedure>();
    private readonly IEquivalenciaBachillerReportService _reporte = Substitute.For<IEquivalenciaBachillerReportService>();

    private GenerarEquivalenciaBachillerHandler Handler() =>
        new(_carreras, _lineas, _reporte, TimeProvider.System);

    private void SembrarEncabezado(EncabezadoEquivalenciaBachillerDto encabezado)
    {
        _carreras.ObtenerEncabezadoEquivalenciaBachillerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(encabezado);
        _lineas.ListarLineasAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { new LineaEquivalenciaBachillerDto { Columna1 = "SI >> Materia", Columna2 = "******" } });
        _reporte.GenerarEquivalenciaBachiller(Arg.Any<EquivalenciaBachillerModel>()).Returns([1, 2, 3]);
    }

    private static EncabezadoEquivalenciaBachillerDto Encabezado(string tipo = "BAC", string? ac = "A", string? actint = "0000103") => new()
    {
        Alumno = "Pérez Juan",
        ActividadInterna = actint,
        DocumentoAC = ac,
        Instituto = "Colegio Origen",
        PlanDescripcion = "Plan 2010",
        NombreCarrera = "Bachiller en Ciencias",
        TipoCarrera = tipo,
        InstitutoEmisor = "ESBA",
        CaracteristicaEmisor = "A-781",
    };

    [Fact]
    public async Task GenerarPdfAsync_CarreraBachiller_GeneraPdfConResolucionFormateada()
    {
        SembrarEncabezado(Encabezado(tipo: "BAC", ac: "A", actint: "0000103"));

        var resultado = await Handler().GenerarPdfAsync("DNI123", "BAC", incluirMembrete: true, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(resultado.Value);
        _reporte.Received(1).GenerarEquivalenciaBachiller(Arg.Is<EquivalenciaBachillerModel>(m =>
            m.ResolucionInterna == "00001/03" && !m.MostrarNotaAdReferendum && m.NombreCarrera == "Bachiller en Ciencias"));
    }

    [Fact]
    public async Task GenerarPdfAsync_TituloEnTramite_PideNotaAdReferendum()
    {
        SembrarEncabezado(Encabezado(ac: "C"));

        await Handler().GenerarPdfAsync("DNI123", "BAC", incluirMembrete: true, CancellationToken.None);

        _reporte.Received(1).GenerarEquivalenciaBachiller(Arg.Is<EquivalenciaBachillerModel>(m => m.MostrarNotaAdReferendum));
    }

    [Fact]
    public async Task GenerarPdfAsync_SinEquivalencias_DevuelveErrorYNoGenera()
    {
        _carreras.ObtenerEncabezadoEquivalenciaBachillerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((EncabezadoEquivalenciaBachillerDto?)null);

        var resultado = await Handler().GenerarPdfAsync("DNI123", "BAC", incluirMembrete: true, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _reporte.DidNotReceive().GenerarEquivalenciaBachiller(Arg.Any<EquivalenciaBachillerModel>());
    }

    [Fact]
    public async Task GenerarPdfAsync_CarreraNoBachiller_DevuelveErrorYNoConsultaLineas()
    {
        SembrarEncabezado(Encabezado(tipo: "TER"));

        var resultado = await Handler().GenerarPdfAsync("DNI123", "TER", incluirMembrete: true, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _lineas.DidNotReceive().ListarLineasAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _reporte.DidNotReceive().GenerarEquivalenciaBachiller(Arg.Any<EquivalenciaBachillerModel>());
    }
}
