namespace Para.Bussiness.Notification;

public interface INotificationService
{
    public void ProcessQueue();
    public Task SendEmailAsync(string message);
}