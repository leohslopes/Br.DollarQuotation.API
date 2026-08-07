namespace Br.DollarQuotation.Domain.Exceptions;

public sealed class InactiveUserException : DomainException
{
    public InactiveUserException() : base("O usuário está inativo e não pode acessar o sistema.")
    {
    }
}