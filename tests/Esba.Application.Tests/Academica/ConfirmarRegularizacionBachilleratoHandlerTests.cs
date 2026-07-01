using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class ConfirmarRegularizacionBachilleratoHandlerTests
{
    private readonly IRegularizacionRepository _repositorio = Substitute.For<IRegularizacionRepository>();

    private ConfirmarRegularizacionBachilleratoHandler CrearHandler() =>
        new(new ConfirmarRegularizacionBachilleratoValidator(), _repositorio);

    private static NotaCursadoBachilleratoInput Fila(
        decimal? tp = null, decimal? tp2 = null, decimal? recup = null, decimal? regular = null,
        int? totHoras = 100, int? inasist = 0, string condicion = "CURSANDO",
        string? paso = null, bool forzarLibre = false) => new()
    {
        CodigoAlumno = "100",
        CodigoMateria = "01",
        CuatrimestreAnio = "1/24",
        CondicionActual = condicion,
        TpEva = tp,
        TpEva2 = tp2,
        Recuperatorio = recup,
        NotaRegular = regular,
        TotalHoras = (short?)totHoras,
        Inasistencias = (short?)inasist,
        Justificadas = 0,
        Fecha = new DateTime(2024, 6, 30),
        Paso = paso,
        ForzarLibre = forzarLibre,
    };

    private static ConfirmarRegularizacionBachilleratoCommand Comando(params NotaCursadoBachilleratoInput[] filas) => new()
    {
        CodigoCarrera = "BAC",
        CodigoUsuario = 1,
        Filas = filas,
    };

    [Fact]
    public async Task Handle_SinFilas_DevuelveError_YNoTocaRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(Comando(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _repositorio.DidNotReceiveWithAnyArgs().ConfirmarBachilleratoAsync(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_NotaFueraDeRango_DevuelveError()
    {
        var resultado = await CrearHandler().HandleAsync(Comando(Fila(tp: 15m)), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _repositorio.DidNotReceiveWithAnyArgs().ConfirmarBachilleratoAsync(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_DosBimestresAprobados_QuedaRegular_ConNotaFinal()
    {
        IReadOnlyList<FilaRegularizacionBachilleratoResuelta>? capturadas = null;
        _repositorio.ConfirmarBachilleratoAsync("BAC", 1, Arg.Do<IReadOnlyList<FilaRegularizacionBachilleratoResuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await CrearHandler().HandleAsync(Comando(Fila(tp: 7m, tp2: 8m)), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(1, resultado.Value);
        Assert.Equal("REGULAR", capturadas![0].NuevaCondicion);
        Assert.Equal(7.5m, capturadas[0].NotaFinal);
    }

    [Fact]
    public async Task Handle_ConsejoSinDecision_DevuelveError_YNoTocaRepositorio()
    {
        // Faltas 30% → CONSEJO; sin Paso el volcado no debe correr.
        var resultado = await CrearHandler().HandleAsync(
            Comando(Fila(tp: 8m, tp2: 8m, inasist: 30)), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Contains("CONSEJO", resultado.Message);
        await _repositorio.DidNotReceiveWithAnyArgs().ConfirmarBachilleratoAsync(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_ConsejoPasoRegular_QuedaRegular()
    {
        IReadOnlyList<FilaRegularizacionBachilleratoResuelta>? capturadas = null;
        _repositorio.ConfirmarBachilleratoAsync("BAC", 1, Arg.Do<IReadOnlyList<FilaRegularizacionBachilleratoResuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await CrearHandler().HandleAsync(
            Comando(Fila(tp: 7m, tp2: 8m, inasist: 30, paso: "Regular")), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("REGULAR", capturadas![0].NuevaCondicion);
    }

    [Fact]
    public async Task Handle_ForzarLibre_FuerzaLibreConNotas99_SinPasarPorElLadder()
    {
        IReadOnlyList<FilaRegularizacionBachilleratoResuelta>? capturadas = null;
        _repositorio.ConfirmarBachilleratoAsync("BAC", 1, Arg.Do<IReadOnlyList<FilaRegularizacionBachilleratoResuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        // Notas altas (que darían REGULAR), pero el override manual las ignora.
        var resultado = await CrearHandler().HandleAsync(
            Comando(Fila(tp: 9m, tp2: 9m, forzarLibre: true)), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("LIBRE", capturadas![0].NuevaCondicion);
        Assert.Equal(99m, capturadas[0].TpEva);
        Assert.Equal(99m, capturadas[0].TpEva2);
        Assert.Null(capturadas[0].NotaFinal);
    }

    [Fact]
    public async Task Handle_ErrorDeVolcado_SeMapeaAResultError()
    {
        _repositorio.ConfirmarBachilleratoAsync("BAC", 1, Arg.Any<IReadOnlyList<FilaRegularizacionBachilleratoResuelta>>(), Arg.Any<CancellationToken>())
            .Returns<int>(_ => throw new InvalidOperationException("Error de volcado."));

        var resultado = await CrearHandler().HandleAsync(Comando(Fila(tp: 7m, tp2: 8m)), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Contains("Error de volcado", resultado.Message);
    }
}
