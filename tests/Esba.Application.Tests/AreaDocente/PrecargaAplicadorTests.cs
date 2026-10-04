using Esba.Application.DTOs.Academica;
using Esba.Application.DTOs.AreaDocente;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.AreaDocente;

namespace Esba.Application.Tests.AreaDocente;

/// <summary>
/// "Tomar valores del docente": solo se tocan los alumnos con detalle, solo los campos
/// que edita cada variante, y el resto de la fila queda intacto.
/// </summary>
public class PrecargaAplicadorTests
{
    private static CargaComisionDocenteDto Precarga(params AlumnoCargaComisionDto[] alumnos) => new()
    {
        CodigoCarrera = "TER", Cutuco = 101, CodigoMateria = "01", CuatrimestreAnio = "226", CargaId = 1, Alumnos = alumnos,
    };

    private static AlumnoCargaComisionDto Detalle(string alumno) => new()
    {
        CodigoAlumno = alumno, DetalleId = 5,
        Evaluacion1 = 7m, Evaluacion2 = 8m, Recuperatorio = null, Evaluacion3 = 6m, NotaRegular = 9m, NotaFinal = 4m,
        TotalHoras = 64, Inasistencias = 5, Justificadas = 1,
    };

    [Fact]
    public void Terciaria_AplicaSoloALosAlumnosConDetalleYRespetaElResto()
    {
        var filas = new List<RegularizacionCursadaDto>
        {
            new() { CodigoAlumno = "A1", CodigoMateria = "01", CuatrimestreAnio = "226", TpEva = 2m, TotalHoras = 10, Condicion = "CURSANDO" },
            new() { CodigoAlumno = "A2", CodigoMateria = "01", CuatrimestreAnio = "226", TpEva = 3m, Condicion = "CURSANDO" },
        };
        // A1 con detalle, A2 sin detalle (DetalleId null), A3 no está en pantalla.
        var precarga = Precarga(Detalle("A1 "), new AlumnoCargaComisionDto { CodigoAlumno = "A2" }, Detalle("A3"));

        var r = PrecargaAplicador.Terciaria(filas, precarga);

        Assert.Equal(1, r.Aplicadas);
        Assert.Equal(1, r.SinPrecarga);
        var a1 = r.Filas.Single(f => f.CodigoAlumno == "A1");
        Assert.Equal(7m, a1.TpEva);
        Assert.Equal(8m, a1.TpEva2);
        Assert.Null(a1.Recuperatorio);
        Assert.Equal((short)64, a1.TotalHoras);
        Assert.Equal((short)5, a1.Inasistencias);
        Assert.Equal((short)1, a1.Justificadas);
        Assert.Equal("CURSANDO", a1.Condicion); // lo que no es nota no se toca
        var a2 = r.Filas.Single(f => f.CodigoAlumno == "A2");
        Assert.Equal(3m, a2.TpEva);
    }

    [Fact]
    public void Bachillerato_AgregaNotaARegularizar()
    {
        var filas = new List<RegularizacionBachilleratoDto>
        {
            new() { CodigoAlumno = "A1", CodigoMateria = "01", CuatrimestreAnio = "226", NotaRegular = 1m, NotaDefinitiva = 10m },
        };

        var r = PrecargaAplicador.Bachillerato(filas, Precarga(Detalle("A1")));

        var a1 = r.Filas.Single();
        Assert.Equal(9m, a1.NotaRegular);
        Assert.Equal(7m, a1.TpEva);
        Assert.Equal(10m, a1.NotaDefinitiva); // campo de secretaría, intacto
    }

    [Fact]
    public void Secundario_TresTrimestresSinTocarDiciembreNiMarzo()
    {
        var filas = new List<Regularizacion333Dto>
        {
            new() { CodigoAlumno = "A1", CodigoMateria = "01", CuatrimestreAnio = "226", NotaDic = 5m, Justificadas = 3 },
        };

        var r = PrecargaAplicador.Secundario(filas, Precarga(Detalle("A1")));

        var a1 = r.Filas.Single();
        Assert.Equal(7m, a1.TpEva);
        Assert.Equal(8m, a1.TpEva2);
        Assert.Equal(6m, a1.TpEva3);
        Assert.Equal((short)64, a1.TotalHoras);
        Assert.Equal(5m, a1.NotaDic);          // de secretaría
        Assert.Equal((short)3, a1.Justificadas); // el secundario no las precarga
    }

    [Fact]
    public void Cna_SoloNotaFinal()
    {
        var filas = new List<RegularizacionCnaDto>
        {
            new() { CodigoAlumno = "A1", CodigoMateria = "01", CuatrimestreAnio = "226", NotaFinal = 1m },
        };

        var r = PrecargaAplicador.Cna(filas, Precarga(Detalle("A1")));

        Assert.Equal(4m, r.Filas.Single().NotaFinal);
        Assert.Equal(1, r.Aplicadas);
    }

    [Fact]
    public void Mesa_PoneLaNotaEnElLlamadoVigenteConFechaDeMesaYSalteaAusentes()
    {
        var fechaMesa = new DateOnly(2026, 7, 17);
        var filas = new List<CargaFinalAlumnoDto>
        {
            new() { CodigoAlumno = "A1", CodigoCarrera = "TER", CodigoMateria = "01", NumeroFinal = 1 },
            new() { CodigoAlumno = "A2", CodigoCarrera = "TER", CodigoMateria = "01", NumeroFinal = 2, NotaFinal1 = 2m, FechaFinal1 = new DateOnly(2025, 12, 1) },
            new() { CodigoAlumno = "A3", CodigoCarrera = "TER", CodigoMateria = "01", NumeroFinal = 1 },
            new() { CodigoAlumno = "A4", CodigoCarrera = "TER", CodigoMateria = "01", NumeroFinal = 1 },
        };
        var precarga = new CargaMesaDocenteDto
        {
            CodigoCarrera = "TER", NumeroMesa = 5001, CargaId = 1, FechaExamen = fechaMesa,
            Alumnos =
            [
                new() { CodigoAlumno = "A1", CodigoMateria = "01", DetalleId = 1, Nota = 8m },
                new() { CodigoAlumno = "A2", CodigoMateria = "01", DetalleId = 2, Nota = 6m },
                new() { CodigoAlumno = "A3", CodigoMateria = "01", DetalleId = 3, Ausente = true },
                new() { CodigoAlumno = "A4", CodigoMateria = "01" },
            ],
        };

        var r = PrecargaAplicador.Mesa(filas, precarga);

        Assert.Equal(2, r.Aplicadas);
        Assert.Equal(1, r.Ausentes);
        Assert.Equal(1, r.SinPrecarga);
        var a1 = r.Filas.Single(f => f.CodigoAlumno == "A1");
        Assert.Equal(8m, a1.NotaFinal1);
        Assert.Equal(fechaMesa, a1.FechaFinal1);
        var a2 = r.Filas.Single(f => f.CodigoAlumno == "A2");
        Assert.Equal(6m, a2.NotaFinal2);        // llamado vigente = 2
        Assert.Equal(fechaMesa, a2.FechaFinal2);
        Assert.Equal(2m, a2.NotaFinal1);        // el final anterior no se toca
        var a3 = r.Filas.Single(f => f.CodigoAlumno == "A3");
        Assert.Null(a3.NotaFinal1);             // ausente: secretaría decide
    }
}
