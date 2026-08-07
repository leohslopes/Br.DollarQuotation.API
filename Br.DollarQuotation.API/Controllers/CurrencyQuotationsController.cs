using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Br.DollarQuotation.API.Controllers;

[Authorize]
[ApiController]
[Route("api/currency-quotations")]
public sealed class CurrencyQuotationsController : ControllerBase
{
    private readonly ICurrencyQuotationService
        _currencyQuotationService;

    public CurrencyQuotationsController(ICurrencyQuotationService currencyQuotationService)
    {
        _currencyQuotationService = currencyQuotationService;
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(CurrencyQuotationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CurrencyQuotationResponse>> GetCurrent([FromQuery] GetCurrentQuotationRequest request, CancellationToken cancellationToken)
    {
        var response = await _currencyQuotationService.GetCurrentAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyCollection<CurrencyQuotationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyCollection<CurrencyQuotationResponse>>> GetHistory([FromQuery] GetQuotationHistoryRequest request, CancellationToken cancellationToken)
    {
        var response = await _currencyQuotationService.GetHistoryAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(CurrencyQuotationSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrencyQuotationSummaryResponse>> GetSummaryAsync([FromQuery] GetQuotationSummaryRequest request, CancellationToken cancellationToken)
    {
        var response = await _currencyQuotationService.GetSummaryAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("paged")]
    [ProducesResponseType(
    typeof(PagedResponse<CurrencyQuotationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<CurrencyQuotationResponse>>> GetAll([FromQuery] GetQuotationPagedRequest request, CancellationToken cancellationToken)
    {
        var response = await _currencyQuotationService.GetPagedAsync(request, cancellationToken);

        return Ok(response);
    }
}