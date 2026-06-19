using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Application.Features.Certificados;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Certificados;

public class GenerarConstanciaExamenFinalHandlerTests
{
    private readonly IConstanciaMateriasProcedure _materias = Substitute.For<IConstanciaMateriasProcedure>();
    private readonly IConstanciasQuery _carreras = Substitute.For<IConstanciasQuery>();
    private readonly IParrafoConstanciaProcedure _parrafo = Substitute.For<IParrafoConstanciaProcedure>();
    private readonly IConstanciaReportService _reporte = Substitute.For<IConstanciaReportService>();

    private GenerarConstanciaExamenFinalHandler Handler() => new(_materias, _carreras, _parrafo, _reporte);

    private void SembrarMateria(string condicion)
    {
        _materias.ListarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { new ConstanciaMateriaDto { Cuatrimestre = 1, CodigoMateria = "07", Condicion = condicion } });
        _carreras.ObtenerDatosCarreraAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new CarreraConstanciaDto { Nombre = "Tecnicatura", Secretaria = "S", Rector = "R" });
        _parrafo.ObtenerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Cuerpo de la constancia");
        _reporte.GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>()).Returns([1, 2, 3]);
    }

    [Fact]
    public async Task GenerarPdfAsync_MateriaRendida_GeneraElPdfConTipoCE()
    {
        SembrarMateria("APROBADA");

        var resultado = await Handler().GenerarPdfAsync("123", "TER", "07", "Universidad X", true, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(resultado.Value);
        // El TIPO del párrafo de examen final es 'CE-<codmat>'.
        await _parrafo.Received(1).ObtenerAsync("123", "TER", "CE-07", Arg.Any<CancellationToken>());
        _reporte.Received(1).GenerarConstanciaAlumno(Arg.Is<ConstanciaAlumnoModel>(m =>
            m.Titulo == "CONSTANCIA DE EXAMEN FINAL" && m.MateriasQueAdeuda == null));
    }

    [Fact]
    public async Task GenerarPdfAsync_CondicionNoElegible_DevuelveErrorYNoGenera()
    {
        SembrarMateria("CURSANDO");

        var resultado = await Handler().GenerarPdfAsync("123", "TER", "07", "Universidad X", true, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _reporte.DidNotReceive().GenerarConstanciaAlumno(Arg.Any<ConstanciaAlumnoModel>());
    }

    [Fact]
    public async Task GenerarPdfAsync_MateriaInexistente_DevuelveError()
    {
        SembrarMateria("APROBADA");

        var resultado = await Handler().GenerarPdfAsync("123", "TER", "99", "Universidad X", true, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _parrafo.DidNotReceive().ObtenerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerarPdfAsync_SinAnteQuien_DevuelveError()
    {
        SembrarMateria("APROBADA");

        var resultado = await Handler().GenerarPdfAsync("123", "TER", "07", "  ", true, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _materias.DidNotReceive().ListarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
