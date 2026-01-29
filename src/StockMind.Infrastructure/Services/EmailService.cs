using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StockMind.Application.Interfaces;

namespace StockMind.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendLowStockAlertEmailAsync(
        string productName,
        string productSku,
        string warehouseName,
        decimal currentQuantity,
        decimal minimumQuantity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var toEmail = _configuration["Email:AlertsEmail"];

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("Alerts email not configured. Skipping email.");
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            var subject = $"?? ALERTA: Estoque Baixo - {productName}";
            var body = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .alert-box {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 20px; border-radius: 5px; }}
        .product-info {{ margin: 15px 0; }}
        .quantity {{ font-size: 24px; font-weight: bold; color: #dc3545; }}
    </style>
</head>
<body>
    <div class='alert-box'>
        <h2>?? Alerta de Estoque Baixo</h2>
        <div class='product-info'>
            <p><strong>Produto:</strong> {productName}</p>
            <p><strong>SKU:</strong> {productSku}</p>
            <p><strong>Depósito:</strong> {warehouseName}</p>
        </div>
        <div>
            <p><strong>Quantidade Atual:</strong> <span class='quantity'>{currentQuantity:N2}</span></p>
            <p><strong>Estoque Mínimo:</strong> {minimumQuantity:N2}</p>
        </div>
        <p style='margin-top: 20px; color: #666;'>
            Este é um alerta automático do Sistema de Controle de Estoque.<br>
            Por favor, tome as medidas necessárias para reabastecer o produto.
        </p>
    </div>
</body>
</html>";

            var mailMessage = new MailMessage(fromEmail!, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation(
                "Low stock alert email sent for product {ProductName} ({ProductSku})",
                productName,
                productSku);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending low stock alert email");
            throw;
        }
    }
}
