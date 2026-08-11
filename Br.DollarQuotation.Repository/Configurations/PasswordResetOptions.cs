namespace Br.DollarQuotation.Repository.Configurations;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    public string FrontendResetPasswordUrl { get; set; } = string.Empty;

    public int TokenExpirationInMinutes { get; set; } = 30;
}