using ClosedXML.Excel;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Ministerio;

namespace Esba.Infrastructure.Excel;

/// <summary>
/// Exportación a Excel del padrón de comisiones al Ministerio con ClosedXML. Sucesora
/// del BitBtn1Click de ComisionesAlMinisterio.pas, que volcaba el dataset por OLE con
/// Exportar_Excel_DS ("Juntas": una hoja) o Exportar_Excel_DS_Hojas ("Separadas": una
/// hoja por CUTUCO nombrada CARRE-CUTUCO). Los títulos de columna son los que el
/// legacy enviaba (son el formato que recibe el Ministerio) y el layout depende del
/// tipo de carrera: terciario (24 columnas, con condición y título de ingreso) o
/// secundario (26 columnas, con PPI y adulto responsable).
/// </summary>
public sealed class PadronMinisterioExcelService : IPadronMinisterioExcelService
{
    private const string NombreHojaJuntas = "Comisiones";
    private const double AnchoMinimo = 12;
    private const double AnchoMaximo = 45;

    private sealed record Columna(string Titulo, Func<FilaPadronMinisterioDto, object?> Valor);

    private static readonly Columna[] ColumnasTerciaria =
    [
        new("NÚMERO DE RESOLUCION DEL PLAN", f => f.Resolucion),
        new("ORIENTACION DE LA CARRERA (TS=TECNICO SUPERIOR)", f => f.Orientacion),
        new("TIPO DE CARRERA (G=GRADO)", f => f.TipoCarrera),
        new("MODALIDAD (P=PRESENCIAL, D=DISTANCIA)", f => f.Modalidad),
        new("TURNO (M=MAÑANA, N=NOCHE, NC=NO CORRESPONDE)", f => f.Turno),
        new("AÑO DE ESTUDIO (1,2,3)", f => f.AnioEstudio),
        new("CUATRIMESTRE (1,2,3,4,5,6)", f => f.Cuatrimestre),
        new("DIVISION (A,B,C)", f => f.Division),
        new("APELLIDO", f => f.Apellido),
        new("NOMBRES", f => f.Nombre),
        new("TIPO DE DOCUMENTO (DNI, PASS,ETC)", f => f.TipoDocumento),
        new("NRO. DOCUMENTO", f => f.NumeroDocumento),
        new("GENERO (VARON/MUJER)", f => f.Genero),
        new("FECHA DE NACIMIENTO (DD/MM/AA)", f => f.FechaNacimiento),
        new("PAIS DE NACIMIENTO", f => f.PaisNacimiento),
        new("JURISDICCION/PROVINCIA DE NACIMIENTO", f => f.LugarNacimiento),
        new("DOMICILIO", f => f.Domicilio),
        new("PISO Y DEPTO.", _ => null),
        new("CODIGO POSTAL", f => f.CodigoPostal),
        new("BARRIO/LOCALIDAD", f => f.Localidad),
        new("COMUNA/PARTIDO", _ => null),
        new("BARRIO/JURISDICCION", _ => null),
        new("CONDICION (R=REGULAR, C=CONDICIONAL, RC=RECURSANTE)", f => f.Condicion),
        new("TITULO CON EL QUE INGRESA", f => f.TituloIngreso),
    ];

