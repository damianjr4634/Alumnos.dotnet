using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class ConfirmarRegularizacionCnaHandlerTests
{
    private readonly IRegularizacionRepository _repositorio = Substitute.For<IRegularizacionRepository>();

    private ConfirmarRegularizacionCnaHandler CrearHandler() =>
        new(new ConfirmarRegularizacionCnaValidator(), _repositorio);

    private static NotaCursadoCnaInput Fila(decimal? nota = null, DateTime? fecha = null, string condicion = "CURSANDO") => new()
    {
        CodigoAlumno = "100",
        CodigoMateria = "17",
        CuatrimestreAnio = "1/24",
        CondicionActual = condicion,
        NotaFinal = nota,
        Fecha = fecha ?? new DateTime(2024, 7, 1),
    };

    private static ConfirmarRegularizacionCnaCommand Comando(params NotaCursadoCnaInput[] filas) => new()
    {
        CodigoCarrera = "CNA",
        CodigoUsuario = 1,
        Filas = filas,
    };

    [Fact]
    public async Task Handle_SinFilas_DevuelveError_YNoTocaRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(Comando(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _repositorio.DidNotReceiveWithAnyArgs().ConfirmarCnaAsync(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_SinFecha_DevuelveError_YNoTocaRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(
            Comando(Fila(nota: 8m) with { Fecha = null }), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Contains("fecha", resultado.Message);
        await _repositorio.DidNotReceiveWithAnyArgs().ConfirmarCnaAsync(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_NotaAlta_QuedaRegular_ConNotaFinal()
    {
        IReadOnlyList<FilaRegularizacionCnaResuelta>? capturadas = null;
        _repositorio.ConfirmarCnaAsync("CNA", 1, Arg.Do<IReadOnlyList<FilaRegularizacionCnaResuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await CrearHandler().HandleAsync(Comando(Fila(nota: 8m)), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("REGULAR", capturadas![0].NuevaCondicion);
        Assert.Equal(8m, capturadas[0].NotaFinal);
    }

    [Fact]
    public async Task Handle_NotaMedia_QuedaRecursa()
    {
        IReadOnlyList<FilaRegularizacionCnaResuelta>? capturadas = null;
        _repositorio.ConfirmarCnaAsync("CNA", 1, Arg.Do<IReadOnlyList<FilaRegularizacionCnaResuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await CrearHandler().HandleAsync(Comando(Fila(nota: 5m)), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("RECURSA", capturadas![0].NuevaCondicion);
    }
}
