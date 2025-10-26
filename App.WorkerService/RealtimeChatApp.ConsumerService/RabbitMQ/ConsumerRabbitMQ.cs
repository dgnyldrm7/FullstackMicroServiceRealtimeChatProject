using Microsoft.Data.SqlClient;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RealtimeChatApp.ConsumerService.Db;
using RealtimeChatApp.ConsumerService.Models;
using System.Text;
using System.Text.Json;

namespace RealtimeChatApp.ConsumerService.RabbitMQ;

public class ConsumerRabbitMQ : IConsumerRabbitMQ
{
    private readonly RabbitMQConnection _connection;
    private readonly IConfiguration _configuration;
    private readonly IDbConfiguration dbConfiguration;

    public ConsumerRabbitMQ(RabbitMQConnection connection, IConfiguration configuration, IDbConfiguration dbConfiguration)
    {
        _connection = connection;
        _configuration = configuration;
        this.dbConfiguration = dbConfiguration;
    }

    public async Task ConsumeAsync<T>(string queueName, Func<T, Task> onMessageReceived)
    {
        try
        {
            var connection = await _connection.ConnectAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            Console.WriteLine($"[✔] Queue dinleniyor: {queueName}");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var deliveryTag = ea.DeliveryTag;
                try
                {
                    var body = ea.Body.ToArray();
                    var jsonMessage = Encoding.UTF8.GetString(body);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[📩] Mesaj alındı: {jsonMessage}");
                    Console.ResetColor();

                    var message = JsonSerializer.Deserialize<T>(jsonMessage);

                    if (message is MessageModel msg)
                    {
                        //When the message is of type MessageModel, save it to the database!
                        await dbConfiguration.SaveMessageToDatabaseAsync(msg);
                    }

                    if (message != null)
                        await onMessageReceived(message);

                    await channel.BasicAckAsync(deliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[❌] Mesaj işlenemedi: {ex.Message}");
                    Console.ResetColor();

                    await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

            Console.WriteLine("[ℹ️] Mesaj bekleniyor... Çıkmak için CTRL+C");
            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[❌] Consumer hata: {ex.Message}");
            Console.ResetColor();
        }
    }

    
}
