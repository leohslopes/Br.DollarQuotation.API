using Br.DollarQuotation.API.Services;
using Br.DollarQuotation.API.Services.Interfaces;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Domain.ValueObjects;
using Br.DollarQuotation.Messaging.Configurations;
using Br.DollarQuotation.Messaging.Contracts;
using Br.DollarQuotation.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Br.DollarQuotation.Tests.API;

public sealed class QuotationUpdatedConsumerWorkerTests
{
    private const string QueueName =
        "dollarquotation.quotation.queue.test";

    private const string RoutingKey =
        "quotation.updated";

    private readonly Mock<IMessageConsumer>
        _messageConsumerMock;

    private readonly Mock<IQuotationAlertRepository>
        _quotationAlertRepositoryMock;

    private readonly Mock<IQuotationNotificationService>
        _notificationServiceMock;

    private readonly Mock<IUserRepository>
        _userRepositoryMock;

    private readonly Mock<IEmailService>
        _emailServiceMock;

    private readonly Mock<ILogger<QuotationUpdatedConsumerWorker>>
        _loggerMock;

    public QuotationUpdatedConsumerWorkerTests()
    {
        _messageConsumerMock =
            new Mock<IMessageConsumer>();

        _quotationAlertRepositoryMock =
            new Mock<IQuotationAlertRepository>();

        _notificationServiceMock =
            new Mock<IQuotationNotificationService>();

        _userRepositoryMock =
            new Mock<IUserRepository>();

        _emailServiceMock =
            new Mock<IEmailService>();

        _loggerMock =
            new Mock<ILogger<QuotationUpdatedConsumerWorker>>();
    }

    // =========================================================
    // ABOVE
    // =========================================================

