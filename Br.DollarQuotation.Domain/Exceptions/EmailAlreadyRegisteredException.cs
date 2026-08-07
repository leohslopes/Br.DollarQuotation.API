using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Domain.Exceptions
{
    public sealed class EmailAlreadyRegisteredException(string email) : DomainException($"O e-mail '{email}' já está cadastrado.")
    {

    }
}
