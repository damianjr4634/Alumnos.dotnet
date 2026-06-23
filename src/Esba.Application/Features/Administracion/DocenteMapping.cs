using Esba.Application.DTOs.Academica;
using Esba.Domain.Entities;

namespace Esba.Application.Features.Administracion;

/// <summary>Vuelca los campos editables de los comandos de docente sobre la entidad.</summary>
internal static class DocenteMapping
{
    public static void Aplicar(Docente d, CrearDocenteCommand c) =>
        AplicarCampos(d, c.Nombre, c.TipoDocumento, c.NumeroDocumento, c.FechaNacimiento, c.Direccion,
            c.Piso, c.Departamento, c.CodigoPostal, c.Localidad, c.TelefonoParticular, c.TelefonoMensajes,
            c.Interno, c.FechaIngreso, c.EnLicencia, c.FechaLicencia);

    public static void Aplicar(Docente d, ActualizarDocenteCommand c) =>
        AplicarCampos(d, c.Nombre, c.TipoDocumento, c.NumeroDocumento, c.FechaNacimiento, c.Direccion,
            c.Piso, c.Departamento, c.CodigoPostal, c.Localidad, c.TelefonoParticular, c.TelefonoMensajes,
            c.Interno, c.FechaIngreso, c.EnLicencia, c.FechaLicencia);

    private static void AplicarCampos(Docente d, string? nombre, string? tipoDoc, string? nroDoc,
        DateOnly? fechaNac, string? direccion, string? piso, string? depto, string? codPostal,
        string? localidad, string? telParticular, string? telMensajes, string? interno,
        DateOnly? fechaIngreso, bool enLicencia, DateOnly? fechaLicencia)
    {
        d.Nombre = Limpiar(nombre);
        d.TipoDocumento = Limpiar(tipoDoc);
        d.NumeroDocumento = Limpiar(nroDoc);
        d.FechaNacimiento = fechaNac;
        d.Direccion = Limpiar(direccion);
        d.Piso = Limpiar(piso);
        d.Departamento = Limpiar(depto);
        d.CodigoPostal = Limpiar(codPostal);
        d.Localidad = Limpiar(localidad);
        d.TelefonoParticular = Limpiar(telParticular);
        d.TelefonoMensajes = Limpiar(telMensajes);
        d.Interno = Limpiar(interno);
        d.FechaIngreso = fechaIngreso;
        d.EnLicencia = enLicencia;
        d.FechaLicencia = fechaLicencia;
    }

    private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
