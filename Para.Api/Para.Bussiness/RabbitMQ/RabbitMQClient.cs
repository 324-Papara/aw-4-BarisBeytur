using RabbitMQ.Client;
using System.Text;

namespace Para.Bussiness.RabbitMQ;

public class RabbitMQClient : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQClient(string hostname, int port, string username, string password, string queueName)
    {
        var factory = new ConnectionFactory()
        {
            HostName = hostname,
            Port = port,
            UserName = username,
            Password = password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public void SendMessage(string message, string queueName)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }

    public async Task SendMessageAsync(string message, string queueName)
    {
        var body = Encoding.UTF8.GetBytes(message);
        await Task.Run(() => _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body));
    }

    public IModel GetChannel() => _channel;

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
