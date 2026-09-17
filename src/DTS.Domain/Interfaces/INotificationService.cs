using DTS.Domain.Enums;

namespace DTS.Domain.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string title, string message, NotificationType type, string? url = null);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}
