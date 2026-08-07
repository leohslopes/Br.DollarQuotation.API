using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace Br.DollarQuotation.Repository.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher;

    public PasswordHasher()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("A senha é obrigatória.", nameof(password));
        }

        return _passwordHasher.HashPassword(user: null!, password);
    }

    public bool Verify(
        string password,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        var result = _passwordHasher.VerifyHashedPassword(user: null!, hashedPassword: passwordHash, providedPassword: password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}