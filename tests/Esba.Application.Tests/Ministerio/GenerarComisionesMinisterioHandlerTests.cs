using Esba.Application.Abstractions;
using Esba.Application.DTOs.Ministerio;
using Esba.Application.Features.Ministerio;
using Esba.Application.Validators;
using NSubstitute;

namespace Esba.Application.Tests.Ministerio;

public class GenerarComisionesMinisterioHandlerTests
{
    private static readonly DateTimeOffset Hoy = new(2026, 9, 29, 10, 0, 0, TimeSpan.Zero);

    /// <summary>Reloj fijo en UTC para que la fecha de emisión y las edades sean deterministas.</summary>
    private sealed class RelojFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => ahora;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private readonly IPadronMinisterioQuery _padron = Substitute.For<IPadronMinisterioQuery>();
    private readonly INominaMinisterioReportService _reporte = Substitute.For<INominaMinisterioReportService>();
    private readonly IPadronMinisterioExcelService _excel = Substitute.For<IPadronMinisterioExcelService>();

    private GenerarComisionesMinisterioHandler CrearHandler() => new(
        new GenerarNominaMinisterioValidator(),
        new ExportarPadronMinisterioValidator(),
        _padron,
        _reporte,
        _excel,
        new RelojFijo(Hoy));

    private static GenerarNominaMinisterioCommand ComandoNomina(
        string? cuatrimestre = "1/26", short? cutuco = null, decimal margen = 4m, bool edad = false) => new()
    {
        CodigoCarrera = "TEC",
        CuatrimestreAnio = cuatrimestre!,
        Cutuco = cutuco,
        ConEdadYNacionalidad = edad,
        MargenSuperiorCm = margen,
    };

    private static ExportarPadronMinisterioCommand ComandoPadron(
        string? cuatrimestre = "1/26", short? cutuco = null, bool separadas = false) => new()
    {
        CodigoCarrera = "TEC",
        CuatrimestreAnio = cuatrimestre!,
        Cutuco = cutuco,
        HojasSeparadas = separadas,
    };

    private static CarreraMinisterioDto Terciaria => new()
    {
        Codigo = "TEC", Nombre = "TECNICATURA SUPERIOR EN GESTIÓN", Tipo = "TER", Resolucion = "1234/20", EsADistancia = false,
    };

    private static CarreraMinisterioDto Bachillerato => new()
    {
        Codigo = "BAC", Nombre = "BACHILLERATO", Tipo = "BAC", Resolucion = "99/10", EsADistancia = false,
    };

    private static PadronMinisterioAlumnoDto Alumno(
        short cutuco, string codigo, string apellido, string condicion = "CURSANDO", string? sexo = "F",
        DateOnly? nacimiento = null, int? dniTutor = null) => new()
    {
        Cutuco = cutuco,
        CodigoAlumno = codigo,
        Apellido = apellido,
        Nombre = "Nombre",
        Condicion = condicion,
        Sexo = sexo,
        FechaNacimiento = nacimiento,
        Nacionalidad = "ARGENTINA",
        ColegioSecundario = "Colegio",
        TituloSecundario = "Bachiller",
        ApellidoTutor = dniTutor.HasValue ? "Tutor" : null,
        NombreTutor = dniTutor.HasValue ? "Nombre Tutor" : null,
        DniTutor = dniTutor,
    };

    // ---- Nómina PDF ----