    [Fact]
    public async Task ProcessMessageAsync_WithAboveAlertAndPriceAboveTarget_ShouldTriggerAlert()
    {
        // Arrange
        var user =
            CreateUser();

        var alert =
            CreateAlert(
                user.Id,
                AlertCondition.Above,
                5.00m);

        var message =
            CreateMessage(
                bidPrice: 5.10m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        "USD-BRL",
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        Assert.False(
            alert.IsActive);

        Assert.NotNull(
            alert.TriggeredAt);

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    alert,
                    message.BidPrice,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationUpdatedAsync(
                    message,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(
            service =>
                service.SendQuotationAlertTriggeredAsync(
                    user.Email.Value,
                    user.Name,
                    "USD-BRL",
                    5.10m,
                    5.00m,
                    "Acima ou igual ao preço-alvo",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WithAboveAlertAndPriceEqualTarget_ShouldTriggerAlert()
    {
        // Arrange
        var user =
            CreateUser();

        var alert =
            CreateAlert(
                user.Id,
                AlertCondition.Above,
                5.00m);

        var message =
            CreateMessage(
                bidPrice: 5.00m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        Assert.False(
            alert.IsActive);

        Assert.NotNull(
            alert.TriggeredAt);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    alert,
                    5.00m,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(
            service =>
                service.SendQuotationAlertTriggeredAsync(
                    user.Email.Value,
                    user.Name,
                    "USD-BRL",
                    5.00m,
                    5.00m,
                    "Acima ou igual ao preço-alvo",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WithAboveAlertAndPriceBelowTarget_ShouldNotTriggerAlert()
    {
        // Arrange
        var alert =
            CreateAlert(
                Guid.NewGuid(),
                AlertCondition.Above,
                5.00m);

        var message =
            CreateMessage(
                bidPrice: 4.90m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        Assert.True(
            alert.IsActive);

        Assert.Null(
            alert.TriggeredAt);

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    It.IsAny<QuotationAlert>(),
                    It.IsAny<decimal>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _emailServiceMock.Verify(
            service =>
                service.SendQuotationAlertTriggeredAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationUpdatedAsync(
                    message,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // BELOW
    // =========================================================

    [Fact]
    public async Task ProcessMessageAsync_WithBelowAlertAndPriceBelowTarget_ShouldTriggerAlert()
    {
        // Arrange
        var user =
            CreateUser();

        var alert =
            CreateAlert(
                user.Id,
                AlertCondition.Below,
                5.00m);

        var message =
            CreateMessage(
                bidPrice: 4.90m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        "USD-BRL",
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        Assert.False(
            alert.IsActive);

        Assert.NotNull(
            alert.TriggeredAt);

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    alert,
                    message.BidPrice,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(
            service =>
                service.SendQuotationAlertTriggeredAsync(
                    user.Email.Value,
                    user.Name,
                    "USD-BRL",
                    4.90m,
                    5.00m,
                    "Abaixo ou igual ao preço-alvo",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WithBelowAlertAndPriceEqualTarget_ShouldTriggerAlert()
    {
        // Arrange
        var user =
            CreateUser();

        var alert =
            CreateAlert(
                user.Id,
                AlertCondition.Below,
                5.00m);

        var message =
            CreateMessage(
                bidPrice: 5.00m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        Assert.False(
            alert.IsActive);

        Assert.NotNull(
            alert.TriggeredAt);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    alert,
                    5.00m,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(
            service =>
                service.SendQuotationAlertTriggeredAsync(
                    user.Email.Value,
                    user.Name,
                    "USD-BRL",
                    5.00m,
                    5.00m,
                    "Abaixo ou igual ao preço-alvo",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WithBelowAlertAndPriceAboveTarget_ShouldNotTriggerAlert()
    {
        // Arrange
        var alert =
            CreateAlert(
                Guid.NewGuid(),
                AlertCondition.Below,
                5.00m);

        var message =
            CreateMessage(
                bidPrice: 5.10m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        Assert.True(
            alert.IsActive);

        Assert.Null(
            alert.TriggeredAt);

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    It.IsAny<QuotationAlert>(),
                    It.IsAny<decimal>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _emailServiceMock.Verify(
            service =>
                service.SendQuotationAlertTriggeredAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // NENHUM ALERTA
    // =========================================================

    [Fact]
    public async Task ProcessMessageAsync_WithNoActiveAlerts_ShouldOnlyNotifyQuotationUpdate()
    {
        // Arrange
        var message =
            CreateMessage(
                bidPrice: 5.10m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        "USD-BRL",
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<QuotationAlert>());

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    It.IsAny<QuotationAlert>(),
                    It.IsAny<decimal>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _emailServiceMock.Verify(
            service =>
                service.SendQuotationAlertTriggeredAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationUpdatedAsync(
                    message,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // MÚLTIPLOS ALERTAS
    // =========================================================

    [Fact]
    public async Task ProcessMessageAsync_WithMultipleAlerts_ShouldTriggerOnlyEligibleAlerts()
    {
        // Arrange
        var user =
            CreateUser();

        var aboveTriggered =
            CreateAlert(
                user.Id,
                AlertCondition.Above,
                5.00m);

        var belowNotTriggered =
            CreateAlert(
                Guid.NewGuid(),
                AlertCondition.Below,
                4.50m);

        var aboveNotTriggered =
            CreateAlert(
                Guid.NewGuid(),
                AlertCondition.Above,
                5.50m);

        var message =
            CreateMessage(
                bidPrice: 5.10m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        "USD-BRL",
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    aboveTriggered,
                    belowNotTriggered,
                    aboveNotTriggered
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        Assert.False(
            aboveTriggered.IsActive);

        Assert.NotNull(
            aboveTriggered.TriggeredAt);

        Assert.True(
            belowNotTriggered.IsActive);

        Assert.Null(
            belowNotTriggered.TriggeredAt);

        Assert.True(
            aboveNotTriggered.IsActive);

        Assert.Null(
            aboveNotTriggered.TriggeredAt);

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    aboveTriggered,
                    message.BidPrice,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    belowNotTriggered,
                    It.IsAny<decimal>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    aboveNotTriggered,
                    It.IsAny<decimal>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _emailServiceMock.Verify(
            service =>
                service.SendQuotationAlertTriggeredAsync(
                    user.Email.Value,
                    user.Name,
                    "USD-BRL",
                    message.BidPrice,
                    aboveTriggered.TargetPrice,
                    "Acima ou igual ao preço-alvo",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WithMultipleTriggeredAlerts_ShouldSaveOnlyOnce()
    {
        // Arrange
        var firstUser =
            CreateUser();

        var secondUser =
            CreateUser(
                "Maria",
                "maria@email.com");

        var firstAlert =
            CreateAlert(
                firstUser.Id,
                AlertCondition.Above,
                5.00m);

        var secondAlert =
            CreateAlert(
                secondUser.Id,
                AlertCondition.Below,
                5.20m);

        var message =
            CreateMessage(
                bidPrice: 5.10m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    firstAlert,
                    secondAlert
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        firstUser.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                firstUser);

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        secondUser.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                secondUser);

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        Assert.False(
            firstAlert.IsActive);

        Assert.False(
            secondAlert.IsActive);

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationAlertTriggeredAsync(
                    It.IsAny<QuotationAlert>(),
                    message.BidPrice,
                    It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _emailServiceMock.Verify(
            service =>
                service.SendQuotationAlertTriggeredAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    "USD-BRL",
                    message.BidPrice,
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    // =========================================================
    // CURRENCY PAIR
    // =========================================================

    [Fact]
    public async Task ProcessMessageAsync_ShouldQueryAlertsUsingBaseAndQuoteCurrencies()
    {
        // Arrange
        var message =
            CreateMessage(
                baseCurrency: "EUR",
                quoteCurrency: "BRL",
                bidPrice: 6.00m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        "EUR-BRL",
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<QuotationAlert>());

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.GetActiveByCurrencyPairAsync(
                    "EUR-BRL",
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // NOTIFICAÇÃO DA COTAÇÃO
    // =========================================================

    [Fact]
    public async Task ProcessMessageAsync_ShouldAlwaysNotifyQuotationUpdatedAfterProcessingAlerts()
    {
        // Arrange
        var message =
            CreateMessage();

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<QuotationAlert>());

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        _notificationServiceMock.Verify(
            service =>
                service.NotifyQuotationUpdatedAsync(
                    message,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // EMAIL DE ALERTA
    // =========================================================

    [Fact]
    public async Task ProcessMessageAsync_WithTriggeredAlert_ShouldSendEmailToUser()
    {
        // Arrange
        var user =
            CreateUser();

        var alert =
            CreateAlert(
                user.Id,
                AlertCondition.Above,
                5.00m);

        var message =
            CreateMessage(
                bidPrice: 5.10m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        "USD-BRL",
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            emailService =>
                emailService.SendQuotationAlertTriggeredAsync(
                    user.Email.Value,
                    user.Name,
                    "USD-BRL",
                    5.10m,
                    5.00m,
                    "Acima ou igual ao preço-alvo",
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            notificationService =>
                notificationService
                    .NotifyQuotationAlertTriggeredAsync(
                        alert,
                        5.10m,
                        It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WhenAlertUserDoesNotExist_ShouldNotSendEmail()
    {
        // Arrange
        var alert =
            CreateAlert(
                Guid.NewGuid(),
                AlertCondition.Above,
                5.00m);

        var message =
            CreateMessage(
                bidPrice: 5.10m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        "USD-BRL",
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        alert.UserId,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (User?)null);

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            emailService =>
                emailService.SendQuotationAlertTriggeredAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationServiceMock.Verify(
            notificationService =>
                notificationService
                    .NotifyQuotationAlertTriggeredAsync(
                        alert,
                        message.BidPrice,
                        It.IsAny<CancellationToken>()),
            Times.Once);

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WhenAlertUserIsInactive_ShouldNotSendEmail()
    {
        // Arrange
        var user =
            CreateUser();

        user.Deactivate();

        var alert =
            CreateAlert(
                user.Id,
                AlertCondition.Below,
                5.20m);

        var message =
            CreateMessage(
                bidPrice: 5.10m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        "USD-BRL",
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        await handler(
            message,
            CancellationToken.None);

        // Assert
        Assert.False(
            alert.IsActive);

        Assert.NotNull(
            alert.TriggeredAt);

        _emailServiceMock.Verify(
            emailService =>
                emailService.SendQuotationAlertTriggeredAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _notificationServiceMock.Verify(
            notificationService =>
                notificationService
                    .NotifyQuotationAlertTriggeredAsync(
                        alert,
                        message.BidPrice,
                        It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WhenEmailServiceThrows_ShouldContinueQuotationProcessing()
    {
        // Arrange
        var user =
            CreateUser();

        var alert =
            CreateAlert(
                user.Id,
                AlertCondition.Above,
                5.00m);

        var message =
            CreateMessage(
                bidPrice: 5.10m);

        _quotationAlertRepositoryMock
            .Setup(
                repository =>
                    repository.GetActiveByCurrencyPairAsync(
                        "USD-BRL",
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<QuotationAlert>
                {
                    alert
                });

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _emailServiceMock
            .Setup(
                emailService =>
                    emailService.SendQuotationAlertTriggeredAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<decimal>(),
                        It.IsAny<decimal>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "SMTP indisponível."));

        var handler =
            await StartWorkerAndCaptureHandlerAsync();

        // Act
        var exception =
            await Record.ExceptionAsync(
                () =>
                    handler(
                        message,
                        CancellationToken.None));

        // Assert
        Assert.Null(
            exception);

        Assert.False(
            alert.IsActive);

        Assert.NotNull(
            alert.TriggeredAt);

        _quotationAlertRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            notificationService =>
                notificationService
                    .NotifyQuotationAlertTriggeredAsync(
                        alert,
                        message.BidPrice,
                        It.IsAny<CancellationToken>()),
            Times.Once);

        _notificationServiceMock.Verify(
            notificationService =>
                notificationService
                    .NotifyQuotationUpdatedAsync(
                        message,
                        It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // CONSUMER / RESILIÊNCIA
    // =========================================================

    [Fact]
    public async Task StartAsync_ShouldRegisterConsumerWithConfiguredQueueAndRoutingKey()
    {
        // Arrange
        Func<
            QuotationUpdatedMessage,
            CancellationToken,
            Task>? capturedHandler =
            null;

        var consumeStarted =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        _messageConsumerMock
            .Setup(
                consumer =>
                    consumer.ConsumeAsync(
                        QueueName,
                        RoutingKey,
                        It.IsAny<
                            Func<
                                QuotationUpdatedMessage,
                                CancellationToken,
                                Task>>(),
                        It.IsAny<CancellationToken>()))
            .Callback<
                string,
                string,
                Func<
                    QuotationUpdatedMessage,
                    CancellationToken,
                    Task>,
                CancellationToken>(
                (
                    _,
                    _,
                    handler,
                    _) =>
                {
                    capturedHandler =
                        handler;

                    consumeStarted
                        .TrySetResult(
                            true);
                })
            .Returns(
                async (
                    string _,
                    string _,
                    Func<
                        QuotationUpdatedMessage,
                        CancellationToken,
                        Task> _,
                    CancellationToken cancellationToken) =>
                {
                    await Task.Delay(
                        Timeout.InfiniteTimeSpan,
                        cancellationToken);
                });

        using var provider =
            CreateServiceProvider();

        var worker =
            CreateWorker(
                provider.GetRequiredService<
                    IServiceScopeFactory>());

        // Act
        await worker.StartAsync(
            cancellationTokenSource.Token);

        await consumeStarted.Task
            .WaitAsync(
                TimeSpan.FromSeconds(2));

        // Assert
        Assert.NotNull(
            capturedHandler);

        _messageConsumerMock.Verify(
            consumer =>
                consumer.ConsumeAsync(
                    QueueName,
                    RoutingKey,
                    It.IsAny<
                        Func<
                            QuotationUpdatedMessage,
                            CancellationToken,
                            Task>>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);

        cancellationTokenSource.Cancel();

        await worker.StopAsync(
            CancellationToken.None);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConsumerFails_ShouldRetryWithoutStoppingWorker()
    {
        // Arrange
        var attempts =
            0;

        var secondAttemptStarted =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        _messageConsumerMock
            .Setup(
                consumer =>
                    consumer.ConsumeAsync(
                        QueueName,
                        RoutingKey,
                        It.IsAny<
                            Func<
                                QuotationUpdatedMessage,
                                CancellationToken,
                                Task>>(),
                        It.IsAny<CancellationToken>()))
            .Returns(
                async (
                    string _,
                    string _,
                    Func<
                        QuotationUpdatedMessage,
                        CancellationToken,
                        Task> _,
                    CancellationToken cancellationToken) =>
                {
                    attempts++;

                    if (attempts == 1)
                    {
                        throw new InvalidOperationException(
                            "RabbitMQ indisponível.");
                    }

                    secondAttemptStarted
                        .TrySetResult(
                            true);

                    await Task.Delay(
                        Timeout.InfiniteTimeSpan,
                        cancellationToken);
                });

        using var provider =
            CreateServiceProvider();

        var worker =
            CreateWorker(
                provider.GetRequiredService<
                    IServiceScopeFactory>());

        // Act
        await worker.StartAsync(
            cancellationTokenSource.Token);

        await secondAttemptStarted.Task
            .WaitAsync(
                TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(
            attempts >= 2);

        _messageConsumerMock.Verify(
            consumer =>
                consumer.ConsumeAsync(
                    QueueName,
                    RoutingKey,
                    It.IsAny<
                        Func<
                            QuotationUpdatedMessage,
                            CancellationToken,
                            Task>>(),
                    It.IsAny<CancellationToken>()),
            Times.AtLeast(2));

        cancellationTokenSource.Cancel();

        await worker.StopAsync(
            CancellationToken.None);
    }

    [Fact]
    public async Task StopAsync_WhenConsumerIsRunning_ShouldStopGracefully()
    {
        // Arrange
        var consumeStarted =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        _messageConsumerMock
            .Setup(
                consumer =>
                    consumer.ConsumeAsync(
                        QueueName,
                        RoutingKey,
                        It.IsAny<
                            Func<
                                QuotationUpdatedMessage,
                                CancellationToken,
                                Task>>(),
                        It.IsAny<CancellationToken>()))
            .Returns(
                async (
                    string _,
                    string _,
                    Func<
                        QuotationUpdatedMessage,
                        CancellationToken,
                        Task> _,
                    CancellationToken cancellationToken) =>
                {
                    consumeStarted
                        .TrySetResult(
                            true);

                    await Task.Delay(
                        Timeout.InfiniteTimeSpan,
                        cancellationToken);
                });

        using var provider =
            CreateServiceProvider();

        var worker =
            CreateWorker(
                provider.GetRequiredService<
                    IServiceScopeFactory>());

        await worker.StartAsync(
            cancellationTokenSource.Token);

        await consumeStarted.Task
            .WaitAsync(
                TimeSpan.FromSeconds(2));

        // Act
        cancellationTokenSource.Cancel();

        var exception =
            await Record.ExceptionAsync(
                () =>
                    worker.StopAsync(
                        CancellationToken.None));

        // Assert
        Assert.Null(
            exception);
    }

    // =========================================================
    // CONFIGURAÇÃO INVÁLIDA
    // =========================================================

    [Fact]
    public async Task StartAsync_WithEmptyQueueName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options =
            CreateRabbitMqOptions();

        options.QuotationQueueName =
            string.Empty;

        using var provider =
            CreateServiceProvider();

        var worker =
            CreateWorker(
                provider.GetRequiredService<
                    IServiceScopeFactory>(),
                options);

        // Act
        var exception =
            await Assert.ThrowsAsync<
                InvalidOperationException>(
                async () =>
                    await worker.StartAsync(
                        CancellationToken.None));

        // Assert
        Assert.Equal(
            "A fila de cotações do RabbitMQ não foi configurada.",
            exception.Message);

        _messageConsumerMock.Verify(
            consumer =>
                consumer.ConsumeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<
                        Func<
                            QuotationUpdatedMessage,
                            CancellationToken,
                            Task>>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StartAsync_WithEmptyRoutingKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options =
            CreateRabbitMqOptions();

        options.QuotationRoutingKey =
            string.Empty;

        using var provider =
            CreateServiceProvider();

        var worker =
            CreateWorker(
                provider.GetRequiredService<
                    IServiceScopeFactory>(),
                options);

        // Act
        var exception =
            await Assert.ThrowsAsync<
                InvalidOperationException>(
                async () =>
                    await worker.StartAsync(
                        CancellationToken.None));

        // Assert
        Assert.Equal(
            "A RoutingKey de cotações do RabbitMQ não foi configurada.",
            exception.Message);

        _messageConsumerMock.Verify(
            consumer =>
                consumer.ConsumeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<
                        Func<
                            QuotationUpdatedMessage,
                            CancellationToken,
                            Task>>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private async Task<
        Func<
            QuotationUpdatedMessage,
            CancellationToken,
            Task>>
        StartWorkerAndCaptureHandlerAsync()
    {
        Func<
            QuotationUpdatedMessage,
            CancellationToken,
            Task>? capturedHandler =
            null;

        var consumeStarted =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        _messageConsumerMock
            .Setup(
                consumer =>
                    consumer.ConsumeAsync(
                        QueueName,
                        RoutingKey,
                        It.IsAny<
                            Func<
                                QuotationUpdatedMessage,
                                CancellationToken,
                                Task>>(),
                        It.IsAny<CancellationToken>()))
            .Callback<
                string,
                string,
                Func<
                    QuotationUpdatedMessage,
                    CancellationToken,
                    Task>,
                CancellationToken>(
                (
                    _,
                    _,
                    handler,
                    _) =>
                {
                    capturedHandler =
                        handler;

                    consumeStarted
                        .TrySetResult(
                            true);
                })
            .Returns(
                async (
                    string _,
                    string _,
                    Func<
                        QuotationUpdatedMessage,
                        CancellationToken,
                        Task> _,
                    CancellationToken cancellationToken) =>
                {
                    await Task.Delay(
                        Timeout.InfiniteTimeSpan,
                        cancellationToken);
                });

        var provider =
            CreateServiceProvider();

        var worker =
            CreateWorker(
                provider.GetRequiredService<
                    IServiceScopeFactory>());

        await worker.StartAsync(
            cancellationTokenSource.Token);

        await consumeStarted.Task
            .WaitAsync(
                TimeSpan.FromSeconds(2));

        Assert.NotNull(
            capturedHandler);

        cancellationTokenSource.Cancel();

        await worker.StopAsync(
            CancellationToken.None);

        return capturedHandler;
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services =
            new ServiceCollection();

        services.AddScoped(
            _ =>
                _quotationAlertRepositoryMock
                    .Object);

        services.AddScoped(
            _ =>
                _notificationServiceMock
                    .Object);

        services.AddScoped(
            _ =>
                _userRepositoryMock
                    .Object);

        services.AddScoped(
            _ =>
                _emailServiceMock
                    .Object);

        return services
            .BuildServiceProvider();
    }

    private QuotationUpdatedConsumerWorker CreateWorker(
        IServiceScopeFactory scopeFactory,
        RabbitMqOptions? options = null)
    {
        options ??=
            CreateRabbitMqOptions();

        return new QuotationUpdatedConsumerWorker(
            _messageConsumerMock.Object,
            scopeFactory,
            Options.Create(
                options),
            _loggerMock.Object);
    }

    private static RabbitMqOptions CreateRabbitMqOptions()
    {
        return new RabbitMqOptions
        {
            QuotationQueueName =
                QueueName,

            QuotationRoutingKey =
                RoutingKey
        };
    }

    private static QuotationAlert CreateAlert(
        Guid userId,
        AlertCondition condition,
        decimal targetPrice)
    {
        return new QuotationAlert(
            userId,
            CurrencyPair.Create(
                "USD",
                "BRL"),
            condition,
            targetPrice);
    }

    private static User CreateUser(
        string name = "Leonardo",
        string email = "teste@email.com")
    {
        return new User(
            name,
            Email.Create(
                email),
            "hash-da-senha");
    }

    private static QuotationUpdatedMessage CreateMessage(
        string baseCurrency = "USD",
        string quoteCurrency = "BRL",
        decimal bidPrice = 5.10m)
    {
        return new QuotationUpdatedMessage
        {
            Id =
                Guid.NewGuid(),

            BaseCurrency =
                baseCurrency,

            QuoteCurrency =
                quoteCurrency,

            CurrencyPair =
                $"{baseCurrency}/{quoteCurrency}",

            BidPrice =
                bidPrice,

            AskPrice =
                bidPrice + 0.01m,

            HighPrice =
                bidPrice + 0.05m,

            LowPrice =
                bidPrice - 0.05m,

            Variation =
                0.01m,

            VariationPercentage =
                0.20m,

            QuotationDate =
                DateTime.UtcNow,

            PublishedAt =
                DateTime.UtcNow
        };
    }
}