using App.Core.DTOs;
using App.Core.Entities;

namespace App.Core.Interface.RabbitMQ
{
    public interface IRabbitMQPublisher
    {
        Task Publish(MessageDtoForRabbitMQ message);
    }
}