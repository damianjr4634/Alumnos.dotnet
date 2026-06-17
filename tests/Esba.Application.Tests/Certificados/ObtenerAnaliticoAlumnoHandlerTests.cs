using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Application.Features.Certificados;
using NSubstitute;

namespace Esba.Application.Tests.Certificados;

public class ObtenerAnaliticoAlumnoHandlerTests
{
    private readonly IConstanciaMateriasProcedure _materias = Substitute.For<IConstanciaMateriasProcedure>();
    private readonly IPromedioGeneralProcedure _promedio = Substitute.For<IPromedioGeneralProcedure>();

    private ObtenerAnaliticoAlumnoHandler Handler() => new(_materias, _promedio);

    [Fact]
    public async Task ObtenerAsync_ComponeMateriasYPromedio()
    {
        var materias = new[]
        {
            new ConstanciaMateriaDto { Cuatrimestre = 1, CodigoMateria = "01", Descripcion = "Matemática", Condicion = "REGULAR" },
            new ConstanciaMateriaDto { Cuatrimestre = 2, CodigoMateria = "02", Descripcion = "Lengua", Condicion = "* ADEUDA *" },
        };
        _materias.ListarAsync("27123456789", "TER", Arg.Any<CancellationToken>()).Returns(materias);
        _promedio.ObtenerAsync("27123456789", "TER", Arg.Any<CancellationToken>()).Returns(8.50m);

        var resultado = await Handler().ObtenerAsync("27123456789", "TER", CancellationToken.None);

        Assert.Equal(2, resultado.Materias.Count);
        Assert.Equal(8.50m, resultado.PromedioGeneral);
        // El promedio y las materias salen del documento del alumno, no se mezclan.
        await _materias.Received(1).ListarAsync("27123456789", "TER", Arg.Any<CancellationToken>());
        await _promedio.Received(1).ObtenerAsync("27123456789", "TER", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObtenerAsync_AlumnoSinNotas_PromedioCeroYSinMaterias()
    {
        _materias.ListarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ConstanciaMateriaDto>());
        _promedio.ObtenerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(0m);

        var resultado = await Handler().ObtenerAsync("1", "TER", CancellationToken.None);

        Assert.Empty(resultado.Materias);
        Assert.Equal(0m, resultado.PromedioGeneral);
    }
}