    private static readonly Columna[] ColumnasSecundaria =
    [
        new("NÚMERO DE RESOLUCION DEL PLAN", f => f.Resolucion),
        new("MODALIDAD (C=COMÚN, A=ADULTOS)", f => f.Modalidad),
        new("TURNO (M=MAÑANA, T=TARDE, N=NOCHE)", f => f.Turno),
        new("AÑO DE ESTUDIO (1,2,3,4)", f => f.AnioEstudio),
        new("DIVISION (A,B,C)", f => f.Division),
        new("APELLIDO", f => f.Apellido),
        new("NOMBRES", f => f.Nombre),
        new("TIPO DE DOCUMENTO (DNI, PASS,ETC)", f => f.TipoDocumento),
        new("NRO. DOCUMENTO", f => f.NumeroDocumento),
        new("GENERO (VARON/MUJER)", f => f.Genero),
        new("FECHA DE NACIMIENTO (DD/MM/AA)", f => f.FechaNacimiento),
        new("PAIS DE NACIMIENTO", f => f.PaisNacimiento),
        new("JURISDICCION/PROVINCIA DE NACIMIENTO", f => f.LugarNacimiento),
        new("CON PPI SIGNIFICATIVAS (X)", _ => null),
        new("CON PPI NO SIGNIFICATIVAS (X)", _ => null),
        new("SIN PPI (EN SEGUIMIENTO)", _ => null),
        new("DOMICILIO", f => f.Domicilio),
        new("PISO Y DEPTO.", _ => null),
        new("CODIGO POSTAL", f => f.CodigoPostal),
        new("BARRIO/LOCALIDAD", f => f.Localidad),
        new("COMUNA/PARTIDO", _ => null),
        new("JURISDICCION/PROVINCIA", _ => null),
        new("APELLIDO ADULTO RESPONSABLE", f => f.ApellidoTutor),
        new("NOMBRE ADULTO RESPONSABLE", f => f.NombreTutor),
        new("TIPO DOCUMENTO DEL ADULTO RESPONSABLE", f => f.TipoDocumentoTutor),
        new("NRO. DOCUMENTO DEL ADULTO RESPONSABLE", f => f.DniTutor),
    ];

    public byte[] GenerarPadron(PadronMinisterioModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var columnas = model.EsTerciaria ? ColumnasTerciaria : ColumnasSecundaria;

        using var libro = new XLWorkbook();

        if (model.HojasSeparadas)
        {
            // Una hoja por comisión, en el orden en que vienen las filas (CUTUCO ascendente).
            foreach (var grupo in model.Filas.GroupBy(f => f.Cutuco))
            {
                var nombre = grupo.First().NombreHoja;
                EscribirHoja(libro.AddWorksheet(NombreHojaValido(nombre)), columnas, grupo.ToList());
            }
        }
        else
        {
            EscribirHoja(libro.AddWorksheet(NombreHojaJuntas), columnas, model.Filas);
        }

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    private static void EscribirHoja(IXLWorksheet hoja, Columna[] columnas, IReadOnlyList<FilaPadronMinisterioDto> filas)
    {
        for (var c = 0; c < columnas.Length; c++)
        {
            var celda = hoja.Cell(1, c + 1);
            celda.Value = columnas[c].Titulo;
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Alignment.WrapText = true;
            celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        for (var f = 0; f < filas.Count; f++)
        {
            for (var c = 0; c < columnas.Length; c++)
            {
                Escribir(hoja.Cell(f + 2, c + 1), columnas[c].Valor(filas[f]));
            }
        }

        hoja.SheetView.FreezeRows(1);
        hoja.Row(1).Height = 45;

        // Ancho por el contenido de los datos (no por los títulos largos, que van
        // envueltos), acotado para que la planilla siga siendo legible.
        var ultimaFila = Math.Max(2, filas.Count + 1);
        hoja.Columns(1, columnas.Length).AdjustToContents(2, ultimaFila);
        foreach (var columna in hoja.Columns(1, columnas.Length))
        {
            columna.Width = Math.Clamp(columna.Width, AnchoMinimo, AnchoMaximo);
        }
    }

    private static void Escribir(IXLCell celda, object? valor)
    {
        switch (valor)
        {
            case null:
                break;
            case string texto:
                celda.Value = texto;
                break;
            case int entero:
                celda.Value = entero;
                break;
            case DateOnly fecha:
                celda.Value = fecha.ToDateTime(TimeOnly.MinValue);
                celda.Style.DateFormat.Format = "dd/MM/yyyy";
                break;
            default:
                celda.Value = valor.ToString();
                break;
        }
    }

    /// <summary>Excel limita el nombre de hoja a 31 chars y prohíbe : \ / ? * [ ].</summary>
    private static string NombreHojaValido(string titulo)
    {
        var limpio = new string((titulo ?? string.Empty)
            .Where(ch => !"\\/?*[]:".Contains(ch, StringComparison.Ordinal)).ToArray()).Trim();
        return limpio.Length switch
        {
            0 => NombreHojaJuntas,
            > 31 => limpio[..31],
            _ => limpio,
        };
    }
}
