using Br.DollarQuotation.API.Filters;
using Br.DollarQuotation.API.Services.Interfaces;
using Br.DollarQuotation.Application.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Br.DollarQuotation.API.Controllers;

[ApiController]
[Route("api/internal/quotation-notifications")]
[InternalApiKey]
public sealed class InternalQuotationNotificationsController : ControllerBase
{
    private readonly IQuotationNotificationService
        _notificationService;

    public InternalQuotationNotificationsController(IQuotationNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Notify([FromBody] CurrencyQuotationResponse quotation, CancellationToken cancellationToken)
    {
        await _notificationService.NotifyQuotationUpdatedAsync(quotation, cancellationToken);

        return NoContent();
    }
}