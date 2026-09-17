namespace DTS.Domain.Interfaces;

public interface IAuditService
{
    Task LogAsync(string userId, string action, string entityName, string? entityId = null, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null);
}
