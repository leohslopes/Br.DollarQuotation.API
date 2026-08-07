namespace Br.DollarQuotation.Domain.Exceptions;

public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException() : base("E-mail ou senha inválidos.")
    {

    }
}