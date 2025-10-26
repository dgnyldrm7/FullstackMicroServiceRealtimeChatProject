namespace App.Core.Options
{
    public class RabbitMQOptions
    {
        public string User { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string Uri { get; set; }
        public string QueueName { get; set; }
    }
}