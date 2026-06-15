using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Examenes;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Examenes;

public class GuardarPermisosMasivoHandlerTests
{
    private readonly IPermisosExamenRepository _permisos = Substitute.For<IPermisosExamenRepository>();

    private GuardarPermisosMasivoHandler CrearHandler() => new(_permisos);

    private static CrearPermisoExamenCommand Item(string carrera, string codAlu) => new()
    {
        CodigoCarrera = carrera,
        CodigoAlumno = codAlu,
        Mesa = 10,
        Cutuco = 111,
        CodigoMateria = "01",
        CodigoUsuario = 1,
    };

    [Fact]
    public async Task Guardar_ListaVacia_DevuelveError()
    {
        var resultado = await CrearHandler().HandleAsync([], CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _permisos.DidNotReceive().InsertarVariosAsync(Arg.Any<IReadOnlyList<CrearPermisoExamenCommand>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_MezclaCarreras_DevuelveError()
    {
        var resultado = await CrearHandler().HandleAsync(
            [Item("ADM", "A1"), Item("CON", "A2")], CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _permisos.DidNotReceive().InsertarVariosAsync(Arg.Any<IReadOnlyList<CrearPermisoExamenCommand>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_MismaCarrera_InsertaEnBloque()
    {
        _permisos.InsertarVariosAsync(Arg.Any<IReadOnlyList<CrearPermisoExamenCommand>>(), Arg.Any<CancellationToken>())
            .Returns(2);

        var resultado = await CrearHandler().HandleAsync(
            [Item("ADM", "A1"), Item("ADM", "A2")], CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(2, resultado.Value);
        await _permisos.Received(1).InsertarVariosAsync(Arg.Any<IReadOnlyList<CrearPermisoExamenCommand>>(), Arg.Any<CancellationToken>());
    }
}
