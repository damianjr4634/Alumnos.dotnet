using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.DTOs.Administracion;
using Esba.Application.DTOs.Alumnos;
using Esba.Application.DTOs.Asistencias;
using Esba.Application.DTOs.Certificados;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Academica;
using Esba.Application.Features.Administracion;
using Esba.Application.Features.Alumnos;
using Esba.Application.Features.Asistencias;
using Esba.Application.Features.Certificados;
using Esba.Application.Features.Examenes;
using Esba.Application.Validators;
using Esba.Infrastructure.Email;
using Esba.Infrastructure.Excel;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.Queries;
using Esba.Infrastructure.Reports;
using Esba.Infrastructure.Security;
using Esba.Infrastructure.StoredProcedures;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Esba.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la infraestructura. En Blazor Server se usa IDbContextFactory
    /// para evitar contextos compartidos entre renders concurrentes del mismo
    /// circuito (migration_improvements.md §2.3.2).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Esba")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'Esba' en la configuración.");

        services.AddDbContextFactory<EsbaDbContext>(options => options.UseFirebird(connectionString));

        services.AddSingleton(new FbConnectionFactory(connectionString));
        services.AddScoped<IAlumnosQuery, AlumnosQuery>();
        services.AddScoped<ICarrerasQuery, CarrerasQuery>();

        // DbContext scoped resuelto desde la factory (un contexto por scope/circuito).
        services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<EsbaDbContext>>().CreateDbContext());
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Seguridad (§2.7): hash nuevo + cifrado legacy solo para la transición.
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ILegacyPasswordCipher, EncriptoCadena2Cipher>();

        // Casos de uso y validadores de Application.
        services.AddScoped<IValidator<IniciarSesionCommand>, IniciarSesionValidator>();
        services.AddScoped<IniciarSesionHandler>();

        // Administración: ABM de usuarios (hito 10.1a).
        services.AddScoped<IUsuariosQuery, UsuariosQuery>();
        services.AddScoped<IValidator<CrearUsuarioCommand>, CrearUsuarioValidator>();
        services.AddScoped<IValidator<ActualizarUsuarioCommand>, ActualizarUsuarioValidator>();
        services.AddScoped<CrearUsuarioHandler>();
        services.AddScoped<ActualizarUsuarioHandler>();
        services.AddScoped<DarDeBajaUsuarioHandler>();
        services.AddScoped<ReactivarUsuarioHandler>();

        // Administración: permisos por usuario (hito 10.1b). Wrappers de YYY_SEGU_*.
        services.AddScoped<ISeguOpcionesProcedure, SeguOpcionesProcedure>();
        services.AddScoped<ISeguGrabaProcedure, SeguGrabaProcedure>();
        services.AddScoped<IValidator<AsignarPermisosUsuarioCommand>, AsignarPermisosUsuarioValidator>();
        services.AddScoped<AsignarPermisosUsuarioHandler>();

        // Administración: contraseña — cambio propio y blanqueo por admin (hito 10.1c).
        services.AddScoped<IValidator<CambiarPasswordCommand>, CambiarPasswordValidator>();
        services.AddScoped<IValidator<BlanquearPasswordCommand>, BlanquearPasswordValidator>();
        services.AddScoped<CambiarPasswordHandler>();
        services.AddScoped<BlanquearPasswordHandler>();

        // Administración: configuración general XXX_CONF (hito 10.3a). Clave-valor por EF.
        services.AddScoped<IConfiguracionQuery, ConfiguracionQuery>();
        services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
        services.AddScoped<IValidator<ActualizarConfiguracionCommand>, ActualizarConfiguracionValidator>();
        services.AddScoped<ActualizarConfiguracionHandler>();

        // Administración: correo SMTP institucional (hito 10.3b). Cuenta única global;
        // credenciales en user-secrets/entorno (§2.7). Envío con MailKit.
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.AddScoped<IEmailService, MailKitEmailService>();
        services.AddScoped<IValidator<EnviarCorreoPruebaCommand>, EnviarCorreoPruebaValidator>();
        services.AddScoped<EnviarCorreoPruebaHandler>();

        // Administración: correo por comisión (hito 10.4b). Un mensaje + copia de auditoría.
        services.AddScoped<IValidator<EnviarCorreoComisionCommand>, EnviarCorreoComisionValidator>();
        services.AddScoped<EnviarCorreoComisionHandler>();

        // Administración: ABM de profesores (hito 10.2). DOCENTES por EF (sin SP).
        services.AddScoped<IDocenteRepository, DocenteRepository>();
        services.AddScoped<IValidator<CrearDocenteCommand>, CrearDocenteValidator>();
        services.AddScoped<IValidator<ActualizarDocenteCommand>, ActualizarDocenteValidator>();
        services.AddScoped<CrearDocenteHandler>();
        services.AddScoped<ActualizarDocenteHandler>();
        services.AddScoped<DarDeBajaDocenteHandler>();
        services.AddScoped<ReactivarDocenteHandler>();

        services.AddScoped<IAlumnoRepository, AlumnoRepository>();
        services.AddScoped<IValidator<CrearAlumnoCommand>, CrearAlumnoValidator>();
        services.AddScoped<IValidator<ActualizarAlumnoCommand>, ActualizarAlumnoValidator>();
        services.AddScoped<CrearAlumnoHandler>();
        services.AddScoped<ActualizarAlumnoHandler>();

        // Wrappers de SP legacy (§1.3: única vía de invocación de los XXX_*).
        services.AddScoped<ICambioDniLibroMatrizProcedure, CambioDniLibroMatrizProcedure>();
        services.AddScoped<ICuatrimestreVigenteProcedure, CuatrimestreVigenteProcedure>();

        // Académica: inscripción de materias.
        services.AddScoped<ICursadaQuery, CursadaQuery>();
        services.AddScoped<IMateriasQuery, MateriasQuery>();
        services.AddScoped<ICursadaRepository, CursadaRepository>();
        services.AddScoped<IMateriaRepository, MateriaRepository>();
        services.AddScoped<IValidator<InscribirEnMateriaCommand>, InscribirEnMateriaValidator>();
        services.AddScoped<IValidator<ModificarInscripcionCommand>, ModificarInscripcionValidator>();
        services.AddScoped<InscribirEnMateriaHandler>();
        services.AddScoped<ModificarInscripcionHandler>();
        services.AddScoped<EliminarInscripcionHandler>();

        // Académica: regularización de materias (hito 15). Porta a C# el cálculo de
        // condición (_TERC terciarias, _BAC/_POSTVAL bachillerato) y el commit
        // (XXX_REGULARIZACION), sin "$$$CURSADA".
        services.AddScoped<IRegularizacionQuery, RegularizacionQuery>();
        services.AddScoped<IRegularizacionRepository, RegularizacionRepository>();
        services.AddScoped<IValidator<ConfirmarRegularizacionCommand>, ConfirmarRegularizacionValidator>();
        services.AddScoped<ConfirmarRegularizacionHandler>();
        services.AddScoped<IValidator<ConfirmarRegularizacionBachilleratoCommand>, ConfirmarRegularizacionBachilleratoValidator>();
        services.AddScoped<ConfirmarRegularizacionBachilleratoHandler>();
        services.AddScoped<IValidator<ConfirmarRegularizacion333Command>, ConfirmarRegularizacion333Validator>();
        services.AddScoped<ConfirmarRegularizacion333Handler>();
        services.AddScoped<IValidator<ConfirmarRegularizacionCnaCommand>, ConfirmarRegularizacionCnaValidator>();
        services.AddScoped<ConfirmarRegularizacionCnaHandler>();

        // Académica: alta de equivalencias (hito 9.3b).
        services.AddScoped<IAnaliticoRepository, AnaliticoRepository>();
        services.AddScoped<IValidacionMateriaProcedure, ValidacionMateriaProcedure>();
        services.AddScoped<IEquivalenciaNumeracionProcedure, EquivalenciaNumeracionProcedure>();
        services.AddScoped<IValidator<CrearEquivalenciaCommand>, CrearEquivalenciaValidator>();
        services.AddScoped<CrearEquivalenciaHandler>();

        // Académica: ABM de materias (hito 6).
        services.AddScoped<IValidator<CrearMateriaCommand>, CrearMateriaValidator>();
        services.AddScoped<IValidator<ActualizarMateriaCommand>, ActualizarMateriaValidator>();
        services.AddScoped<CrearMateriaHandler>();
        services.AddScoped<ActualizarMateriaHandler>();

        // Académica: comisiones y docentes (hito 6).
        services.AddScoped<IComisionesQuery, ComisionesQuery>();
        services.AddScoped<IDocentesQuery, DocentesQuery>();
        services.AddScoped<IComisionRepository, ComisionRepository>();
        services.AddScoped<IValidoComisionProcedure, ValidoComisionProcedure>();
        services.AddScoped<IValidator<CrearComisionCommand>, CrearComisionValidator>();
        services.AddScoped<IValidator<ActualizarComisionCommand>, ActualizarComisionValidator>();
        services.AddScoped<CrearComisionHandler>();
        services.AddScoped<ActualizarComisionHandler>();
        services.AddScoped<EliminarComisionHandler>();

        // Académica: inscripción masiva por cuatrimestre (deuda hito 6, dos fases).
        services.AddScoped<IInscripcionMasivaCuatrimestreProcedure, InscripcionMasivaCuatrimestreProcedure>();
        services.AddScoped<IValidator<InscribirCuatrimestreCompletoCommand>, InscribirCuatrimestreCompletoValidator>();
        services.AddScoped<InscribirCuatrimestreCompletoHandler>();

        // Asistencias: lectura (hito 7, increment 1).
        services.AddScoped<ITipoFaltasQuery, TipoFaltasQuery>();
        services.AddScoped<IFaltasComisionProcedure, FaltasComisionProcedure>();
        services.AddScoped<IFaltasAlumnoProcedure, FaltasAlumnoProcedure>();

        // Asistencias: carga de inasistencias por comisión (hito 7, increment 2).
        services.AddScoped<IInasistenciasRepository, InasistenciasRepository>();
        services.AddScoped<IValidator<GuardarInasistenciasComisionCommand>, GuardarInasistenciasComisionValidator>();
        services.AddScoped<GuardarInasistenciasComisionHandler>();

        // Asistencias: planillas + pase a libre (hito 7, increment 3).
        services.AddScoped<IPlanillaInasistenciasProcedure, PlanillaInasistenciasProcedure>();
        services.AddScoped<IPaseLibreProcedure, PaseLibreProcedure>();
        services.AddScoped<PasarMateriasALibreHandler>();

        // Exámenes: mesas (hito 8).
        services.AddScoped<IMesasQuery, MesasQuery>();
        services.AddScoped<ITipoMesaQuery, TipoMesaQuery>();
        services.AddScoped<IMesaRepository, MesaRepository>();
        services.AddScoped<IValidoMesaProcedure, ValidoMesaProcedure>();
        services.AddScoped<IValidator<CrearMesaCommand>, CrearMesaValidator>();
        services.AddScoped<IValidator<ActualizarMesaCommand>, ActualizarMesaValidator>();
        services.AddScoped<CrearMesaHandler>();
        services.AddScoped<ActualizarMesaHandler>();
        services.AddScoped<EliminarMesaHandler>();

        // Exámenes: permisos de examen (hito 8, increment 3).
        services.AddScoped<IMateriasFinalesProcedure, MateriasFinalesProcedure>();
        services.AddScoped<IPermisosExamenRepository, PermisosExamenRepository>();
        services.AddScoped<IValidator<CrearPermisoExamenCommand>, CrearPermisoExamenValidator>();
        services.AddScoped<CrearPermisoExamenHandler>();
        services.AddScoped<EliminarPermisoExamenHandler>();
        services.AddScoped<GuardarPermisosMasivoHandler>();

        // Exámenes: carga de notas de finales por mesa y comisión (hito 14).
        // Porta XXX_MESAS a C# sin staging; el cálculo de condición es de dominio.
        services.AddScoped<ICargaFinalCandidatosQuery, CargaFinalCandidatosQuery>();
        services.AddScoped<ICargaFinalRepository, CargaFinalRepository>();
        services.AddScoped<IValidator<CargaNotasFinalCommand>, CargaNotasFinalValidator>();
        services.AddScoped<ConfirmarCargaNotasFinalHandler>();

        // Exámenes: actas de examen (hito 14). Reportes de lectura (PDF Oficio + Excel)
        // por comisión (A/REGULAR, Reincorporación, Exámenes) y volante por mesa.
        services.AddScoped<IActasQuery, ActasQuery>();
        services.AddScoped<IActaReportService, ActaPdfService>();
        services.AddSingleton<IActaExcelService, ActaExcelService>();
        services.AddScoped<IValidator<GenerarActaComisionCommand>, GenerarActaComisionValidator>();
        services.AddScoped<IValidator<GenerarActaMesaCommand>, GenerarActaMesaValidator>();
        services.AddScoped<GenerarActaComisionHandler>();
        services.AddScoped<GenerarActaMesaHandler>();

        // Exportación de listados (hito 5): genéricos para EsbaListView. Sin estado.
        services.AddSingleton<IExcelExportService, ClosedXmlExportService>();
        services.AddSingleton<IPdfExportService, QuestPdfExportService>();

        // Certificados: constancia de alumno (hito 9.1). Primer reporte con formato propio.
        services.Configure<InstitucionSettings>(configuration.GetSection(InstitucionSettings.SectionName));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IConstanciasQuery, ConstanciasQuery>();
        services.AddScoped<ICertificadoEnTramiteProcedure, CertificadoEnTramiteProcedure>();
        services.AddScoped<IPaseAlumnoProcedure, PaseAlumnoProcedure>();
        services.AddScoped<IParrafoConstanciaProcedure, ParrafoConstanciaProcedure>();
        services.AddScoped<IConstanciaMateriasProcedure, ConstanciaMateriasProcedure>();
        services.AddScoped<IConstanciaReportService, ConstanciaPdfService>();
        services.AddScoped<IValidator<GenerarConstanciaCommand>, GenerarConstanciaCommandValidator>();
        services.AddScoped<GenerarConstanciaAlumnoHandler>();

        // Certificados: analítico tabular + promedio general (hito 9.2a).
        services.AddScoped<IPromedioGeneralProcedure, PromedioGeneralProcedure>();
        services.AddScoped<ObtenerAnaliticoAlumnoHandler>();

        // Certificados: reporte "Constancia de Materias Aprobadas" (hito 9.2b).
        services.AddScoped<IConstanciaAnaliticoReportService, ConstanciaAnaliticoPdfService>();
        services.AddScoped<GenerarConstanciaMateriasAprobadasHandler>();

        // Certificados: Constancia de Examen Final (hito 9.2c).
        services.AddScoped<GenerarConstanciaExamenFinalHandler>();

        // Certificados: impresión de equivalencia bachiller (hito 9.3c).
        services.AddScoped<IEquivalenciaBachillerProcedure, ImpresionEquivalenciaBachillerProcedure>();
        services.AddScoped<IEquivalenciaBachillerReportService, EquivalenciaBachillerPdfService>();
        services.AddScoped<GenerarEquivalenciaBachillerHandler>();

        // Certificados: Constancia de Alumno Regular (hito 10.4a). A4 + membrete de fondo.
        services.AddScoped<IConstanciaRegularReportService, ConstanciaRegularPdfService>();
        services.AddScoped<IValidator<GenerarConstanciaRegularCommand>, GenerarConstanciaRegularCommandValidator>();
        services.AddScoped<GenerarConstanciaRegularHandler>();

        // Certificados: resolución de equivalencia terciaria (hito 9.3d).
        services.AddScoped<IEquivalenciaTerciariaQuery, EquivalenciaTerciariaQuery>();
        services.AddScoped<IResolucionEquivalenciaReportService, ResolucionEquivalenciaTerciariaPdfService>();
        services.AddScoped<GenerarResolucionEquivalenciaTerciariaHandler>();

        return services;
    }
}
