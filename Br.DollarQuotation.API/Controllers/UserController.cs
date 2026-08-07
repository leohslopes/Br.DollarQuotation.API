using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Br.DollarQuotation.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var response = await _userService.RegisterAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _userService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Update(Guid id,[FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var response = await _userService.UpdateAsync(id, request, cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/photo")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdatePhoto(Guid id, [FromBody] UpdateUserPhotoRequest request,CancellationToken cancellationToken)
    {
        var response = await _userService.UpdatePhotoAsync(id, request, cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(UserResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var response = await _userService.ActivateAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType( typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var response = await _userService.DeactivateAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetAll([FromQuery] int page = 1,[FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var response = await _userService.GetPagedAsync(page, pageSize, cancellationToken);

        return Ok(response);
    }
}