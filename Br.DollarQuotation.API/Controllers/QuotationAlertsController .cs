using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Br.DollarQuotation.API.Controllers;

[Authorize]
[ApiController]
[Route("api/quotation-alerts")]
public sealed class QuotationAlertsController : ControllerBase
{
    private readonly IQuotationAlertService _quotationAlertService;

    public QuotationAlertsController(
        IQuotationAlertService quotationAlertService)
    {
        _quotationAlertService = quotationAlertService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(QuotationAlertResponse),StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<QuotationAlertResponse>> Create([FromBody] CreateQuotationAlertRequest request, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        var response = await _quotationAlertService.CreateAsync(
            userId,
            request,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetAll),
            new
            {
                id = response.Id
            },
            response
        );
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<QuotationAlertResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<QuotationAlertResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        var response = await _quotationAlertService.GetByUserAsync(
            userId,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(QuotationAlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuotationAlertResponse>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        var response = await _quotationAlertService.ActivateAsync(
            userId,
            id,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(QuotationAlertResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuotationAlertResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        var response = await _quotationAlertService.DeactivateAsync(
            userId,
            id,
            cancellationToken
        );

        return Ok(response);
    }

    private Guid GetAuthenticatedUserId()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (
            string.IsNullOrWhiteSpace(userIdValue) ||
            !Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Não foi possível identificar o usuário autenticado."
            );
        }

        return userId;
    }
}