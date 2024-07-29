using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Net;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using Para.Bussiness.RabbitMQ;

namespace Para.Bussiness.Notification;

public class EmailJob : INotificationService
{
    private readonly RabbitMQClient _rabbitMQClient;
    private readonly string _queueName;
    private readonly IConfiguration _configuration;

    public EmailJob(RabbitMQClient rabbitMQClient, IConfiguration configuration)
    {
        _rabbitMQClient = rabbitMQClient;
        _queueName = configuration["RabbitMQ:QueueName"];
        _configuration = configuration;
    }

    public void ProcessQueue()
    {
        var channel = _rabbitMQClient.GetChannel();

        var result = channel.BasicGet(_queueName, autoAck: false);

        while (result != null)
        {
            var message = Encoding.UTF8.GetString(result.Body.ToArray());
            SendEmailAsync(message).GetAwaiter().GetResult();
            channel.BasicAck(result.DeliveryTag, false);
            result = channel.BasicGet(_queueName, autoAck: false);
        }

        Console.WriteLine("Mailler gönderildi.");
    }

    public async Task SendEmailAsync(string message)
    {
        var emailInfo = JsonConvert.DeserializeObject<EmailInfo>(message);

        var myMail = new MailMessage
        {
            To = { emailInfo.To },
            From = new MailAddress("deprembilgideposu@gmail.com"), // From e-mail address
            Subject = emailInfo.Subject,
            Body = emailInfo.Body,
            IsBodyHtml = true
        };

        using var smtpClient = new SmtpClient
        {
            Credentials = new NetworkCredential(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]),
            Port = int.Parse(_configuration["SmtpSettings:Port"]),
            Host = _configuration["SmtpSettings:Host"],
            EnableSsl = true
        };

        await smtpClient.SendMailAsync(myMail);
    }

    private class EmailInfo
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}




