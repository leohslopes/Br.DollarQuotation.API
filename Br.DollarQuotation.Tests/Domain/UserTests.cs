using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.ValueObjects;

namespace Br.DollarQuotation.Tests.Domain;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        var email = Email.Create("teste@email.com");

        var user = new User(
            "Leonardo",
            email,
            "hash-da-senha");

        Assert.NotNull(user);
        Assert.Equal("Leonardo", user.Name);
        Assert.Equal("teste@email.com", user.Email.Value);
        Assert.Equal("hash-da-senha", user.PasswordHash);
        Assert.True(user.IsActive);
        Assert.NotEqual(default, user.CreatedAt);
    }

    [Fact]
    public void Constructor_WithInvalidName_ShouldThrowDomainException()
    {
        var email = Email.Create("teste@email.com");

        var exception = Assert.Throws<DomainException>(
            () => new User(
                "Le",
                email,
                "hash-da-senha"));

        Assert.Contains(
            "mínimo",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_WithEmptyPasswordHash_ShouldThrowDomainException()
    {
        var email = Email.Create("teste@email.com");

        var exception = Assert.Throws<DomainException>(
            () => new User(
                "Leonardo",
                email,
                ""));

        Assert.Contains(
            "hash da senha",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Activate_ShouldSetUserAsActive()
    {
        var user = CreateUser();

        user.Deactivate();

        Assert.False(user.IsActive);

        user.Activate();

        Assert.True(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void Deactivate_ShouldSetUserAsInactive()
    {
        var user = CreateUser();

        user.Deactivate();

        Assert.False(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        var user = CreateUser();

        user.UpdateName(
            "Leonardo Silverio");

        Assert.Equal(
            "Leonardo Silverio",
            user.Name);

        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateEmail()
    {
        var user = CreateUser();

        var newEmail = Email.Create(
            "novo@email.com");

        user.UpdateEmail(newEmail);

        Assert.Equal(
            "novo@email.com",
            user.Email.Value);

        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void UpdatePassword_WithValidHash_ShouldUpdatePassword()
    {
        var user = CreateUser();

        user.UpdatePassword(
            "novo-hash");

        Assert.Equal(
            "novo-hash",
            user.PasswordHash);

        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void UpdatePhoto_WithValidPng_ShouldUpdatePhoto()
    {
        var user = CreateUser();

        var base64 = Convert.ToBase64String(
            new byte[] { 1, 2, 3, 4, 5 });

        user.UpdatePhoto(
            base64,
            "image/png");

        Assert.Equal(
            base64,
            user.PhotoBase64);

        Assert.Equal(
            "image/png",
            user.PhotoContentType);

        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void UpdatePhoto_WithInvalidBase64_ShouldThrowDomainException()
    {
        var user = CreateUser();

        var exception =
            Assert.Throws<DomainException>(
                () => user.UpdatePhoto(
                    "base64-invalido",
                    "image/png"));

        Assert.Contains(
            "Base64",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UpdatePhoto_WithInvalidContentType_ShouldThrowDomainException()
    {
        var user = CreateUser();

        var base64 = Convert.ToBase64String(
            new byte[] { 1, 2, 3 });

        var exception =
            Assert.Throws<DomainException>(
                () => user.UpdatePhoto(
                    base64,
                    "application/pdf"));

        Assert.Contains(
            "Formato de imagem",
            exception.Message);
    }

    [Fact]
    public void UpdatePhoto_WithPhotoGreaterThanTwoMb_ShouldThrowDomainException()
    {
        var user = CreateUser();

        var bytes = new byte[
            2 * 1024 * 1024 + 1];

        var base64 =
            Convert.ToBase64String(bytes);

        var exception =
            Assert.Throws<DomainException>(
                () => user.UpdatePhoto(
                    base64,
                    "image/jpeg"));

        Assert.Contains(
            "2 MB",
            exception.Message);
    }

    [Fact]
    public void UpdatePhoto_WithDataUri_ShouldNormalizeBase64()
    {
        var user = CreateUser();

        var rawBase64 =
            Convert.ToBase64String(
                new byte[] { 10, 20, 30 });

        var dataUri =
            $"data:image/png;base64,{rawBase64}";

        user.UpdatePhoto(
            dataUri,
            "image/png");

        Assert.Equal(
            rawBase64,
            user.PhotoBase64);
    }

    [Fact]
    public void RemovePhoto_ShouldClearPhotoData()
    {
        var user = CreateUser();

        var base64 =
            Convert.ToBase64String(
                new byte[] { 1, 2, 3 });

        user.UpdatePhoto(
            base64,
            "image/png");

        user.RemovePhoto();

        Assert.Null(user.PhotoBase64);
        Assert.Null(user.PhotoContentType);
        Assert.NotNull(user.UpdatedAt);
    }

    private static User CreateUser()
    {
        return new User(
            "Leonardo",
            Email.Create(
                "teste@email.com"),
            "hash-da-senha");
    }
}