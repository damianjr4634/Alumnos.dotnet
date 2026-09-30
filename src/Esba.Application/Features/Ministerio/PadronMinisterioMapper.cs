using Esba.Application.DTOs.Ministerio;
using Esba.Domain.Ministerio;

namespace Esba.Application.Features.Ministerio;

/// <summary>
/// Proyección de las filas crudas del padrón a las columnas codificadas que se
/// informan al Ministerio (sucesor de las expresiones IIF/CASE del SELECT de
/// ComisionesAlMinisterio.pas). Es público para que el test de equivalencia contra el
/// SQL legacy compare exactamente lo que exporta el handler.
/// </summary>
public static class PadronMinisterioMapper
{
    public static FilaPadronMinisterioDto Fila(CarreraMinisterioDto carrera, PadronMinisterioAlumnoDto alumno)
    {
        ArgumentNullException.ThrowIfNull(carrera);
        ArgumentNullException.ThrowIfNull(alumno);

        var esTerciaria = CodificacionMinisterio.EsTerciaria(carrera.Tipo);

        return new FilaPadronMinisterioDto
        {
            Cutuco = alumno.Cutuco,
            NombreHoja = CodificacionMinisterio.NombreHoja(carrera.Codigo, alumno.Cutuco),
            Resolucion = carrera.Resolucion,
            Orientacion = esTerciaria ? CodificacionMinisterio.Orientacion(carrera.Nombre) : null,
            TipoCarrera = esTerciaria ? CodificacionMinisterio.TipoCarreraGrado : null,
            Modalidad = CodificacionMinisterio.Modalidad(carrera.Tipo, carrera.EsADistancia),
            Turno = CodificacionMinisterio.Turno(alumno.Cutuco, carrera.EsADistancia),
            AnioEstudio = CodificacionMinisterio.AnioEstudio(alumno.Cutuco, carrera.Codigo),
            Cuatrimestre = esTerciaria ? CodificacionMinisterio.Cuatrimestre(alumno.Cutuco) : null,
            Division = CodificacionMinisterio.Division(alumno.Cutuco),
            Apellido = alumno.Apellido,
            Nombre = alumno.Nombre,
            TipoDocumento = CodificacionMinisterio.TipoDocumento(alumno.CodigoAlumno),
            NumeroDocumento = CodificacionMinisterio.NumeroDocumentoConPuntos(alumno.CodigoAlumno),
            Genero = CodificacionMinisterio.Genero(alumno.Sexo),
            FechaNacimiento = alumno.FechaNacimiento,
            PaisNacimiento = alumno.Nacionalidad,
            LugarNacimiento = alumno.LugarNacimiento,
            Domicilio = alumno.Domicilio,
            CodigoPostal = alumno.CodigoPostal,
            Localidad = alumno.Localidad,
            Condicion = esTerciaria ? CodificacionMinisterio.Condicion(alumno.Condicion) : null,
            TituloIngreso = esTerciaria
                ? CodificacionMinisterio.TituloIngreso(alumno.ColegioSecundario, alumno.TituloSecundario)
                : null,
            ApellidoTutor = esTerciaria ? null : alumno.ApellidoTutor,
            NombreTutor = esTerciaria ? null : alumno.NombreTutor,
            TipoDocumentoTutor = !esTerciaria && alumno.DniTutor.HasValue ? "DNI" : null,
            DniTutor = esTerciaria ? null : alumno.DniTutor,
        };
    }

    public static NominaMinisterioAlumnoDto AlumnoNomina(PadronMinisterioAlumnoDto alumno, DateOnly fechaEmision)
    {
        ArgumentNullException.ThrowIfNull(alumno);

        return new NominaMinisterioAlumnoDto
        {
            CodigoAlumno = alumno.CodigoAlumno.Trim(),
            Apellido = alumno.Apellido,
            Nombre = alumno.Nombre,
            Documento = CodificacionMinisterio.DocumentoParaNomina(alumno.CodigoAlumno),
            Nacionalidad = alumno.Nacionalidad,
            Edad = CodificacionMinisterio.Edad(alumno.FechaNacimiento, fechaEmision),
        };
    }
}
