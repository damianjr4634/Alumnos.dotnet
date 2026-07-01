using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class ConfirmarRegularizacion333HandlerTests
{
    private readonly IRegularizacionRepository _repositorio = Substitute.For<IRegularizacionRepository>();

    private ConfirmarRegularizacion333Handler CrearHandler() =>
        new(new ConfirmarRegularizacion333Validator(), _repositorio);

    private static NotaCursado333Input Fila(
        decimal? tp = null, decimal? tp2 = null, decimal? dic = null, decimal? mar = null,
        DateTime? fechDic = null, DateTime? fecEva2 = null, string condicion = "CURSANDO",
        bool forzarPrevia = false) => new()
    {
        CodigoAlumno = "100",
        CodigoMateria = "03",
        CuatrimestreAnio = "1/24",
        CondicionActual = condicion,
        TpEva = tp,
        TpEva2 = tp2,
        NotaDic = dic,
        NotaMar = mar,
        FechDic = fechDic,
        FecEva2 = fecEva2,
        TotalHoras = 100,
        Inasistencias = 0,
        Justificadas = 0,
        Fecha = new DateTime(2024, 7, 1),
        ForzarPrevia = forzarPrevia,
    };

    private static ConfirmarRegularizacion333Command Comando(params NotaCursado333Input[] filas) => new()
    {
        CodigoCarrera = "650",
        CodigoUsuario = 1,
        Filas = filas,
    };

    [Fact]
    public async Task Handle_SinFilas_DevuelveError_YNoTocaRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(Comando(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _repositorio.DidNotReceiveWithAnyArgs().Confirmar333Async(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_NotaFueraDeRango_DevuelveError()
    {
        var resultado = await CrearHandler().HandleAsync(Comando(Fila(tp2: 15m)), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _repositorio.DidNotReceiveWithAnyArgs().Confirmar333Async(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_SegundoTrimestreAprobado_QuedaRegular_ConNotaFinal()
    {
        IReadOnlyList<FilaRegularizacion333Resuelta>? capturadas = null;
        _repositorio.Confirmar333Async("650", 1, Arg.Do<IReadOnlyList<FilaRegularizacion333Resuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await CrearHandler().HandleAsync(
            Comando(Fila(tp: 6m, tp2: 8m, fecEva2: new DateTime(2024, 7, 1))), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("REGULAR", capturadas![0].NuevaCondicion);
        Assert.Equal(8m, capturadas[0].NotaFinal);
    }

    [Fact]
    public async Task Handle_DiciembreApruebaSinFecha_DevuelveError_YNoTocaRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(
            Comando(Fila(tp: 5m, tp2: 4m, dic: 8m, fechDic: null)), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Contains("fecha", resultado.Message);
        await _repositorio.DidNotReceiveWithAnyArgs().Confirmar333Async(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_ForzarPrevia_FuerzaPreviaConMarzo99_SinPasarPorElLadder()
    {
        IReadOnlyList<FilaRegularizacion333Resuelta>? capturadas = null;
        _repositorio.Confirmar333Async("650", 1, Arg.Do<IReadOnlyList<FilaRegularizacion333Resuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        // Notas altas (que darían REGULAR), pero el override manual las ignora.
        var resultado = await CrearHandler().HandleAsync(
            Comando(Fila(tp: 8m, tp2: 9m, fecEva2: new DateTime(2024, 7, 1), forzarPrevia: true)), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("PREVIA", capturadas![0].NuevaCondicion);
        Assert.Equal(99m, capturadas[0].NotaMar);
        Assert.Equal(0m, capturadas[0].NotaFinal);
    }
}
