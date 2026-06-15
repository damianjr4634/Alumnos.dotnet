using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Application.Features.Asistencias;
using Esba.Application.Validators;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Asistencias;

public class GuardarInasistenciasComisionHandlerTests
{
    private readonly IInasistenciasRepository _repo = Substitute.For<IInasistenciasRepository>();

    private GuardarInasistenciasComisionHandler CrearHandler() =>
        new(_repo, new GuardarInasistenciasComisionValidator());

    private static GuardarInasistenciasComisionCommand ComandoValido() => new()
    {
        CodigoCarrera = "ADM",
        Cutuco = 111,
        CuatrimestreAnio = "124",
        CodigoMateria = "01",
        CodigoUsuario = 7,
        Faltas = [new FaltaInasistencia { CodigoAlumno = "DNI1", Fecha = new DateOnly(2024, 4, 10), CodigoFalta = "01", Cantidad = 1 }],
    };

    [Fact]
    public async Task Guardar_DerivaElAnioDelCuatrimestreYReemplaza()
    {
        _repo.ReemplazarFaltasComisionAsync(
            Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>(), Arg.Any<int>(),
            Arg.Any<short?>(), Arg.Any<IReadOnlyList<FaltaInasistencia>>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _repo.Received(1).ReemplazarFaltasComisionAsync(
            "ADM", (short)111, "01", 2024, (short)7,
            Arg.Any<IReadOnlyList<FaltaInasistencia>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_MateriaVacia_PasaNull()
    {
        await CrearHandler().HandleAsync(ComandoValido() with { CodigoMateria = "" }, CancellationToken.None);

        await _repo.Received(1).ReemplazarFaltasComisionAsync(
            "ADM", (short)111, Arg.Is<string?>(m => m == null), 2024, Arg.Any<short?>(),
            Arg.Any<IReadOnlyList<FaltaInasistencia>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_ComandoInvalido_NoTocaElRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(
            ComandoValido() with { CuatrimestreAnio = "1" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _repo.DidNotReceive().ReemplazarFaltasComisionAsync(
            Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>(), Arg.Any<int>(),
            Arg.Any<short?>(), Arg.Any<IReadOnlyList<FaltaInasistencia>>(), Arg.Any<CancellationToken>());
    }
}
