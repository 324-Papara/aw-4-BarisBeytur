using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Para.Bussiness.Notification;

public class NotificationService : INotificationService
{

    private readonly IConfiguration _configuration;

    public NotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendEmail(string subject, string email, string content)
    {
        MailMessage mailim = new MailMessage();
        mailim.To.Add("beyturbaris@gmail.com");
        mailim.From = new MailAddress("deprembilgideposu@gmail.com");
        mailim.Subject = "Yeni bir bildiriminiz var!";
        mailim.Body = "Mail test body.";
        mailim.IsBodyHtml = true;
        SmtpClient smtpClient = new SmtpClient();
        smtpClient.Credentials = new NetworkCredential(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]);

        smtpClient.Port = Convert.ToInt32(_configuration["SmtpSettings:Port"]);
        smtpClient.Host = _configuration["SmtpSettings:Host"];
        smtpClient.EnableSsl = true;
        smtpClient.Send(mailim);
    }
}