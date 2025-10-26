using App.Core.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace App.Infrastructure.RabbitMQ
{
    public class RabbitMQConnection
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private readonly object _lock = new();
        private readonly RabbitMQOptions _rabbitOptions;

        public RabbitMQConnection(IOptions<RabbitMQOptions> rabbitOptions)
        {

            _rabbitOptions = rabbitOptions.Value;

            _connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_rabbitOptions.Uri)
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
