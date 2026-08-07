using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.Services;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Domain.Models;
using Br.DollarQuotation.Domain.ValueObjects;
using Moq;

namespace Br.DollarQuotation.Tests.Application.Services;

public sealed class CurrencyQuotationServiceTests
{
    private readonly Mock<ICurrencyQuotationProvider> _providerMock;
    private readonly Mock<ICurrencyQuotationRepository> _repositoryMock;

    public CurrencyQuotationServiceTests()
    {
        _providerMock =
            new Mock<ICurrencyQuotationProvider>();

        _repositoryMock =
            new Mock<ICurrencyQuotationRepository>();
    }

    #region GetCurrentAsync

    [Fact]
    public async Task GetCurrentAsync_WithValidRequest_ShouldReturnQuotation()
    {
        // Arrange
        var quotation = CreateQuotation();

        var request = new GetCurrentQuotationRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = "BRL"
        };

        _providerMock
            .Setup(provider =>
                provider.GetCurrentAsync(
                    It.IsAny<CurrencyPair>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);

        _repositoryMock
            .Setup(repository =>
                repository.ExistsAsync(
                    It.IsAny<CurrencyPair>(),
                    quotation.QuotationDate,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CreateService();

        // Act
        var response =
            await service.GetCurrentAsync(request);

        // Assert
        Assert.NotNull(response);

        Assert.Equal(
            "USD",
            response.BaseCurrency);

        Assert.Equal(
            "BRL",
            response.QuoteCurrency);

        Assert.Equal(
            "USD/BRL",
            response.CurrencyPair);

        Assert.Equal(
            quotation.BidPrice,
            response.BidPrice);

        Assert.Equal(
            quotation.AskPrice,
            response.AskPrice);

        Assert.True(
            response.WasInserted);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenQuotationDoesNotExist_ShouldInsertQuotation()
    {
        // Arrange
        var quotation = CreateQuotation();

        var request = CreateCurrentRequest();

        _providerMock
            .Setup(provider =>
                provider.GetCurrentAsync(
                    It.IsAny<CurrencyPair>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);

        _repositoryMock
            .Setup(repository =>
                repository.ExistsAsync(
                    It.IsAny<CurrencyPair>(),
                    quotation.QuotationDate,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CreateService();

        // Act
        var response =
            await service.GetCurrentAsync(request);

        // Assert
        Assert.True(
            response.WasInserted);

        _repositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    quotation,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenQuotationAlreadyExists_ShouldNotInsertQuotation()
    {
        // Arrange
        var quotation = CreateQuotation();

        var request = CreateCurrentRequest();

        _providerMock
            .Setup(provider =>
                provider.GetCurrentAsync(
                    It.IsAny<CurrencyPair>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotation);

        _repositoryMock
            .Setup(repository =>
                repository.ExistsAsync(
                    It.IsAny<CurrencyPair>(),
                    quotation.QuotationDate,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var response =
            await service.GetCurrentAsync(request);

        // Assert
        Assert.False(
            response.WasInserted);

        _repositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<CurrencyQuotation>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentAsync_WithEmptyBaseCurrency_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetCurrentQuotationRequest
        {
            BaseCurrency = "",
            QuoteCurrency = "BRL"
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetCurrentAsync(request));

        // Assert
        Assert.Equal(
            "A moeda base é obrigatória.",
            exception.Message);

        _providerMock.Verify(
            provider =>
                provider.GetCurrentAsync(
                    It.IsAny<CurrencyPair>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentAsync_WithEmptyQuoteCurrency_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetCurrentQuotationRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = ""
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetCurrentAsync(request));

        // Assert
        Assert.Equal(
            "A moeda de cotação é obrigatória.",
            exception.Message);

        _providerMock.Verify(
            provider =>
                provider.GetCurrentAsync(
                    It.IsAny<CurrencyPair>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentAsync_WithNullRequest_ShouldThrowDomainException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetCurrentAsync(null!));

        // Assert
        Assert.Equal(
            "Os dados da consulta são obrigatórios.",
            exception.Message);

        _providerMock.Verify(
            provider =>
                provider.GetCurrentAsync(
                    It.IsAny<CurrencyPair>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentAsync_WithSameCurrencies_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetCurrentQuotationRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = "USD"
        };

        var service = CreateService();

        // Act / Assert
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetCurrentAsync(request));

        Assert.Equal(
            "A moeda base não pode ser igual à moeda de cotação.",
            exception.Message);

        _providerMock.Verify(
            provider =>
                provider.GetCurrentAsync(
                    It.IsAny<CurrencyPair>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenProviderThrowsException_ShouldPropagateException()
    {
        // Arrange
        var request = CreateCurrentRequest();

        _providerMock
            .Setup(provider =>
                provider.GetCurrentAsync(
                    It.IsAny<CurrencyPair>(),
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Erro ao consultar provider."));

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetCurrentAsync(request));

        // Assert
        Assert.Equal(
            "Erro ao consultar provider.",
            exception.Message);

        _repositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<CurrencyQuotation>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_WithValidRequest_ShouldReturnPagedResponse()
    {
        // Arrange
        var request = new GetQuotationPagedRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = "BRL",
            Page = 1,
            PageSize = 2
        };

        var quotations = new List<CurrencyQuotation>
        {
            CreateQuotation(),
            CreateQuotation()
        };

        _repositoryMock
            .Setup(repository =>
                repository.GetPagedAsync(
                    It.IsAny<CurrencyPair?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    request.Page,
                    request.PageSize,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(quotations);

        _repositoryMock
            .Setup(repository =>
                repository.CountAsync(
                    It.IsAny<CurrencyPair?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var service = CreateService();

        // Act
        var response =
            await service.GetPagedAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Items.Count);
        Assert.Equal(1, response.Page);
        Assert.Equal(2, response.PageSize);
        Assert.Equal(5, response.TotalItems);
        Assert.Equal(3, response.TotalPages);
    }

    [Fact]
    public async Task GetPagedAsync_WithoutCurrencyFilter_ShouldSendNullCurrencyPair()
    {
        // Arrange
        var request = new GetQuotationPagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        _repositoryMock
            .Setup(repository =>
                repository.GetPagedAsync(
                    null,
                    null,
                    null,
                    request.Page,
                    request.PageSize,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<CurrencyQuotation>());

        _repositoryMock
            .Setup(repository =>
                repository.CountAsync(
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = CreateService();

        // Act
        var response =
            await service.GetPagedAsync(request);

        // Assert
        Assert.Empty(
            response.Items);

        Assert.Equal(
            0,
            response.TotalItems);

        Assert.Equal(
            0,
            response.TotalPages);

        _repositoryMock.Verify(
            repository =>
                repository.GetPagedAsync(
                    null,
                    null,
                    null,
                    1,
                    10,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_WithOnlyBaseCurrency_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetQuotationPagedRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = null,
            Page = 1,
            PageSize = 10
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetPagedAsync(request));

        // Assert
        Assert.Equal(
            "A moeda base e a moeda de cotação devem ser informadas juntas.",
            exception.Message);
    }

    [Fact]
    public async Task GetPagedAsync_WithOnlyQuoteCurrency_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetQuotationPagedRequest
        {
            BaseCurrency = null,
            QuoteCurrency = "BRL",
            Page = 1,
            PageSize = 10
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetPagedAsync(request));

        // Assert
        Assert.Equal(
            "A moeda base e a moeda de cotação devem ser informadas juntas.",
            exception.Message);
    }

    [Fact]
    public async Task GetPagedAsync_WithPageEqualZero_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetQuotationPagedRequest
        {
            Page = 0,
            PageSize = 10
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetPagedAsync(request));

        // Assert
        Assert.Equal(
            "A página deve ser maior que zero.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetPagedAsync_WithInvalidPageSize_ShouldThrowDomainException(
        int pageSize)
    {
        // Arrange
        var request = new GetQuotationPagedRequest
        {
            Page = 1,
            PageSize = pageSize
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetPagedAsync(request));

        // Assert
        Assert.Equal(
            "O tamanho da página deve estar entre 1 e 100.",
            exception.Message);
    }

    [Fact]
    public async Task GetPagedAsync_WithStartDateGreaterThanEndDate_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetQuotationPagedRequest
        {
            Page = 1,
            PageSize = 10,

            StartDate = new DateTime(
                2026,
                8,
                10,
                0,
                0,
                0,
                DateTimeKind.Utc),

            EndDate = new DateTime(
                2026,
                8,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc)
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetPagedAsync(request));

        // Assert
        Assert.Equal(
            "A data inicial não pode ser maior que a data final.",
            exception.Message);
    }

    [Fact]
    public async Task GetPagedAsync_WithCurrencyFilter_ShouldSendCurrencyPairToRepository()
    {
        // Arrange
        var request = new GetQuotationPagedRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = "BRL",
            Page = 1,
            PageSize = 10
        };

        _repositoryMock
            .Setup(repository =>
                repository.GetPagedAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    request.Page,
                    request.PageSize,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<CurrencyQuotation>());

        _repositoryMock
            .Setup(repository =>
                repository.CountAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = CreateService();

        // Act
        await service.GetPagedAsync(request);

        // Assert
        _repositoryMock.Verify(
            repository =>
                repository.GetPagedAsync(
                    It.Is<CurrencyPair>(
                        pair =>
                            pair.BaseCurrency ==
                            CurrencyType.USD &&
                            pair.QuoteCurrency ==
                            CurrencyType.BRL),
                    null,
                    null,
                    1,
                    10,
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetSummaryAsync

    [Fact]
    public async Task GetSummaryAsync_WithValidData_ShouldReturnSummary()
    {
        // Arrange
        var request =
            CreateSummaryRequest();

        var firstQuotation =
            CreateQuotation(
                bidPrice: 5.00m,
                askPrice: 5.01m,
                quotationDate:
                    DateTime.UtcNow.AddHours(-2));

        var latestQuotation =
            CreateQuotation(
                bidPrice: 5.50m,
                askPrice: 5.51m,
                quotationDate:
                    DateTime.UtcNow);

        var summary =
            new CurrencyQuotationSummary
            {
                MinimumBidPrice = 4.90m,
                MaximumBidPrice = 5.60m,
                AverageBidPrice = 5.25m,
                TotalQuotations = 10
            };

        SetupSummary(
            summary,
            firstQuotation,
            latestQuotation);

        var service = CreateService();

        // Act
        var response =
            await service.GetSummaryAsync(request);

        // Assert
        Assert.NotNull(response);

        Assert.Equal(
            "USD",
            response.BaseCurrency);

        Assert.Equal(
            "BRL",
            response.QuoteCurrency);

        Assert.Equal(
            "USD/BRL",
            response.CurrencyPair);

        Assert.Equal(
            latestQuotation.BidPrice,
            response.LatestBidPrice);

        Assert.Equal(
            latestQuotation.AskPrice,
            response.LatestAskPrice);

        Assert.Equal(
            summary.MinimumBidPrice,
            response.MinimumBidPrice);

        Assert.Equal(
            summary.MaximumBidPrice,
            response.MaximumBidPrice);

        Assert.Equal(
            summary.AverageBidPrice,
            response.AverageBidPrice);

        Assert.Equal(
            summary.TotalQuotations,
            response.TotalQuotations);

        Assert.Equal(
            latestQuotation.QuotationDate,
            response.LatestQuotationDate);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldCalculateVariationBetweenFirstAndLatestQuotation()
    {
        // Arrange
        var request =
            CreateSummaryRequest();

        var firstQuotation =
            CreateQuotation(
                bidPrice: 5.00m,
                askPrice: 5.01m,
                quotationDate:
                    DateTime.UtcNow.AddHours(-2));

        var latestQuotation =
            CreateQuotation(
                bidPrice: 5.50m,
                askPrice: 5.51m,
                quotationDate:
                    DateTime.UtcNow);

        var summary =
            new CurrencyQuotationSummary
            {
                MinimumBidPrice = 5.00m,
                MaximumBidPrice = 5.50m,
                AverageBidPrice = 5.25m,
                TotalQuotations = 2
            };

        SetupSummary(
            summary,
            firstQuotation,
            latestQuotation);

        var service = CreateService();

        // Act
        var response =
            await service.GetSummaryAsync(request);

        // Assert
        // (5.50 - 5.00) / 5.00 * 100 = 10%
        Assert.Equal(
            10m,
            response.VariationPercentage);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenSummaryDoesNotExist_ShouldThrowQuotationNotFoundException()
    {
        // Arrange
        var request =
            CreateSummaryRequest();

        _repositoryMock
            .Setup(repository =>
                repository.GetSummaryAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (CurrencyQuotationSummary?)null);

        var service = CreateService();

        // Act / Assert
        await Assert.ThrowsAsync<
            QuotationNotFoundException>(
            () => service.GetSummaryAsync(request));

        _repositoryMock.Verify(
            repository =>
                repository.GetPagedAsync(
                    It.IsAny<CurrencyPair?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenLatestQuotationDoesNotExist_ShouldThrowQuotationNotFoundException()
    {
        // Arrange
        var request =
            CreateSummaryRequest();

        var summary =
            new CurrencyQuotationSummary
            {
                MinimumBidPrice = 5.00m,
                MaximumBidPrice = 5.50m,
                AverageBidPrice = 5.25m,
                TotalQuotations = 2
            };

        _repositoryMock
            .Setup(repository =>
                repository.GetSummaryAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _repositoryMock
            .Setup(repository =>
                repository.GetPagedAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    1,
                    1,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<CurrencyQuotation>());

        var service = CreateService();

        // Act / Assert
        await Assert.ThrowsAsync<
            QuotationNotFoundException>(
            () => service.GetSummaryAsync(request));
    }

    [Fact]
    public async Task GetSummaryAsync_WhenFirstQuotationDoesNotExist_ShouldThrowQuotationNotFoundException()
    {
        // Arrange
        var request =
            CreateSummaryRequest();

        var latestQuotation =
            CreateQuotation();

        var summary =
            new CurrencyQuotationSummary
            {
                MinimumBidPrice = 5.00m,
                MaximumBidPrice = 5.50m,
                AverageBidPrice = 5.25m,
                TotalQuotations = 2
            };

        _repositoryMock
            .Setup(repository =>
                repository.GetSummaryAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _repositoryMock
            .Setup(repository =>
                repository.GetPagedAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    1,
                    1,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<CurrencyQuotation>
                {
                    latestQuotation
                });

        _repositoryMock
            .Setup(repository =>
                repository.GetFirstAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (CurrencyQuotation?)null);

        var service = CreateService();

        // Act / Assert
        await Assert.ThrowsAsync<
            QuotationNotFoundException>(
            () => service.GetSummaryAsync(request));
    }

    [Fact]
    public async Task GetSummaryAsync_WithEmptyBaseCurrency_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetQuotationSummaryRequest
        {
            BaseCurrency = "",
            QuoteCurrency = "BRL"
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetSummaryAsync(request));

        // Assert
        Assert.Equal(
            "A moeda base é obrigatória.",
            exception.Message);
    }

    [Fact]
    public async Task GetSummaryAsync_WithEmptyQuoteCurrency_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetQuotationSummaryRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = ""
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetSummaryAsync(request));

        // Assert
        Assert.Equal(
            "A moeda de cotação é obrigatória.",
            exception.Message);
    }

    [Fact]
    public async Task GetSummaryAsync_WithStartDateGreaterThanEndDate_ShouldThrowDomainException()
    {
        // Arrange
        var request = new GetQuotationSummaryRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = "BRL",

            StartDate = new DateTime(
                2026,
                8,
                10,
                0,
                0,
                0,
                DateTimeKind.Utc),

            EndDate = new DateTime(
                2026,
                8,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc)
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.GetSummaryAsync(request));

        // Assert
        Assert.Equal(
            "A data inicial não pode ser maior que a data final.",
            exception.Message);
    }

    #endregion

    #region Helpers

    private CurrencyQuotationService CreateService()
    {
        return new CurrencyQuotationService(
            _providerMock.Object,
            _repositoryMock.Object);
    }

    private static GetCurrentQuotationRequest CreateCurrentRequest()
    {
        return new GetCurrentQuotationRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = "BRL"
        };
    }

    private static GetQuotationSummaryRequest CreateSummaryRequest()
    {
        return new GetQuotationSummaryRequest
        {
            BaseCurrency = "USD",
            QuoteCurrency = "BRL"
        };
    }

    private void SetupSummary(
        CurrencyQuotationSummary summary,
        CurrencyQuotation firstQuotation,
        CurrencyQuotation latestQuotation)
    {
        _repositoryMock
            .Setup(repository =>
                repository.GetSummaryAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        _repositoryMock
            .Setup(repository =>
                repository.GetPagedAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    1,
                    1,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<CurrencyQuotation>
                {
                    latestQuotation
                });

        _repositoryMock
            .Setup(repository =>
                repository.GetFirstAsync(
                    It.IsAny<CurrencyPair>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstQuotation);
    }

    private static CurrencyQuotation CreateQuotation(
        decimal bidPrice = 5.20m,
        decimal askPrice = 5.21m,
        DateTime? quotationDate = null)
    {
        var effectiveQuotationDate =
            quotationDate ?? DateTime.UtcNow;

        var highPrice = Math.Max(
            bidPrice,
            askPrice) + 0.05m;

        var lowPrice = Math.Min(
            bidPrice,
            askPrice) - 0.05m;

        return new CurrencyQuotation(
            CurrencyPair.Create(
                "USD",
                "BRL"),
            bidPrice: bidPrice,
            askPrice: askPrice,
            highPrice: highPrice,
            lowPrice: lowPrice,
            variation: 0.05m,
            variationPercentage: 0.97m,
            quotationDate: effectiveQuotationDate);
    }

    #endregion
}