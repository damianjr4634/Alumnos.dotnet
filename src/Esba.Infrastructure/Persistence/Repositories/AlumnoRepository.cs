using Esba.Application.Abstractions;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class AlumnoRepository : IAlumnoRepository
{
    private readonly EsbaDbContext _contexto;

    public AlumnoRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Alumno?> ObtenerAsync(string codigoCarrera, string codigo, CancellationToken ct) =>
        _contexto.Alumnos
            .FirstOrDefaultAsync(a => a.CodigoCarrera == codigoCarrera && a.Codigo == codigo, ct);

    public void Agregar(Alumno alumno) => _contexto.Alumnos.Add(alumno);
}
