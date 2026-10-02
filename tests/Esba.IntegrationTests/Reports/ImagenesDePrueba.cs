namespace Esba.IntegrationTests.Reports;

/// <summary>
/// Archivos de imagen mínimos (PNG 1×1) en un directorio temporal, para ejercer las ramas
/// de los reportes que incrustan membrete, sello y firmas sin depender de los JPG reales
/// de wwwroot/plantillas. Se borran al disponer.
/// </summary>
internal sealed class ImagenesDePrueba : IDisposable
{
    private static readonly byte[] PngUnPixel = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    private readonly string _directorio;

    private ImagenesDePrueba(string directorio)
    {
        _directorio = directorio;
        Membrete = Escribir("membrete.png");
        Sello = Escribir("sello.png");
        Firma = Escribir("firma.png");
    }

    public string Membrete { get; }

    public string Sello { get; }

    public string Firma { get; }

    public static ImagenesDePrueba Crear() =>
        new(Directory.CreateTempSubdirectory("esba-img-").FullName);

    private string Escribir(string nombre)
    {
        var ruta = Path.Combine(_directorio, nombre);
        File.WriteAllBytes(ruta, PngUnPixel);
        return ruta;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_directorio, recursive: true);
        }
        catch (IOException)
        {
            // Directorio temporal: si no se pudo borrar, no afecta al test.
        }
    }
}
