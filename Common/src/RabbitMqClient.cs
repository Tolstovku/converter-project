using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common;

public interface IMessageQueueClient
{
    Task SendMessageAsync(string msg, string topic);
    Task SubscribeAsync(string queueName, Action<string> handler);
}

public class RabbitMqClient : IMessageQueueClient
{
    private const string HostName = "localhost";
    private readonly IChannel _channel;

    public RabbitMqClient()
    {
        var connectionFactory = new ConnectionFactory() { HostName = HostName };
        var connection = connectionFactory.CreateConnection();
        _channel = connection.CreateChannel();
    }

    public async Task SendMessageAsync(string msg, string topic)
    {
        await _channel.BasicPublishAsync("", topic, Encoding.UTF8.GetBytes(msg));
    }

    public async Task SubscribeAsync(string queueName, Action<string> handler)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            handler.Invoke(message);
            _channel.BasicAck(args.DeliveryTag, false);
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );
    }
}