using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

        Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);

        Task AddAsync(User user, CancellationToken cancellationToken = default);

        Task UpdateAsync(User user,CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<User>> GetPagedAsync(int page,int pageSize, CancellationToken cancellationToken = default);

        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }
}
