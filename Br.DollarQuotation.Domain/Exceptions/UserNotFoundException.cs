
namespace Br.DollarQuotation.Domain.Exceptions;

public sealed class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid id) : base($"O usuário com o identificador '{id}' não foi encontrado.")
    {

    }

    public UserNotFoundException(string email): base($"O usuário com o e-mail '{email}' não foi encontrado.")
    {

    }
}