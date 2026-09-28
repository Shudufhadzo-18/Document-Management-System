using Document_Management_System.Models.DTOS;

namespace Document_Management_System.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, string message, int? documentId = null);
        Task<IEnumerable<NotificationResponseDto>> GetForUserAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkReadAsync(int notificationId, string userId);
        Task MarkAllReadAsync(string userId);
    }
}
