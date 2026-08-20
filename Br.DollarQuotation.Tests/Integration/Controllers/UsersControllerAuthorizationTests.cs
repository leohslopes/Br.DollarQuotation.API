using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.ValueObjects;
using Br.DollarQuotation.Repository.Configurations;
using Br.DollarQuotation.Repository.Services;
using Br.DollarQuotation.Tests.Integration.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;

namespace Br.DollarQuotation.Tests.Integration.Controllers;

public sealed class UsersControllerAuthorizationTests
    : IClassFixture<DollarQuotationWebApplicationFactory>
{
    private readonly DollarQuotationWebApplicationFactory _factory;

    public UsersControllerAuthorizationTests(
        DollarQuotationWebApplicationFactory factory)
    {
        _factory = factory;

        _factory
            .UserServiceMock
            .Reset();
    }

    // =========================================================
    // SEM TOKEN
    // =========================================================

    [Fact]
    public async Task GetAll_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        using var client =
            _factory.CreateClient();

        // Act
        var response =
            await client.GetAsync(
                "/api/users");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.GetPagedAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    // =========================================================
    // USER -> ENDPOINT ADMIN
    // =========================================================

    [Fact]
    public async Task GetAll_WithUserRole_ShouldReturnForbidden()
    {
        // Arrange
        using var client =
            CreateAuthenticatedClient(
                UserRole.User);

        // Act
        var response =
            await client.GetAsync(
                "/api/users");

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.GetPagedAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    // =========================================================
    // ADMIN -> ENDPOINT ADMIN
    // =========================================================

    [Fact]
    public async Task GetAll_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        var userResponse =
            CreateUserResponse();

        var pagedResponse =
            new PagedResponse<UserResponse>
            {
                Items =
                [
                    userResponse
                ],

                Page = 1,
                PageSize = 10,
                TotalItems = 1,
                TotalPages = 1
            };

        _factory
            .UserServiceMock
            .Setup(
                service =>
                    service.GetPagedAsync(
                        1,
                        10,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                pagedResponse);

        using var client =
            CreateAuthenticatedClient(
                UserRole.Admin);

        // Act
        var response =
            await client.GetAsync(
                "/api/users?page=1&pageSize=10");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.GetPagedAsync(
                        1,
                        10,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    // =========================================================
    // USER -> PRÓPRIO PERFIL
    // =========================================================

    [Fact]
    public async Task GetById_WithUserAccessingOwnId_ShouldReturnOk()
    {
        // Arrange
        var user =
            CreateUser(
                UserRole.User);

        var userResponse =
            CreateUserResponse(
                user);

        _factory
            .UserServiceMock
            .Setup(
                service =>
                    service.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                userResponse);

        using var client =
            CreateAuthenticatedClient(
                user);

        // Act
        var response =
            await client.GetAsync(
                $"/api/users/{user.Id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    // =========================================================
    // USER -> OUTRO PERFIL
    // =========================================================

    [Fact]
    public async Task GetById_WithUserAccessingAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        var authenticatedUser =
            CreateUser(
                UserRole.User);

        var anotherUserId =
            Guid.NewGuid();

        using var client =
            CreateAuthenticatedClient(
                authenticatedUser);

        // Act
        var response =
            await client.GetAsync(
                $"/api/users/{anotherUserId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.GetByIdAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    // =========================================================
    // USER -> ATIVAR USUÁRIO
    // =========================================================

    [Fact]
    public async Task Activate_WithUserRole_ShouldReturnForbidden()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        using var client =
            CreateAuthenticatedClient(
                UserRole.User);

        // Act
        var response =
            await client.PatchAsync(
                $"/api/users/{userId}/activate",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.ActivateAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    // =========================================================
    // ADMIN -> ATIVAR USUÁRIO
    // =========================================================

    [Fact]
    public async Task Activate_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var userResponse =
            CreateUserResponse(
                userId);

        _factory
            .UserServiceMock
            .Setup(
                service =>
                    service.ActivateAsync(
                        userId,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                userResponse);

        using var client =
            CreateAuthenticatedClient(
                UserRole.Admin);

        // Act
        var response =
            await client.PatchAsync(
                $"/api/users/{userId}/activate",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.ActivateAsync(
                        userId,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    // =========================================================
    // USER -> DESATIVAR USUÁRIO
    // =========================================================

    [Fact]
    public async Task Deactivate_WithUserRole_ShouldReturnForbidden()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        using var client =
            CreateAuthenticatedClient(
                UserRole.User);

        // Act
        var response =
            await client.PatchAsync(
                $"/api/users/{userId}/deactivate",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.DeactivateAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    // =========================================================
    // ADMIN -> DESATIVAR OUTRO USUÁRIO
    // =========================================================

    [Fact]
    public async Task Deactivate_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        var admin =
            CreateUser(
                UserRole.Admin);

        var anotherUser =
            CreateUser(
                UserRole.User);

        var responseModel =
            CreateUserResponse(
                anotherUser);

        responseModel.IsActive =
            false;

        _factory
            .UserServiceMock
            .Setup(
                service =>
                    service.DeactivateAsync(
                        anotherUser.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                responseModel);

        using var client =
            CreateAuthenticatedClient(
                admin);

        // Act
        var response =
            await client.PatchAsync(
                $"/api/users/{anotherUser.Id}/deactivate",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.DeactivateAsync(
                        anotherUser.Id,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    // =========================================================
    // ADMIN -> DESATIVAR A SI MESMO
    // =========================================================

    [Fact]
    public async Task Deactivate_AdminTryingToDeactivateItself_ShouldReturnForbidden()
    {
        // Arrange
        var admin =
            CreateUser(
                UserRole.Admin);

        using var client =
            CreateAuthenticatedClient(
                admin);

        // Act
        var response =
            await client.PatchAsync(
                $"/api/users/{admin.Id}/deactivate",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.DeactivateAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    // =========================================================
    // ADMIN -> ATUALIZAR OUTRO USUÁRIO
    // =========================================================

    [Fact]
    public async Task Update_AdminUpdatingAnotherUser_ShouldReturnOk()
    {
        // Arrange
        var admin =
            CreateUser(
                UserRole.Admin);

        var anotherUser =
            CreateUser(
                UserRole.User);

        var request =
            new UpdateUserRequest
            {
                Name =
                    "Usuário Atualizado",

                Email =
                    anotherUser.Email.Value,

                Role =
                    UserRole.Admin.ToString()
            };

        var responseModel =
            CreateUserResponse(
                anotherUser);

        responseModel.Name =
            request.Name;

        responseModel.Role =
            request.Role;

        _factory
            .UserServiceMock
            .Setup(
                service =>
                    service.UpdateAsync(
                        anotherUser.Id,
                        It.IsAny<UpdateUserRequest>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                responseModel);

        using var client =
            CreateAuthenticatedClient(
                admin);

        // Act
        var response =
            await client.PutAsJsonAsync(
                $"/api/users/{anotherUser.Id}",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.UpdateAsync(
                        anotherUser.Id,
                        It.Is<UpdateUserRequest>(
                            updateRequest =>
                                updateRequest.Role ==
                                    UserRole.Admin.ToString()),
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    // =========================================================
    // ADMIN -> REBAIXAR A SI MESMO
    // =========================================================

    [Fact]
    public async Task Update_AdminTryingToDowngradeItself_ShouldReturnForbidden()
    {
        // Arrange
        var admin =
            CreateUser(
                UserRole.Admin);

        var currentAdminResponse =
            CreateUserResponse(
                admin);

        _factory
            .UserServiceMock
            .Setup(
                service =>
                    service.GetByIdAsync(
                        admin.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                currentAdminResponse);

        var request =
            new UpdateUserRequest
            {
                Name =
                    admin.Name,

                Email =
                    admin.Email.Value,

                Role =
                    UserRole.User.ToString()
            };

        using var client =
            CreateAuthenticatedClient(
                admin);

        // Act
        var response =
            await client.PutAsJsonAsync(
                $"/api/users/{admin.Id}",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.GetByIdAsync(
                        admin.Id,
                        It.IsAny<CancellationToken>()),
                Times.Once);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.UpdateAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<UpdateUserRequest>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    // =========================================================
    // ADMIN -> ATUALIZAR A SI MESMO MANTENDO ADMIN
    // =========================================================

    [Fact]
    public async Task Update_AdminUpdatingOwnDataAndKeepingRole_ShouldReturnOk()
    {
        // Arrange
        var admin =
            CreateUser(
                UserRole.Admin);

        var currentAdminResponse =
            CreateUserResponse(
                admin);

        var request =
            new UpdateUserRequest
            {
                Name =
                    "Leonardo Atualizado",

                Email =
                    admin.Email.Value,

                Role =
                    UserRole.Admin.ToString()
            };

        var updatedResponse =
            CreateUserResponse(
                admin);

        updatedResponse.Name =
            request.Name;

        _factory
            .UserServiceMock
            .Setup(
                service =>
                    service.GetByIdAsync(
                        admin.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                currentAdminResponse);

        _factory
            .UserServiceMock
            .Setup(
                service =>
                    service.UpdateAsync(
                        admin.Id,
                        It.IsAny<UpdateUserRequest>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                updatedResponse);

        using var client =
            CreateAuthenticatedClient(
                admin);

        // Act
        var response =
            await client.PutAsJsonAsync(
                $"/api/users/{admin.Id}",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        _factory
            .UserServiceMock
            .Verify(
                service =>
                    service.UpdateAsync(
                        admin.Id,
                        It.Is<UpdateUserRequest>(
                            updateRequest =>
                                updateRequest.Role ==
                                    UserRole.Admin.ToString()),
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    // =========================================================
    // HELPERS JWT
    // =========================================================

    private HttpClient CreateAuthenticatedClient(
        UserRole role)
    {
        var user =
            CreateUser(
                role);

        return CreateAuthenticatedClient(
            user);
    }

    private HttpClient CreateAuthenticatedClient(
        User user)
    {
        var client =
            _factory.CreateClient();

        var tokenService =
            CreateTokenService();

        var token =
            tokenService.GenerateAccessToken(
                user,
                DateTime.UtcNow.AddMinutes(
                    60));

        client
            .DefaultRequestHeaders
            .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

        return client;
    }

    private static JwtTokenService CreateTokenService()
    {
        var options =
            new JwtOptions
            {
                SecretKey =
                    "BrDollarQuotation@TestJwtKey#2026!123456789",

                Issuer =
                    "Br.DollarQuotation.Tests",

                Audience =
                    "Br.DollarQuotation.Tests",

                ExpirationInMinutes =
                    60
            };

        return new JwtTokenService(
            Options.Create(
                options));
    }

    // =========================================================
    // HELPERS USER
    // =========================================================

    private static User CreateUser(
        UserRole role)
    {
        return new User(
            "Leonardo",
            Email.Create(
                $"teste-{Guid.NewGuid():N}@email.com"),
            "hash-da-senha",
            role: role);
    }

    private static UserResponse CreateUserResponse()
    {
        return new UserResponse
        {
            Id =
                Guid.NewGuid(),

            Name =
                "Leonardo",

            Email =
                "teste@email.com",

            Role =
                UserRole.User.ToString(),

            IsActive =
                true,

            CreatedAt =
                DateTime.UtcNow
        };
    }

    private static UserResponse CreateUserResponse(
        Guid userId)
    {
        return new UserResponse
        {
            Id =
                userId,

            Name =
                "Leonardo",

            Email =
                "teste@email.com",

            Role =
                UserRole.User.ToString(),

            IsActive =
                true,

            CreatedAt =
                DateTime.UtcNow
        };
    }

    private static UserResponse CreateUserResponse(
        User user)
    {
        return new UserResponse
        {
            Id =
                user.Id,

            Name =
                user.Name,

            Email =
                user.Email.Value,

            Role =
                user.Role.ToString(),

            PhotoBase64 =
                user.PhotoBase64,

            PhotoContentType =
                user.PhotoContentType,

            IsActive =
                user.IsActive,

            CreatedAt =
                user.CreatedAt,

            UpdatedAt =
                user.UpdatedAt
        };
    }
}