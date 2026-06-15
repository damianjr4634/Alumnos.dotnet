namespace Esba.Application.DTOs.Examenes;

/// <summary>Campos editables de una mesa de examen, comunes al alta y a la modificación.</summary>
public interface IMesaCampos
{
    string CodigoCarrera { get; }

    int NumeroMesa { get; }

    string? CodigoMateria { get; }

    int Llamado { get; }

    DateOnly FechaExamen { get; }

    /// <summary>Hora como entero (ej. 1830); 0 = sin hora.</summary>
    int Hora { get; }

    string? Titular { get; }

    string? Vocal1 { get; }

    string? Vocal2 { get; }

    /// <summary>0 = sin aula.</summary>
    int Aula { get; }

    /// <summary>Comisiones que rinden; 0 = ninguna.</summary>
    int Comision1 { get; }

    int Comision2 { get; }

    int Comision3 { get; }

    /// <summary>TIPMES: tipo de mesa (obligatorio).</summary>
    string? CodigoTipo { get; }
}
