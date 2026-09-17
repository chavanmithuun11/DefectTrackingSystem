namespace DTS.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailAsync(string to, string subject, string body, List<string> attachments, bool isHtml = true);
}
