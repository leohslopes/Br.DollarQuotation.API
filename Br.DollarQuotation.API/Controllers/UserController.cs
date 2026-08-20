using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Br.DollarQuotation.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(
        IUserService userService)
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

        return CreatedAtAction(nameof(GetById), new { id = response.Id}, response);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        var response = await _userService.GetByIdAsync(userId, cancellationToken);

        return Ok(response);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> UpdateMyProfile([FromBody] UpdateMyProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var currentUser = await _userService.GetByIdAsync(userId, cancellationToken);
        var updateRequest = new UpdateUserRequest
            {
                Name = request.Name,
                Email = request.Email,
                Role = currentUser.Role
            };

        var response = await _userService.UpdateAsync(userId, updateRequest, cancellationToken);

        return Ok(response);
    }

    [HttpPatch("me/photo")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateMyPhoto([FromBody] UpdateUserPhotoRequest request, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        var response = await _userService.UpdatePhotoAsync(userId, request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(id))
        {
            return Forbid();
        }

        var response = await _userService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }


    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(id))
        {
            return Forbid();
        }

        if (User.IsInRole("Admin") && IsAuthenticatedUser(id))
        {
            var currentUser = await _userService.GetByIdAsync(id, cancellationToken);

            if (!string.Equals(request.Role, currentUser.Role, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
        }

        if (!User.IsInRole("Admin"))
        {
            var currentUser = await _userService.GetByIdAsync(id, cancellationToken);

            request.Role = currentUser.Role;
        }

        var response = await _userService.UpdateAsync(id, request, cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/photo")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdatePhoto(Guid id, [FromBody] UpdateUserPhotoRequest request, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(id))
        {
            return Forbid();
        }

        var response = await _userService.UpdatePhotoAsync(id, request, cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var response = await _userService.ActivateAsync(id, cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        if (IsAuthenticatedUser(id))
        {
            return Forbid();
        }

        var response = await _userService.DeactivateAsync(id, cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var response = await _userService.GetPagedAsync(page, pageSize, cancellationToken);

        return Ok(response);
    }

    private bool CanAccessUser(Guid userId)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        return IsAuthenticatedUser(userId);
    }

    private bool IsAuthenticatedUser(Guid userId)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return
            Guid.TryParse(authenticatedUserId, out var currentUserId) && currentUserId == userId;
    }

    private Guid GetAuthenticatedUserId()
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(authenticatedUserId, out var userId))
        {
            throw new UnauthorizedAccessException("Não foi possível identificar o usuário autenticado.");
        }

        return userId;
    }
}