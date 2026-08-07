using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

        Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);

        Task<UserResponse> UpdatePhotoAsync(Guid id, UpdateUserPhotoRequest request, CancellationToken cancellationToken = default);

        Task<UserResponse> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

        Task<UserResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

        Task<PagedResponse<UserResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
