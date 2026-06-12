namespace Esba.Domain.Enums;

/// <summary>
/// ALUMNOS.GENERO INTEGER: índice del combo CbGenero de FrmAltaAlumno guardado
/// tal cual (0 a 16). El orden de los miembros replica exactamente el orden de
/// los ítems del combo legacy — no reordenar.
/// </summary>
public enum Genero
{
    SinEspecificar = 0,
    Cisgenero = 1,
    Cissexual = 2,
    Transgenero = 3,
    Transexual = 4,
    Cuigenero = 5,
    TercerGenero = 6,
    Agenero = 7,
    GeneroNeutro = 8,
    Intergenero = 9,
    Androgino = 10,
    Bigenero = 11,
    GeneroFluido = 12,
    Poligenero = 13,
    Pangenero = 14,
    SemiMujer = 15,
    SemiHombre = 16,
}
