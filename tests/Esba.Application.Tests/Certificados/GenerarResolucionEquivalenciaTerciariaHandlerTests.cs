using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Application.Features.Certificados;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Certificados;

public class GenerarResolucionEquivalenciaTerciariaHandlerTests
{
    private readonly IEquivalenciaTerciariaQuery _equivalencias = Substitute.For<IEquivalenciaTerciariaQuery>();
    private readonly IConstanciasQuery _carreras = Substitute.For<IConstanciasQuery>();
    private readonly ICarrerasQuery _carrerasMeta = Substitute.For<ICarrerasQuery>();
    private readonly IResolucionEquivalenciaReportService _reporte = Substitute.For<IResolucionEquivalenciaReportService>();

    private GenerarResolucionEquivalenciaTerciariaHandler Handler() =>
        new(_equivalencias, _carreras, _carrerasMeta, _reporte, TimeProvider.System);

    private void Sembrar(string tipo = "TER", int materias = 2)
    {
        _carrerasMeta.ObtenerTipoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(tipo);
        _equivalencias.ObtenerEncabezadoAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new EncabezadoResolucionTerciariaDto { AnioActual = 2026, NombreAlumno = "Pérez Juan", CodigoAlumno = "DNI 1", ActasInternas = "200/19" });
        _equivalencias.ListarMateriasAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Range(1, materias).Select(i => new MateriaEquivalenciaTerciariaDto { Descripcion = $"Materia {i}", Cuatrimestre = 1 }).ToArray());
        _carreras.ObtenerDatosCarreraAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new CarreraConstanciaDto { Nombre = "Tecnicatura", Rector = "Dra. López" });
        _reporte.GenerarResolucionTerciaria(Arg.Any<ResolucionEquivalenciaTerciariaModel>()).Returns([1, 2, 3]);
    }

    [Fact]
    public async Task GenerarPdfAsync_TerciariaConMaterias_GeneraConUnParrafoPorMateria()
    {
        Sembrar(materias: 3);

        var resultado = await Handler().GenerarPdfAsync("DNI1", "37414", "2,3", incluirMembrete: true, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _reporte.Received(1).GenerarResolucionTerciaria(Arg.Is<ResolucionEquivalenciaTerciariaModel>(m =>
            m.Materias.Count == 3 && m.Rector == "Dra. López"));
    }

    [Fact]
    public async Task GenerarPdfAsync_SinCuatrimestres_DevuelveErrorYNoConsultaTipo()
    {
        Sembrar();

        var resultado = await Handler().GenerarPdfAsync("DNI1", "37414", "  ", incluirMembrete: true, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _carrerasMeta.DidNotReceive().ObtenerTipoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerarPdfAsync_CarreraNoTerciaria_DevuelveErrorYNoConsultaMaterias()
    {
        Sembrar(tipo: "BAC");

        var resultado = await Handler().GenerarPdfAsync("DNI1", "BAC", "1", incluirMembrete: true, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _equivalencias.DidNotReceive().ListarMateriasAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerarPdfAsync_SinMateriasEnLosCuatrimestres_DevuelveErrorYNoGenera()
    {
        Sembrar(materias: 0);

        var resultado = await Handler().GenerarPdfAsync("DNI1", "37414", "9", incluirMembrete: true, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _reporte.DidNotReceive().GenerarResolucionTerciaria(Arg.Any<ResolucionEquivalenciaTerciariaModel>());
    }
}
