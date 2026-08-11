using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;


namespace Br.DollarQuotation.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

        Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    }
}
