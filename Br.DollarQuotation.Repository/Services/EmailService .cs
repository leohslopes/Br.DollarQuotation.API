using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Repository.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Br.DollarQuotation.Repository.Services;

public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _options;

    private readonly ILogger<EmailService> _logger;

    public EmailService( IOptions<EmailOptions> options,
        ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink, CancellationToken cancellationToken = default)
    {
        ValidateOptions();

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(new MailboxAddress(recipientName, recipientEmail));
        message.Subject = "Redefinição de senha - Câmbio Pulse";
        message.Body = new TextPart(TextFormat.Html)
            {
                Text = BuildPasswordResetEmail(recipientName, resetLink)
            };

        using var smtpClient = new SmtpClient();

        smtpClient.CheckCertificateRevocation = false;

        try
        {
            await smtpClient.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await smtpClient.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            await smtpClient.SendAsync(message, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation( "E-mail de recuperação de senha enviado com sucesso para {RecipientEmail}.", recipientEmail);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao enviar e-mail de recuperação de senha para {RecipientEmail}.", recipientEmail);

            throw;
        }
    }

    public async Task SendQuotationAlertTriggeredAsync(string recipientEmail, string recipientName, string currencyPair, decimal currentPrice, decimal targetPrice, string condition, CancellationToken cancellationToken = default)
    {
        ValidateOptions();

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(new MailboxAddress(recipientName,recipientEmail));
        message.Subject = $"Alerta de cotação atingido - {currencyPair}";
        message.Body = new TextPart(TextFormat.Html)
            {
                Text = BuildQuotationAlertEmail(recipientName, currencyPair, currentPrice,targetPrice, condition)
            };

        using var smtpClient = new SmtpClient();

        smtpClient.CheckCertificateRevocation =
            false;

        try
        {
            await smtpClient.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await smtpClient.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            await smtpClient.SendAsync(message, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("E-mail de alerta de cotação enviado com sucesso para {RecipientEmail}. " + "Par: {CurrencyPair}.", recipientEmail, currencyPair);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao enviar e-mail de alerta de cotação para {RecipientEmail}. " + "Par: {CurrencyPair}.", recipientEmail, currencyPair);

            throw;
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            throw new InvalidOperationException("O servidor SMTP não foi configurado." );
        }

        if (_options.SmtpPort <= 0)
        {
            throw new InvalidOperationException("A porta SMTP é inválida.");
        }

        if (string.IsNullOrWhiteSpace(_options.SenderName))
        {
            throw new InvalidOperationException("O nome do remetente não foi configurado.");
        }

        if (string.IsNullOrWhiteSpace(_options.SenderEmail))
        {
            throw new InvalidOperationException( "O e-mail do remetente não foi configurado.");
        }

        if (string.IsNullOrWhiteSpace(_options.Username))
        {
            throw new InvalidOperationException("O usuário SMTP não foi configurado.");
        }

        if (string.IsNullOrWhiteSpace(_options.Password))
        {
            throw new InvalidOperationException("A senha SMTP não foi configurada.");
        }
    }

    private static string BuildPasswordResetEmail(string recipientName, string resetLink)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
                <meta charset="UTF-8">
            </head>

            <body style="
                margin: 0;
                padding: 0;
                background-color: #f5f7fa;
                font-family: Arial, Helvetica, sans-serif;
                color: #17324d;
            ">

                <table
                    width="100%"
                    cellpadding="0"
                    cellspacing="0"
                    style="
                        background-color: #f5f7fa;
                        padding: 32px 16px;
                    "
                >
                    <tr>
                        <td align="center">

                            <table
                                width="100%"
                                cellpadding="0"
                                cellspacing="0"
                                style="
                                    max-width: 600px;
                                    background-color: #ffffff;
                                    border-radius: 16px;
                                    overflow: hidden;
                                    box-shadow: 0 8px 24px rgba(0, 59, 113, 0.08);
                                "
                            >

                                <tr>
                                    <td style="
                                        padding: 28px 32px;
                                        background: #003b71;
                                        color: #ffffff;
                                    ">
                                        <div style="
                                            font-size: 22px;
                                            font-weight: 700;
                                        ">
                                            Câmbio
                                            <span style="
                                                color: #ff8a1f;
                                            ">
                                                Pulse
                                            </span>
                                        </div>
                                    </td>
                                </tr>

                                <tr>
                                    <td style="
                                        padding: 36px 32px;
                                    ">

                                        <div style="
                                            margin-bottom: 12px;
                                            color: #ec7000;
                                            font-size: 12px;
                                            font-weight: 700;
                                            text-transform: uppercase;
                                            letter-spacing: 1px;
                                        ">
                                            Recuperação de acesso
                                        </div>

                                        <h1 style="
                                            margin: 0 0 18px;
                                            color: #17324d;
                                            font-size: 26px;
                                        ">
                                            Redefinição de senha
                                        </h1>

                                        <p style="
                                            margin: 0 0 16px;
                                            color: #5f6b78;
                                            font-size: 15px;
                                            line-height: 1.6;
                                        ">
                                            Olá, {recipientName}.
                                        </p>

                                        <p style="
                                            margin: 0 0 26px;
                                            color: #5f6b78;
                                            font-size: 15px;
                                            line-height: 1.6;
                                        ">
                                            Recebemos uma solicitação para redefinir
                                            a senha da sua conta no Câmbio Pulse.
                                            Clique no botão abaixo para criar
                                            uma nova senha.
                                        </p>

                                        <table
                                            cellpadding="0"
                                            cellspacing="0"
                                        >
                                            <tr>
                                                <td
                                                    align="center"
                                                    style="
                                                        border-radius: 10px;
                                                        background: #ec7000;
                                                    "
                                                >
                                                    <a
                                                        href="{resetLink}"
                                                        style="
                                                            display: inline-block;
                                                            padding: 15px 24px;
                                                            color: #ffffff;
                                                            font-size: 14px;
                                                            font-weight: 700;
                                                            text-decoration: none;
                                                        "
                                                    >
                                                        Redefinir minha senha
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>

                                        <p style="
                                            margin: 28px 0 0;
                                            color: #8793a0;
                                            font-size: 12px;
                                            line-height: 1.6;
                                        ">
                                            Este link possui tempo limitado e
                                            poderá ser utilizado apenas uma vez.
                                        </p>

                                        <p style="
                                            margin: 12px 0 0;
                                            color: #8793a0;
                                            font-size: 12px;
                                            line-height: 1.6;
                                        ">
                                            Se você não solicitou a redefinição
                                            da senha, ignore este e-mail.
                                        </p>

                                    </td>
                                </tr>

                                <tr>
                                    <td style="
                                        padding: 20px 32px;
                                        border-top: 1px solid #edf1f5;
                                        color: #9ca3ad;
                                        font-size: 11px;
                                    ">
                                        © 2026 Câmbio Pulse
                                    </td>
                                </tr>

                            </table>

                        </td>
                    </tr>
                </table>

            </body>
            </html>
            """;
    }

    private static string BuildQuotationAlertEmail(string recipientName, string currencyPair, decimal currentPrice, decimal targetPrice, string condition)
    {
        var currentPriceFormatted = currentPrice.ToString("N4", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
        var targetPriceFormatted = targetPrice.ToString("N4", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));

        return $"""
        <!DOCTYPE html>
        <html lang="pt-BR">

        <head>
            <meta charset="UTF-8">
        </head>

        <body style="
            margin: 0;
            padding: 0;
            background-color: #f5f7fa;
            font-family: Arial, Helvetica, sans-serif;
            color: #17324d;
        ">

            <table
                width="100%"
                cellpadding="0"
                cellspacing="0"
                style="
                    background-color: #f5f7fa;
                    padding: 32px 16px;
                "
            >
                <tr>
                    <td align="center">

                        <table
                            width="100%"
                            cellpadding="0"
                            cellspacing="0"
                            style="
                                max-width: 600px;
                                background-color: #ffffff;
                                border-radius: 16px;
                                overflow: hidden;
                                box-shadow: 0 8px 24px rgba(0, 59, 113, 0.08);
                            "
                        >

                            <tr>
                                <td style="
                                    padding: 28px 32px;
                                    background: #003b71;
                                    color: #ffffff;
                                ">
                                    <div style="
                                        font-size: 22px;
                                        font-weight: 700;
                                    ">
                                        Câmbio
                                        <span style="color: #ff8a1f;">
                                            Pulse
                                        </span>
                                    </div>
                                </td>
                            </tr>

                            <tr>
                                <td style="padding: 36px 32px;">

                                    <div style="
                                        margin-bottom: 12px;
                                        color: #ec7000;
                                        font-size: 12px;
                                        font-weight: 700;
                                        text-transform: uppercase;
                                        letter-spacing: 1px;
                                    ">
                                        Alerta de cotação
                                    </div>

                                    <h1 style="
                                        margin: 0 0 18px;
                                        color: #17324d;
                                        font-size: 26px;
                                    ">
                                        Seu preço-alvo foi atingido
                                    </h1>

                                    <p style="
                                        margin: 0 0 22px;
                                        color: #5f6b78;
                                        font-size: 15px;
                                        line-height: 1.6;
                                    ">
                                        Olá, {recipientName}.
                                    </p>

                                    <p style="
                                        margin: 0 0 26px;
                                        color: #5f6b78;
                                        font-size: 15px;
                                        line-height: 1.6;
                                    ">
                                        Um dos seus alertas cadastrados no
                                        Câmbio Pulse atingiu a condição definida.
                                    </p>

                                    <table
                                        width="100%"
                                        cellpadding="0"
                                        cellspacing="0"
                                        style="
                                            background-color: #f7f9fc;
                                            border-radius: 12px;
                                            padding: 8px;
                                        "
                                    >

                                        <tr>
                                            <td style="
                                                padding: 14px;
                                                color: #6b7785;
                                                font-size: 13px;
                                            ">
                                                Par de moedas
                                            </td>

                                            <td
                                                align="right"
                                                style="
                                                    padding: 14px;
                                                    color: #17324d;
                                                    font-weight: 700;
                                                "
                                            >
                                                {currencyPair}
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="
                                                padding: 14px;
                                                color: #6b7785;
                                                font-size: 13px;
                                            ">
                                                Condição
                                            </td>

                                            <td
                                                align="right"
                                                style="
                                                    padding: 14px;
                                                    color: #17324d;
                                                    font-weight: 700;
                                                "
                                            >
                                                {condition}
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="
                                                padding: 14px;
                                                color: #6b7785;
                                                font-size: 13px;
                                            ">
                                                Preço-alvo
                                            </td>

                                            <td
                                                align="right"
                                                style="
                                                    padding: 14px;
                                                    color: #17324d;
                                                    font-weight: 700;
                                                "
                                            >
                                                {targetPriceFormatted}
                                            </td>
                                        </tr>

                                        <tr>
                                            <td style="
                                                padding: 14px;
                                                color: #6b7785;
                                                font-size: 13px;
                                            ">
                                                Cotação atual
                                            </td>

                                            <td
                                                align="right"
                                                style="
                                                    padding: 14px;
                                                    color: #ec7000;
                                                    font-size: 18px;
                                                    font-weight: 700;
                                                "
                                            >
                                                {currentPriceFormatted}
                                            </td>
                                        </tr>

                                    </table>

                                    <p style="
                                        margin: 26px 0 0;
                                        color: #8793a0;
                                        font-size: 12px;
                                        line-height: 1.6;
                                    ">
                                        O alerta foi automaticamente marcado
                                        como disparado e não será acionado novamente
                                        até que seja reativado.
                                    </p>

                                </td>
                            </tr>

                            <tr>
                                <td style="
                                    padding: 20px 32px;
                                    border-top: 1px solid #edf1f5;
                                    color: #9ca3ad;
                                    font-size: 11px;
                                ">
                                    © 2026 Câmbio Pulse
                                </td>
                            </tr>

                        </table>

                    </td>
                </tr>
            </table>

        </body>

        </html>
        """;
    }
}