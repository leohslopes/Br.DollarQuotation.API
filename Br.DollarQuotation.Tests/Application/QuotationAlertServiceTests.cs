using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.Services;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.ValueObjects;
using Moq;

namespace Br.DollarQuotation.Tests.Application;

public sealed class QuotationAlertServiceTests
{
    private readonly Mock<IQuotationAlertRepository> _quotationAlertRepositoryMock;

    public QuotationAlertServiceTests()
    {
        _quotationAlertRepositoryMock =
            new Mock<IQuotationAlertRepository>();
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateQuotationAlert()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request =
            CreateRequest();

        var service =
            CreateService();

        // Act
        var response =
            await service.CreateAsync(
                userId,
                request
            );

        // Assert
        Assert.NotNull(response);

        Assert.NotEqual(
            Guid.Empty,
            response.Id
        );

        Assert.Equal(
            userId,
            response.UserId
        );

        Assert.Equal(
            request.BaseCurrency,
            response.BaseCurrency
        );

        Assert.Equal(
            request.QuoteCurrency,
            response.QuoteCurrency
        );

        Assert.Equal(
            "USD-BRL",
            response.CurrencyPair
        );

        Assert.Equal(
            request.Condition,
            response.Condition
        );

        Assert.Equal(
            request.TargetPrice,
            response.TargetPrice
        );

        Assert.True(
            response.IsActive
        );

        Assert.Null(
            response.TriggeredAt
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.Is<QuotationAlert>(
                        alert =>
                            alert.UserId == userId &&
                            alert.CurrencyPair.ToCode() == "USD-BRL" &&
                            alert.Condition == request.Condition &&
                            alert.TargetPrice == request.TargetPrice &&
                            alert.IsActive
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_WithEmptyUserId_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRequest();

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.CreateAsync(
                    Guid.Empty,
                    request
                )
            );

        // Assert
        Assert.Equal(
            "O usuário autenticado é inválido.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<QuotationAlert>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var service =
            CreateService();

        // Act / Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.CreateAsync(
                userId,
                null!
            )
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<QuotationAlert>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTargetPrice_ShouldThrowDomainException()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var request =
            CreateRequest();

        request.TargetPrice = 0;

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.CreateAsync(
                    userId,
                    request
                )
            );

        // Assert
        Assert.Equal(
            "O valor alvo do alerta deve ser maior que zero.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<QuotationAlert>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_WithInvalidCondition_ShouldThrowDomainException()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var request =
            CreateRequest();

        request.Condition =
            (AlertCondition)999;

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.CreateAsync(
                    userId,
                    request
                )
            );

        // Assert
        Assert.Equal(
            "A condição do alerta é inválida.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<QuotationAlert>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion

    #region GetByUserAsync

    [Fact]
    public async Task GetByUserAsync_WithExistingAlerts_ShouldReturnUserAlerts()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var alerts =
            new List<QuotationAlert>
            {
                CreateAlert(
                    userId,
                    "USD",
                    "BRL",
                    AlertCondition.Below,
                    5.10m
                ),

                CreateAlert(
                    userId,
                    "EUR",
                    "BRL",
                    AlertCondition.Above,
                    6.00m
                )
            };

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetByUserIdAsync(
                        userId,
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                alerts
            );

        var service =
            CreateService();

        // Act
        var response =
            await service.GetByUserAsync(
                userId
            );

        // Assert
        Assert.NotNull(response);

        Assert.Equal(
            2,
            response.Count
        );

        Assert.Contains(
            response,
            alert =>
                alert.CurrencyPair == "USD-BRL" &&
                alert.TargetPrice == 5.10m
        );

        Assert.Contains(
            response,
            alert =>
                alert.CurrencyPair == "EUR-BRL" &&
                alert.TargetPrice == 6.00m
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.GetByUserIdAsync(
                    userId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetByUserAsync_WhenUserHasNoAlerts_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetByUserIdAsync(
                        userId,
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                Array.Empty<QuotationAlert>()
            );

        var service =
            CreateService();

        // Act
        var response =
            await service.GetByUserAsync(
                userId
            );

        // Assert
        Assert.NotNull(response);

        Assert.Empty(
            response
        );
    }

    [Fact]
    public async Task GetByUserAsync_WithEmptyUserId_ShouldThrowDomainException()
    {
        // Arrange
        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetByUserAsync(
                    Guid.Empty
                )
            );

        // Assert
        Assert.Equal(
            "O usuário autenticado é inválido.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.GetByUserIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion

    #region ActivateAsync

    [Fact]
    public async Task ActivateAsync_WithInactiveAlert_ShouldActivateAlert()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var alert =
            CreateAlert(
                userId
            );

        alert.Deactivate();

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        alert.Id,
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                alert
            );

        var service =
            CreateService();

        // Act
        var response =
            await service.ActivateAsync(
                userId,
                alert.Id
            );

        // Assert
        Assert.True(
            response.IsActive
        );

        Assert.Null(
            response.TriggeredAt
        );

        Assert.NotNull(
            response.UpdatedAt
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ActivateAsync_WithEmptyUserId_ShouldThrowDomainException()
    {
        // Arrange
        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.ActivateAsync(
                    Guid.Empty,
                    Guid.NewGuid()
                )
            );

        // Assert
        Assert.Equal(
            "O usuário autenticado é inválido.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ActivateAsync_WithEmptyAlertId_ShouldThrowDomainException()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.ActivateAsync(
                    userId,
                    Guid.Empty
                )
            );

        // Assert
        Assert.Equal(
            "O identificador do alerta é inválido.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ActivateAsync_WhenAlertDoesNotExist_ShouldThrowDomainException()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var alertId =
            Guid.NewGuid();

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        alertId,
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                (QuotationAlert?)null
            );

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.ActivateAsync(
                    userId,
                    alertId
                )
            );

        // Assert
        Assert.Equal(
            "Alerta de cotação não encontrado.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ActivateAsync_WhenAlertBelongsToAnotherUser_ShouldThrowDomainException()
    {
        // Arrange
        var authenticatedUserId =
            Guid.NewGuid();

        var alertOwnerId =
            Guid.NewGuid();

        var alert =
            CreateAlert(
                alertOwnerId
            );

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        alert.Id,
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                alert
            );

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.ActivateAsync(
                    authenticatedUserId,
                    alert.Id
                )
            );

        // Assert
        Assert.Equal(
            "Alerta de cotação não encontrado.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion

    #region DeactivateAsync

    [Fact]
    public async Task DeactivateAsync_WithActiveAlert_ShouldDeactivateAlert()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var alert =
            CreateAlert(
                userId
            );

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        alert.Id,
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                alert
            );

        var service =
            CreateService();

        // Act
        var response =
            await service.DeactivateAsync(
                userId,
                alert.Id
            );

        // Assert
        Assert.False(
            response.IsActive
        );

        Assert.NotNull(
            response.UpdatedAt
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task DeactivateAsync_WhenAlertDoesNotExist_ShouldThrowDomainException()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var alertId =
            Guid.NewGuid();

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        alertId,
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                (QuotationAlert?)null
            );

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.DeactivateAsync(
                    userId,
                    alertId
                )
            );

        // Assert
        Assert.Equal(
            "Alerta de cotação não encontrado.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task DeactivateAsync_WhenAlertBelongsToAnotherUser_ShouldThrowDomainException()
    {
        // Arrange
        var authenticatedUserId =
            Guid.NewGuid();

        var alertOwnerId =
            Guid.NewGuid();

        var alert =
            CreateAlert(
                alertOwnerId
            );

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        alert.Id,
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                alert
            );

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.DeactivateAsync(
                    authenticatedUserId,
                    alert.Id
                )
            );

        // Assert
        Assert.Equal(
            "Alerta de cotação não encontrado.",
            exception.Message
        );

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion

    #region Mapping

    [Fact]
    public async Task GetByUserAsync_ShouldMapQuotationAlertCorrectly()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var alert =
            CreateAlert(
                userId,
                "GBP",
                "BRL",
                AlertCondition.Above,
                7.25m
            );

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetByUserIdAsync(
                        userId,
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                new[]
                {
                    alert
                }
            );

        var service =
            CreateService();

        // Act
        var response =
            await service.GetByUserAsync(
                userId
            );

        var item =
            Assert.Single(
                response
            );

        // Assert
        Assert.Equal(
            alert.Id,
            item.Id
        );

        Assert.Equal(
            userId,
            item.UserId
        );

        Assert.Equal(
            "GBP",
            item.BaseCurrency
        );

        Assert.Equal(
            "BRL",
            item.QuoteCurrency
        );

        Assert.Equal(
            "GBP-BRL",
            item.CurrencyPair
        );

        Assert.Equal(
            AlertCondition.Above,
            item.Condition
        );

        Assert.Equal(
            7.25m,
            item.TargetPrice
        );

        Assert.True(
            item.IsActive
        );

        Assert.Equal(
            alert.CreatedAt,
            item.CreatedAt
        );
    }

    #endregion

    #region Helpers

    private QuotationAlertService CreateService()
    {
        return new QuotationAlertService(
            _quotationAlertRepositoryMock.Object
        );
    }

    private static CreateQuotationAlertRequest CreateRequest()
    {
        return new CreateQuotationAlertRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = "BRL",
            Condition = AlertCondition.Below,
            TargetPrice = 5.10m
        };
    }

    private static QuotationAlert CreateAlert(
        Guid userId,
        string baseCurrency = "USD",
        string quoteCurrency = "BRL",
        AlertCondition condition = AlertCondition.Below,
        decimal targetPrice = 5.10m)
    {
        return new QuotationAlert(
            userId,
            CurrencyPair.Create(
                baseCurrency,
                quoteCurrency
            ),
            condition,
            targetPrice
        );
    }

    #endregion
}