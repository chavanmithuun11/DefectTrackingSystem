using DTS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DTS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        _logger.LogInformation("Email to: {To}, Subject: {Subject}, Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string to, string subject, string body, List<string> attachments, bool isHtml = true)
    {
        _logger.LogInformation("Email to: {To}, Subject: {Subject}, Attachments: {Count}", to, subject, attachments.Count);
        return Task.CompletedTask;
    }
}
