using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.Services;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Domain.ValueObjects;
using Moq;

namespace Br.DollarQuotation.Tests.Application;

public sealed class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    public UserServiceTests()
    {
        _userRepositoryMock =
            new Mock<IUserRepository>();

        _passwordHasherMock =
            new Mock<IPasswordHasher>();
    }

    #region RegisterAsync

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        _userRepositoryMock
            .Setup(repository =>
                repository.EmailExistsAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(hasher =>
                hasher.Hash(
                    request.Password))
            .Returns(
                "password-hash");

        var service =
            CreateService();

        // Act
        var response =
            await service.RegisterAsync(
                request);

        // Assert
        Assert.NotNull(
            response);

        Assert.Equal(
            request.Name,
            response.Name);

        Assert.Equal(
            request.Email,
            response.Email);

        Assert.Equal(
            request.Role,
            response.Role);

        Assert.True(
            response.IsActive);

        _passwordHasherMock
            .Verify(
                hasher =>
                    hasher.Hash(
                        request.Password),
                Times.Once);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<User>(
                            user =>
                                user.Name ==
                                    request.Name &&
                                user.Email.Value ==
                                    request.Email &&
                                user.PasswordHash ==
                                    "password-hash" &&
                                user.Role ==
                                    UserRole.User),
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldThrowEmailAlreadyRegisteredException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        _userRepositoryMock
            .Setup(repository =>
                repository.EmailExistsAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service =
            CreateService();

        // Act / Assert
        await Assert
            .ThrowsAsync<EmailAlreadyRegisteredException>(
                () =>
                    service.RegisterAsync(
                        request));

        _passwordHasherMock
            .Verify(
                hasher =>
                    hasher.Hash(
                        It.IsAny<string>()),
                Times.Never);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.AddAsync(
                        It.IsAny<User>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithDifferentPasswords_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.ConfirmPassword =
            "OutraSenha@123";

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.RegisterAsync(
                            request));

        // Assert
        Assert.Equal(
            "A senha e a confirmação da senha não são iguais.",
            exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordLessThanEightCharacters_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.Password =
            "Ab@123";

        request.ConfirmPassword =
            "Ab@123";

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.RegisterAsync(
                            request));

        // Assert
        Assert.Equal(
            "A senha deve possuir no mínimo 8 caracteres.",
            exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordWithoutUpperCase_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.Password =
            "senha@123";

        request.ConfirmPassword =
            "senha@123";

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.RegisterAsync(
                            request));

        // Assert
        Assert.Equal(
            "A senha deve possuir pelo menos uma letra maiúscula.",
            exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordWithoutLowerCase_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.Password =
            "SENHA@123";

        request.ConfirmPassword =
            "SENHA@123";

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.RegisterAsync(
                            request));

        // Assert
        Assert.Equal(
            "A senha deve possuir pelo menos uma letra minúscula.",
            exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordWithoutNumber_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.Password =
            "Senha@Teste";

        request.ConfirmPassword =
            "Senha@Teste";

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.RegisterAsync(
                            request));

        // Assert
        Assert.Equal(
            "A senha deve possuir pelo menos um número.",
            exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordWithoutSpecialCharacter_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.Password =
            "Senha123";

        request.ConfirmPassword =
            "Senha123";

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.RegisterAsync(
                            request));

        // Assert
        Assert.Equal(
            "A senha deve possuir pelo menos um caractere especial.",
            exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithPhotoWithoutContentType_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.PhotoBase64 =
            Convert.ToBase64String(
                new byte[]
                {
                    1,
                    2,
                    3
                });

        request.PhotoContentType =
            null;

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.RegisterAsync(
                            request));

        // Assert
        Assert.Equal(
            "O tipo do arquivo da foto é obrigatório.",
            exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithContentTypeWithoutPhoto_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.PhotoBase64 =
            null;

        request.PhotoContentType =
            "image/png";

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.RegisterAsync(
                            request));

        // Assert
        Assert.Equal(
            "A foto deve ser informada junto com o tipo do arquivo.",
            exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithAdminRole_ShouldCreateAdminUser()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.Role =
            UserRole.Admin.ToString();

        _userRepositoryMock
            .Setup(repository =>
                repository.EmailExistsAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(hasher =>
                hasher.Hash(
                    request.Password))
            .Returns(
                "password-hash");

        var service =
            CreateService();

        // Act
        var response =
            await service.RegisterAsync(
                request);

        // Assert
        Assert.Equal(
            UserRole.Admin.ToString(),
            response.Role);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<User>(
                            user =>
                                user.Role ==
                                    UserRole.Admin),
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidRole_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            CreateRegisterRequest();

        request.Role =
            "SuperAdmin";

        _userRepositoryMock
            .Setup(repository =>
                repository.EmailExistsAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.RegisterAsync(
                            request));

        // Assert
        Assert.Equal(
            "Perfil de acesso inválido. Utilize User ou Admin.",
            exception.Message);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.AddAsync(
                        It.IsAny<User>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user =
            CreateUser();

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    user.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var service =
            CreateService();

        // Act
        var response =
            await service.GetByIdAsync(
                user.Id);

        // Assert
        Assert.NotNull(
            response);

        Assert.Equal(
            user.Id,
            response.Id);

        Assert.Equal(
            user.Name,
            response.Name);

        Assert.Equal(
            user.Email.Value,
            response.Email);

        Assert.Equal(
            user.Role.ToString(),
            response.Role);

        Assert.Equal(
            user.IsActive,
            response.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ShouldThrowDomainException()
    {
        // Arrange
        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.GetByIdAsync(
                            Guid.Empty));

        // Assert
        Assert.Equal(
            "O identificador do usuário é obrigatório.",
            exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var id =
            Guid.NewGuid();

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (User?)null);

        var service =
            CreateService();

        // Act / Assert
        await Assert
            .ThrowsAsync<UserNotFoundException>(
                () =>
                    service.GetByIdAsync(
                        id));
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var user =
            CreateUser();

        var request =
            new UpdateUserRequest
            {
                Name =
                    "Leonardo Silverio",

                Email =
                    "novo@email.com",

                Role =
                    UserRole.User.ToString()
            };

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    user.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _userRepositoryMock
            .Setup(repository =>
                repository.EmailExistsAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                false);

        var service =
            CreateService();

        // Act
        var response =
            await service.UpdateAsync(
                user.Id,
                request);

        // Assert
        Assert.Equal(
            "Leonardo Silverio",
            response.Name);

        Assert.Equal(
            "novo@email.com",
            response.Email);

        Assert.Equal(
            UserRole.User.ToString(),
            response.Role);

        Assert.NotNull(
            response.UpdatedAt);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        user,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNewEmailAlreadyExists_ShouldThrowEmailAlreadyRegisteredException()
    {
        // Arrange
        var user =
            CreateUser();

        var request =
            new UpdateUserRequest
            {
                Name =
                    "Leonardo",

                Email =
                    "existente@email.com",

                Role =
                    UserRole.User.ToString()
            };

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    user.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _userRepositoryMock
            .Setup(repository =>
                repository.EmailExistsAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                true);

        var service =
            CreateService();

        // Act / Assert
        await Assert
            .ThrowsAsync<EmailAlreadyRegisteredException>(
                () =>
                    service.UpdateAsync(
                        user.Id,
                        request));

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        It.IsAny<User>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserIsLastActiveAdminAndRoleChangesToUser_ShouldThrowDomainException()
    {
        // Arrange
        var admin =
            CreateUser(
                role: UserRole.Admin);

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

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    admin.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                admin);

        _userRepositoryMock
            .Setup(repository =>
                repository.CountActiveAdminsAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                1);

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.UpdateAsync(
                            admin.Id,
                            request));

        // Assert
        Assert.Equal(
            "Não é possível alterar o perfil do último administrador ativo do sistema.",
            exception.Message);

        Assert.Equal(
            UserRole.Admin,
            admin.Role);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.CountActiveAdminsAsync(
                        It.IsAny<CancellationToken>()),
                Times.Once);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        It.IsAny<User>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenThereAreMultipleActiveAdmins_ShouldAllowRoleChangeToUser()
    {
        // Arrange
        var admin =
            CreateUser(
                role: UserRole.Admin);

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

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    admin.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                admin);

        _userRepositoryMock
            .Setup(repository =>
                repository.CountActiveAdminsAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                2);

        var service =
            CreateService();

        // Act
        var response =
            await service.UpdateAsync(
                admin.Id,
                request);

        // Assert
        Assert.Equal(
            UserRole.User.ToString(),
            response.Role);

        Assert.Equal(
            UserRole.User,
            admin.Role);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.CountActiveAdminsAsync(
                        It.IsAny<CancellationToken>()),
                Times.Once);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        admin,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AdminKeepingAdminRole_ShouldNotCountActiveAdmins()
    {
        // Arrange
        var admin =
            CreateUser(
                role: UserRole.Admin);

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

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    admin.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                admin);

        var service =
            CreateService();

        // Act
        var response =
            await service.UpdateAsync(
                admin.Id,
                request);

        // Assert
        Assert.Equal(
            "Leonardo Atualizado",
            response.Name);

        Assert.Equal(
            UserRole.Admin.ToString(),
            response.Role);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.CountActiveAdminsAsync(
                        It.IsAny<CancellationToken>()),
                Times.Never);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        admin,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    #endregion

    #region UpdatePhotoAsync

    [Fact]
    public async Task UpdatePhotoAsync_WithValidPhoto_ShouldUpdateUserPhoto()
    {
        // Arrange
        var user =
            CreateUser();

        var base64 =
            Convert.ToBase64String(
                new byte[]
                {
                    1,
                    2,
                    3
                });

        var request =
            new UpdateUserPhotoRequest
            {
                PhotoBase64 =
                    base64,

                PhotoContentType =
                    "image/png"
            };

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    user.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var service =
            CreateService();

        // Act
        var response =
            await service.UpdatePhotoAsync(
                user.Id,
                request);

        // Assert
        Assert.Equal(
            base64,
            response.PhotoBase64);

        Assert.Equal(
            "image/png",
            response.PhotoContentType);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        user,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    #endregion

    #region Activate / Deactivate

    [Fact]
    public async Task DeactivateAsync_WithExistingUser_ShouldDeactivateUser()
    {
        // Arrange
        var user =
            CreateUser(
                role: UserRole.User);

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    user.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var service =
            CreateService();

        // Act
        var response =
            await service.DeactivateAsync(
                user.Id);

        // Assert
        Assert.False(
            response.IsActive);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.CountActiveAdminsAsync(
                        It.IsAny<CancellationToken>()),
                Times.Never);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        user,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WhenUserIsLastActiveAdmin_ShouldThrowDomainException()
    {
        // Arrange
        var admin =
            CreateUser(
                role: UserRole.Admin);

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    admin.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                admin);

        _userRepositoryMock
            .Setup(repository =>
                repository.CountActiveAdminsAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                1);

        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.DeactivateAsync(
                            admin.Id));

        // Assert
        Assert.Equal(
            "Não é possível desativar o último administrador ativo do sistema.",
            exception.Message);

        Assert.True(
            admin.IsActive);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.CountActiveAdminsAsync(
                        It.IsAny<CancellationToken>()),
                Times.Once);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        It.IsAny<User>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Fact]
    public async Task DeactivateAsync_WhenThereAreMultipleActiveAdmins_ShouldDeactivateAdmin()
    {
        // Arrange
        var admin =
            CreateUser(
                role: UserRole.Admin);

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    admin.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                admin);

        _userRepositoryMock
            .Setup(repository =>
                repository.CountActiveAdminsAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                2);

        var service =
            CreateService();

        // Act
        var response =
            await service.DeactivateAsync(
                admin.Id);

        // Assert
        Assert.False(
            response.IsActive);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.CountActiveAdminsAsync(
                        It.IsAny<CancellationToken>()),
                Times.Once);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        admin,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WhenAdminIsAlreadyInactive_ShouldNotCountActiveAdmins()
    {
        // Arrange
        var admin =
            CreateUser(
                role: UserRole.Admin);

        admin.Deactivate();

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    admin.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                admin);

        var service =
            CreateService();

        // Act
        var response =
            await service.DeactivateAsync(
                admin.Id);

        // Assert
        Assert.False(
            response.IsActive);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.CountActiveAdminsAsync(
                        It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Fact]
    public async Task ActivateAsync_WithInactiveUser_ShouldActivateUser()
    {
        // Arrange
        var user =
            CreateUser();

        user.Deactivate();

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(
                    user.Id,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var service =
            CreateService();

        // Act
        var response =
            await service.ActivateAsync(
                user.Id);

        // Assert
        Assert.True(
            response.IsActive);

        _userRepositoryMock
            .Verify(
                repository =>
                    repository.UpdateAsync(
                        user,
                        It.IsAny<CancellationToken>()),
                Times.Once);
    }

    #endregion

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_WithValidParameters_ShouldReturnPagedUsers()
    {
        // Arrange
        var users =
            new List<User>
            {
                CreateUser(
                    "Leonardo",
                    "leonardo@email.com"),

                CreateUser(
                    "Michael",
                    "michael@email.com")
            };

        _userRepositoryMock
            .Setup(repository =>
                repository.GetPagedAsync(
                    1,
                    10,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                users);

        _userRepositoryMock
            .Setup(repository =>
                repository.CountAsync(
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                15);

        var service =
            CreateService();

        // Act
        var response =
            await service.GetPagedAsync(
                1,
                10);

        // Assert
        Assert.Equal(
            2,
            response.Items.Count);

        Assert.Equal(
            1,
            response.Page);

        Assert.Equal(
            10,
            response.PageSize);

        Assert.Equal(
            15,
            response.TotalItems);

        Assert.Equal(
            2,
            response.TotalPages);
    }

    [Fact]
    public async Task GetPagedAsync_WithPageEqualZero_ShouldThrowDomainException()
    {
        // Arrange
        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.GetPagedAsync(
                            0,
                            10));

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
        var service =
            CreateService();

        // Act
        var exception =
            await Assert
                .ThrowsAsync<DomainException>(
                    () =>
                        service.GetPagedAsync(
                            1,
                            pageSize));

        // Assert
        Assert.Equal(
            "O tamanho da página deve estar entre 1 e 100.",
            exception.Message);
    }

    #endregion

    #region Helpers

    private UserService CreateService()
    {
        return new UserService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object);
    }

    private static RegisterUserRequest CreateRegisterRequest()
    {
        return new RegisterUserRequest
        {
            Name =
                "Leonardo",

            Email =
                "teste@email.com",

            Password =
                "Senha@123",

            ConfirmPassword =
                "Senha@123",

            Role =
                UserRole.User.ToString()
        };
    }

    private static User CreateUser(
        string name = "Leonardo",
        string email = "teste@email.com",
        UserRole role = UserRole.User)
    {
        return new User(
            name,
            Email.Create(
                email),
            "password-hash",
            role: role);
    }

    #endregion
}