    [Fact]
    public async Task Nomina_SinCuatrimestre_DevuelveError_YNoConsulta()
    {
        var resultado = await CrearHandler().GenerarNominaPdfAsync(ComandoNomina(cuatrimestre: ""), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El cuatrimestre es obligatorio.", resultado.Message);
        await _padron.DidNotReceiveWithAnyArgs().ObtenerCarreraAsync(default!, default);
    }

    [Fact]
    public async Task Nomina_MargenFueraDeRango_DevuelveError()
    {
        var resultado = await CrearHandler().GenerarNominaPdfAsync(ComandoNomina(margen: 25m), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Contains("margen superior", resultado.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Nomina_CarreraInexistente_DevuelveError()
    {
        _padron.ObtenerCarreraAsync("TEC", Arg.Any<CancellationToken>()).Returns((CarreraMinisterioDto?)null);

        var resultado = await CrearHandler().GenerarNominaPdfAsync(ComandoNomina(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La carrera no existe.", resultado.Message);
    }

    [Fact]
    public async Task Nomina_SinComisiones_DevuelveError_YNoGeneraReporte()
    {
        _padron.ObtenerCarreraAsync("TEC", Arg.Any<CancellationToken>()).Returns(Terciaria);
        _padron.ObtenerComisionesAsync("TEC", "1/26", null, Arg.Any<CancellationToken>()).Returns([]);

        var resultado = await CrearHandler().GenerarNominaPdfAsync(ComandoNomina(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No hay datos para mostrar.", resultado.Message);
        _reporte.DidNotReceiveWithAnyArgs().GenerarNomina(default!);
    }

    [Fact]
    public async Task Nomina_AgrupaPorComision_SeparaRecursantes_YFormateaDocumentoYEdad()
    {
        _padron.ObtenerCarreraAsync("TEC", Arg.Any<CancellationToken>()).Returns(Terciaria);
        _padron.ObtenerComisionesAsync("TEC", "1/26", null, Arg.Any<CancellationToken>()).Returns([(short)111, (short)112]);
        _padron.ObtenerAlumnosAsync("TEC", "1/26", null, Arg.Any<CancellationToken>()).Returns(
        [
            Alumno(111, "DNI12345678", "Pérez", nacimiento: new DateOnly(2006, 9, 30)),
            Alumno(111, "DNI23456789", "Gómez", condicion: "RECURSANDO", nacimiento: new DateOnly(2000, 1, 1)),
        ]);

        NominaMinisterioModel? capturado = null;
        _reporte.GenerarNomina(Arg.Do<NominaMinisterioModel>(m => capturado = m)).Returns([1, 2, 3]);

        var resultado = await CrearHandler().GenerarNominaPdfAsync(ComandoNomina(edad: true), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal([1, 2, 3], resultado.Value);
        Assert.NotNull(capturado);
        Assert.Equal("TECNICATURA SUPERIOR EN GESTIÓN", capturado!.CarreraLarga);
        Assert.Equal(2026, capturado.CicloLectivo);
        Assert.Equal(new DateOnly(2026, 9, 29), capturado.FechaInscriptosAl);
        Assert.True(capturado.ConEdadYNacionalidad);
        Assert.Equal(4m, capturado.MargenSuperiorCm);
        Assert.Equal(2, capturado.Secciones.Count);

        var primera = capturado.Secciones[0];
        Assert.Equal(111, primera.Cutuco);
        var cursante = Assert.Single(primera.Cursando);
        Assert.Equal("DNI 12.345.678", cursante.Documento);
        Assert.Equal(19, cursante.Edad);                 // cumple el 30/9: todavía 19
        var recursante = Assert.Single(primera.Recursantes);
        Assert.Equal("Gómez", recursante.Apellido);
        Assert.Equal(26, recursante.Edad);

        // La comisión 112 existe en COMARM pero no tiene alumnos: hoja vacía, como el legacy.
        Assert.Equal(112, capturado.Secciones[1].Cutuco);
        Assert.Empty(capturado.Secciones[1].Cursando);
        Assert.Empty(capturado.Secciones[1].Recursantes);
    }

    [Fact]
    public async Task Nomina_PropagaComisionYCancellationTokenALaQuery()
    {
        using var cts = new CancellationTokenSource();
        _padron.ObtenerCarreraAsync("TEC", cts.Token).Returns(Terciaria);
        _padron.ObtenerComisionesAsync("TEC", "1/26", (short)111, cts.Token).Returns([(short)111]);
        _padron.ObtenerAlumnosAsync("TEC", "1/26", (short)111, cts.Token).Returns([]);
        _reporte.GenerarNomina(Arg.Any<NominaMinisterioModel>()).Returns([1]);

        var resultado = await CrearHandler().GenerarNominaPdfAsync(ComandoNomina(cutuco: 111), cts.Token);

        Assert.True(resultado.IsSuccess);
        await _padron.Received(1).ObtenerAlumnosAsync("TEC", "1/26", (short)111, cts.Token);
    }

    // ---- Padrón Excel ----

    [Fact]
    public async Task Excel_ComisionNegativa_DevuelveError()
    {
        var resultado = await CrearHandler().ExportarExcelAsync(ComandoPadron(cutuco: -1), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La comisión debe ser un número positivo.", resultado.Message);
    }

    [Fact]
    public async Task Excel_SinAlumnos_DevuelveError_YNoGeneraLibro()
    {
        _padron.ObtenerCarreraAsync("TEC", Arg.Any<CancellationToken>()).Returns(Terciaria);
        _padron.ObtenerAlumnosAsync("TEC", "1/26", null, Arg.Any<CancellationToken>()).Returns([]);

        var resultado = await CrearHandler().ExportarExcelAsync(ComandoPadron(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No hay datos para mostrar.", resultado.Message);
        _excel.DidNotReceiveWithAnyArgs().GenerarPadron(default!);
    }

    [Fact]
    public async Task Excel_Terciaria_CodificaColumnasDelLayoutTerciario()
    {
        _padron.ObtenerCarreraAsync("TEC", Arg.Any<CancellationToken>()).Returns(Terciaria);
        _padron.ObtenerAlumnosAsync("TEC", "1/26", null, Arg.Any<CancellationToken>()).Returns(
        [
            Alumno(321, "DNI12345678", "Pérez", condicion: "RECURSANDO", sexo: "F", dniTutor: 999),
        ]);

        PadronMinisterioModel? capturado = null;
        _excel.GenerarPadron(Arg.Do<PadronMinisterioModel>(m => capturado = m)).Returns([7]);

        var resultado = await CrearHandler().ExportarExcelAsync(ComandoPadron(separadas: true), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(capturado);
        Assert.True(capturado!.EsTerciaria);
        Assert.True(capturado.HojasSeparadas);
        var fila = Assert.Single(capturado.Filas);
        Assert.Equal("TEC-321", fila.NombreHoja);
        Assert.Equal("1234/20", fila.Resolucion);
        Assert.Equal("TC", fila.Orientacion);
        Assert.Equal("G", fila.TipoCarrera);
        Assert.Equal("P", fila.Modalidad);
        Assert.Equal("TT", fila.Turno);
        Assert.Equal(2, fila.AnioEstudio);
        Assert.Equal(3, fila.Cuatrimestre);
        Assert.Equal("A", fila.Division);
        Assert.Equal("DNI", fila.TipoDocumento);
        Assert.Equal("12.345.678", fila.NumeroDocumento);
        Assert.Equal("MUJER", fila.Genero);
        Assert.Equal("RC", fila.Condicion);
        Assert.Equal("Colegio Bachiller", fila.TituloIngreso);
        // El adulto responsable no forma parte del layout terciario.
        Assert.Null(fila.ApellidoTutor);
        Assert.Null(fila.TipoDocumentoTutor);
        Assert.Null(fila.DniTutor);
    }

    [Fact]
    public async Task Excel_Bachillerato_CodificaColumnasDelLayoutSecundario()
    {
        _padron.ObtenerCarreraAsync("TEC", Arg.Any<CancellationToken>()).Returns(Bachillerato);
        _padron.ObtenerAlumnosAsync("TEC", "1/26", null, Arg.Any<CancellationToken>()).Returns(
        [
            Alumno(142, "DNI12345678", "Pérez", sexo: "M", dniTutor: 999),
            Alumno(142, "DNI22345678", "Ruiz", sexo: "M"),
        ]);

        PadronMinisterioModel? capturado = null;
        _excel.GenerarPadron(Arg.Do<PadronMinisterioModel>(m => capturado = m)).Returns([7]);

        var resultado = await CrearHandler().ExportarExcelAsync(ComandoPadron(), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.False(capturado!.EsTerciaria);
        Assert.Equal(2, capturado.Filas.Count);

        var conTutor = capturado.Filas[0];
        Assert.Equal("C", conTutor.Modalidad);
        Assert.Equal("TN", conTutor.Turno);
        Assert.Equal(1, conTutor.AnioEstudio);
        Assert.Equal("B", conTutor.Division);
        Assert.Equal("VARON", conTutor.Genero);
        Assert.Equal("Tutor", conTutor.ApellidoTutor);
        Assert.Equal("DNI", conTutor.TipoDocumentoTutor);
        Assert.Equal(999, conTutor.DniTutor);
        // Columnas exclusivas del layout terciario quedan vacías.
        Assert.Null(conTutor.Orientacion);
        Assert.Null(conTutor.TipoCarrera);
        Assert.Null(conTutor.Cuatrimestre);
        Assert.Null(conTutor.Condicion);
        Assert.Null(conTutor.TituloIngreso);

        var sinTutor = capturado.Filas[1];
        Assert.Null(sinTutor.ApellidoTutor);
        Assert.Null(sinTutor.TipoDocumentoTutor);
        Assert.Null(sinTutor.DniTutor);
    }
}
