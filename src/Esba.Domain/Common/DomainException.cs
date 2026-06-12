namespace Esba.Domain.Common;

/// <summary>
/// Violación de un invariante de dominio. Reservada para lo verdaderamente
/// excepcional: los errores esperables de negocio viajan como <see cref="Result{T}"/>.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
