using Esba.Domain.Common;

namespace Esba.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void DesdeErrCod_CodigoDos_DevuelveError()
    {
        var resultado = Result.DesdeErrCod(2, "El libro matriz pertenece a otro alumno", 99);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("El libro matriz pertenece a otro alumno", resultado.Message);
        Assert.False(resultado.IsSuccess);
    }

    [Fact]
    public void DesdeErrCod_CodigoUno_DevuelveNeedsConfirmation()
    {
        var resultado = Result.DesdeErrCod(1, "¿Confirma la operación?", 99);

        Assert.Equal(OperationStatus.NeedsConfirmation, resultado.Status);
        Assert.False(resultado.IsSuccess);
    }

    [Fact]
    public void DesdeErrCod_CodigoCeroConMensaje_DevuelveWarningYContinua()
    {
        var resultado = Result.DesdeErrCod(0, "Aviso: el alumno adeuda documentación", 99);

        Assert.Equal(OperationStatus.Warning, resultado.Status);
        Assert.True(resultado.IsSuccess);
        Assert.Equal(99, resultado.Value);
    }

    [Fact]
    public void DesdeErrCod_CodigoCeroSinMensaje_DevuelveOk()
    {
        var resultado = Result.DesdeErrCod(0, null, 99);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.True(resultado.IsSuccess);
        Assert.Equal(99, resultado.Value);
    }

    [Fact]
    public void DesdeErrCod_SinCodigo_DevuelveOk()
    {
        var resultado = Result.DesdeErrCod(null, null, 99);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(99, resultado.Value);
    }
}
