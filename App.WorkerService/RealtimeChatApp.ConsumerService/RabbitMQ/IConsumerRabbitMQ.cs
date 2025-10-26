namespace RealtimeChatApp.ConsumerService.RabbitMQ
{
    public interface IConsumerRabbitMQ
    {
        Task ConsumeAsync<T>(string queueName, Func<T, Task> onMessageReceived);
    }
}