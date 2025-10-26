using App.Core.DTOs;
using App.Core.Interface.RabbitMQ;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace App.Infrastructure.RabbitMQ
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly RabbitMQConnection _connection;

        public RabbitMQPublisher(RabbitMQConnection connection)
        {
            _connection = connection;
        }

        public async Task Publish(MessageDtoForRabbitMQ message)
        {
            try
            {
                var connection = await _connection.ConnectAsync();
                using var channel = await connection.CreateChannelAsync();

                // Queue declare sadece bir defa yapılmalı, genelde startup'ta.
                // Ama burada dursun, sistem çökmemesi için safety-net
                await channel.QueueDeclareAsync(
                    queue: QueueNames.ChatMessageSave,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: QueueNames.ChatMessageSave,
                    body: body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Publish Error] {ex.Message}");
            }
        }
    }
}
