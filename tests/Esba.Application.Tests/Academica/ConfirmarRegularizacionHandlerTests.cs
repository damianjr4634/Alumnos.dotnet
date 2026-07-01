using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class ConfirmarRegularizacionHandlerTests
{
    private readonly IConfiguracionQuery _configuracion = Substitute.For<IConfiguracionQuery>();
    private readonly IRegularizacionRepository _repositorio = Substitute.For<IRegularizacionRepository>();

    private ConfirmarRegularizacionHandler CrearHandler()
    {
        _configuracion.ObtenerValorAsync("Regula_NotPromocion", Arg.Any<CancellationToken>()).Returns("7");
        return new ConfirmarRegularizacionHandler(new ConfirmarRegularizacionValidator(), _configuracion, _repositorio);
    }

    private static NotaCursadoInput Fila(
        decimal? tp = null, decimal? tp2 = null, decimal? recup = null,
        bool promociona = false, bool apruebaSinFinal = false, string condicion = "CURSANDO") => new()
    {
        CodigoAlumno = "100",
        CodigoMateria = "01",
        CuatrimestreAnio = "1/24",
        CondicionActual = condicion,
        TpEva = tp,
        TpEva2 = tp2,
        Recuperatorio = recup,
        TotalHoras = 100,
        Inasistencias = 0,
        Justificadas = 0,
        MateriaPromociona = promociona,
        MateriaApruebaSinFinal = apruebaSinFinal,
    };

    private static ConfirmarRegularizacionCommand Comando(params NotaCursadoInput[] filas) => new()
    {
        CodigoCarrera = "TEC",
        CodigoUsuario = 1,
        Filas = filas,
    };

    [Fact]
    public async Task Handle_SinFilas_DevuelveError_YNoTocaRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(Comando(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _repositorio.DidNotReceiveWithAnyArgs().ConfirmarTerciariaAsync(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_NotaFueraDeRango_DevuelveError()
    {
        var resultado = await CrearHandler().HandleAsync(Comando(Fila(tp: 15m)), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _repositorio.DidNotReceiveWithAnyArgs().ConfirmarTerciariaAsync(default!, default, default!, default);
    }

    [Fact]
    public async Task Handle_DosParcialesAprobados_QuedaRegular_SinNotaAnalitico()
    {
        IReadOnlyList<FilaRegularizacionResuelta>? capturadas = null;
        _repositorio.ConfirmarTerciariaAsync("TEC", 1, Arg.Do<IReadOnlyList<FilaRegularizacionResuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await CrearHandler().HandleAsync(Comando(Fila(tp: 7m, tp2: 6m)), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(1, resultado.Value);
        Assert.NotNull(capturadas);
        Assert.Equal("REGULAR", capturadas![0].NuevaCondicion);
        Assert.Null(capturadas[0].NotaAnalitico);
    }

    [Fact]
    public async Task Handle_MateriaPromocionYNotasAltas_PromocionaConNotaAnalitico()
    {
        IReadOnlyList<FilaRegularizacionResuelta>? capturadas = null;
        _repositorio.ConfirmarTerciariaAsync("TEC", 1, Arg.Do<IReadOnlyList<FilaRegularizacionResuelta>>(f => capturadas = f), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await CrearHandler().HandleAsync(
            Comando(Fila(tp: 8m, tp2: 9m, promociona: true)), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("PROMOCIONA", capturadas![0].NuevaCondicion);
        Assert.Equal(8.5m, capturadas[0].NotaAnalitico);
    }

    [Fact]
    public async Task Handle_ErrorDeVolcado_SeMapeaAResultError()
    {
        _repositorio.ConfirmarTerciariaAsync("TEC", 1, Arg.Any<IReadOnlyList<FilaRegularizacionResuelta>>(), Arg.Any<CancellationToken>())
            .Returns<int>(_ => throw new InvalidOperationException("Falta la fecha de promoción del cuatrimestre 124."));

        var resultado = await CrearHandler().HandleAsync(
            Comando(Fila(tp: 8m, tp2: 9m, promociona: true)), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Contains("fecha de promoción", resultado.Message);
    }
}
