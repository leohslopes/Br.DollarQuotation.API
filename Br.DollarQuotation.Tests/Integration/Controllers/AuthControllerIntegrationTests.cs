using System.Net;
using System.Net.Http.Json;
using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Tests.Integration.Infrastructure;
using Moq;

namespace Br.DollarQuotation.Tests.Integration.Controllers;

public sealed class AuthControllerIntegrationTests
    : IClassFixture<DollarQuotationWebApplicationFactory>
{
    private readonly DollarQuotationWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(
        DollarQuotationWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Password = "Senha@123"
        };

        var expectedResponse = new LoginResponse
        {
            UserId = Guid.NewGuid(),
            Name = "Leonardo",
            Email = "teste@email.com",
            AccessToken = "jwt-token-test",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _factory.AuthServiceMock
            .Setup(service =>
                service.LoginAsync(
                    It.Is<LoginRequest>(
                        login =>
                            login.Email == request.Email &&
                            login.Password == request.Password),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var content =
            await response.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(content);

        Assert.Equal(
            expectedResponse.UserId,
            content.UserId);

        Assert.Equal(
            "Leonardo",
            content.Name);

        Assert.Equal(
            "teste@email.com",
            content.Email);

        Assert.Equal(
            "jwt-token-test",
            content.AccessToken);

        _factory.AuthServiceMock.Verify(
            service =>
                service.LoginAsync(
                    It.IsAny<LoginRequest>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}