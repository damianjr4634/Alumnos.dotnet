using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Examenes;
using Esba.Application.Validators;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Examenes;

public class ConfirmarCargaNotasFinalHandlerTests
{
    private readonly ICargaFinalRepository _repositorio = Substitute.For<ICargaFinalRepository>();

    private ConfirmarCargaNotasFinalHandler CrearHandler() =>
        new(_repositorio, new CargaNotasFinalValidator());

    private static NotaFinalAlumnoInput Fila(string condicion, decimal? nota1) => new()
    {
        CodigoAlumno = "A1",
        CodigoMateria = "01",
        CondicionActual = condicion,
        Nota1 = nota1,
        Fecha1 = nota1 is null ? null : new DateOnly(2026, 6, 1),
        Acta1 = "L1",
    };

    private static CargaNotasFinalCommand Comando(string tipo, params NotaFinalAlumnoInput[] filas) => new()
    {
        CodigoCarrera = "TEC",
        Mesa = 10,
        TipoCarrera = tipo,
        CodigoUsuario = 1,
        Filas = filas,
    };

    [Fact]
    public async Task Confirmar_SinFilas_DevuelveError_YNoTocaRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(Comando("TER"), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _repositorio.DidNotReceive().ConfirmarAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<IReadOnlyList<FilaCargaFinalResuelta>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirmar_NotaFueraDeRango_DevuelveError()
    {
        var resultado = await CrearHandler().HandleAsync(
            Comando("TER", Fila("REGULAR", 11)), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
    }

    [Fact]
    public async Task Confirmar_Terciaria_ResuelveCondicionYAnalitico()
    {
        IReadOnlyList<FilaCargaFinalResuelta>? capturadas = null;
        _repositorio.ConfirmarAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Do<IReadOnlyList<FilaCargaFinalResuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await CrearHandler().HandleAsync(
            Comando("TER", Fila("REGULAR", 7)), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(1, resultado.Value);
        Assert.NotNull(capturadas);
        var fila = Assert.Single(capturadas!);
        Assert.True(fila.EsTerciaria);
        Assert.Equal("FINAL", fila.NuevaCondicion);
        Assert.Equal(7m, fila.NotaAnalitico);
    }

    [Fact]
    public async Task Confirmar_Bachiller_MarcaNoTerciariaYMapeaCondicion()
    {
        IReadOnlyList<FilaCargaFinalResuelta>? capturadas = null;
        _repositorio.ConfirmarAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Do<IReadOnlyList<FilaCargaFinalResuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        await CrearHandler().HandleAsync(Comando("BAC", Fila("PREVIO", 8)), CancellationToken.None);

        var fila = Assert.Single(capturadas!);
        Assert.False(fila.EsTerciaria);
        Assert.Equal("LIBRE", fila.NuevaCondicion);
        Assert.Equal(8m, fila.NotaAnalitico);
    }
}
