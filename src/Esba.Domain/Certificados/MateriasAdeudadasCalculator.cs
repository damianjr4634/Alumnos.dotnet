namespace Esba.Domain.Certificados;

/// <summary>
/// Calcula la línea "Materias que adeuda" de la constancia de alumno, replicando
/// la agregación del legacy <c>constanciaalumnos2.pas</c> (Impresion_Analitico_Pase):
/// acumula las siglas de materias pendientes por cuatrimestre y, si en un
/// cuatrimestre se adeudan todas, las colapsa a "TODAS LAS".
/// </summary>
public static class MateriasAdeudadasCalculator
{
    // Cantidad de materias por cuatrimestre que el legacy considera "todas":
    // 5 en el primer cuatrimestre, 6 en los demás (regla Cont=5/Cuat=1, Cont=6/Cuat<>1).
    private const int TodasPrimerCuatrimestre = 5;
    private const int TodasOtrosCuatrimestres = 6;

    private const string Ninguna = "NINGUNA";

    /// <summary>
    /// Devuelve el texto de materias adeudadas (sin el rótulo "* Materias que adeuda: ").
    /// </summary>
    /// <param name="materias">Filas en el orden del SP (por cuatrimestre).</param>
    /// <param name="cuatrimestreMaximo">
    /// Tope de cuatrimestre a considerar (FCUATRI del SP de validación): si es
    /// distinto de 0, se ignoran las materias de cuatrimestres superiores (título
    /// intermedio). 0 = sin tope.
    /// </param>
    /// <param name="esCarreraPorAnio">
    /// true para carreras anuales (333/650): el separador dice "AÑO" en vez de "CUAT.".
    /// </param>
    public static string Calcular(
        IReadOnlyList<MateriaConstancia> materias,
        int cuatrimestreMaximo,
        bool esCarreraPorAnio)
    {
        ArgumentNullException.ThrowIfNull(materias);

        var separadorCuatrimestre = esCarreraPorAnio ? " AÑO; " : " CUAT.; ";

        var cuatrimestre = 1;
        var contadorPendientes = 0;
        var siglas = string.Empty;
        var todas = string.Empty;

        void CerrarCuatrimestre()
        {
            if (siglas.Length == 0)
            {
                return;
            }

            var bloque = QuitarSeparadorFinal(siglas);
            if (EsTodoElCuatrimestre(contadorPendientes, cuatrimestre))
            {
                bloque = "TODAS LAS ";
            }

            todas += bloque + " DEL " + TextoCastellano.CuatrimestreEnLetras(cuatrimestre) + separadorCuatrimestre;
        }

        foreach (var materia in materias)
        {
            if (cuatrimestreMaximo != 0 && materia.Cuatrimestre > cuatrimestreMaximo)
            {
                break;
            }

            if (materia.Cuatrimestre != cuatrimestre)
            {
                CerrarCuatrimestre();
                contadorPendientes = 0;
                cuatrimestre = materia.Cuatrimestre;
                siglas = string.Empty;
            }

            if (EsPendiente(materia))
            {
                contadorPendientes++;
                siglas += materia.Sigla + ", ";
            }
        }

        CerrarCuatrimestre();

        todas = QuitarSeparadorFinal(todas);
        return todas.Length == 0 ? Ninguna : todas;
    }

    private static bool EsPendiente(MateriaConstancia materia)
    {
        var condicion = materia.Condicion ?? string.Empty;
        if (condicion is "EQUIVALENCIA" or "EXIMIDO")
        {
            return false;
        }

        return condicion is "* ADEUDA *" or "CURSANDO" or "RECURSANDO"
            || (materia.Nota ?? 0m) == 0m;
    }

    private static bool EsTodoElCuatrimestre(int contadorPendientes, int cuatrimestre) =>
        (contadorPendientes == TodasPrimerCuatrimestre && cuatrimestre == 1)
        || (contadorPendientes == TodasOtrosCuatrimestres && cuatrimestre != 1);

    // Equivalente a Copy(s, 1, Length(s) - 2): quita los dos últimos caracteres
    // (el separador ", " o "; " que el legacy deja colgando).
    private static string QuitarSeparadorFinal(string texto) =>
        texto.Length >= 2 ? texto[..^2] : texto;
}
