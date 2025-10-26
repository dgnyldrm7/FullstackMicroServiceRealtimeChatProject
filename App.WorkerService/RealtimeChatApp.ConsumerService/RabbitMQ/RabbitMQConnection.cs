using RabbitMQ.Client;

namespace RealtimeChatApp.ConsumerService.RabbitMQ
{
    public class RabbitMQConnection
    {

        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IConfiguration _configuration;
        private readonly object _lock = new();

        public RabbitMQConnection(IConfiguration configuration)
        {
            _configuration = configuration;

            string connectionUri = _configuration!
                .GetSection("RabbitMQ")
                .GetSection("Uri").Value!;

            _connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(connectionUri)
            };

        }

        public async Task<IConnection> ConnectAsync()
        {
            if (_connection is { IsOpen: true })
                return _connection;

            lock (_lock)
            {
                if (_connection is { IsOpen: true })
                    return _connection;

                _connection = _connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
            }

            return await Task.FromResult(_connection);
        }
    }
}
