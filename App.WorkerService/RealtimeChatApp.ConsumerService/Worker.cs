using Microsoft.AspNetCore.SignalR.Client;
using RealtimeChatApp.ConsumerService.Models;
using RealtimeChatApp.ConsumerService.RabbitMQ;
using System.Net.Http.Json;
using System.Text.Json;

namespace RealtimeChatApp.ConsumerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConsumerRabbitMQ _consumer;
        private readonly IConfiguration _configuration;
        private HubConnection _hubConnection;

        public Worker(ILogger<Worker> logger, IConsumerRabbitMQ consumer, IConfiguration configuration)
        {
            _logger = logger;
            _consumer = consumer;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumeAsync<MessageModel>("chat-message-save", async (message) =>
            {
                try
                {
                    if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
                    {
                        _logger.LogWarning("Yeniden bağlanılıyor...");
                        await EnsureHubConnectedAsync(stoppingToken);
                    }


                    await _hubConnection.InvokeAsync("SendMessageFromWorker", new ChatMessageDto
                    {
                        SenderNumber = message.SenderNumber,
                        ReceiverNumber = message.ReceiverNumber,
                        Content = message.Content,
                        SentAt = message.SentAt
                    });



                    _logger.LogInformation($"✅ SignalR Hub'a mesaj gönderildi: {JsonSerializer.Serialize(message)}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ SignalR Hub çağrısı başarısız oldu.");
                }
            });
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await EnsureHubConnectedAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        private async Task EnsureHubConnectedAsync(CancellationToken cancellationToken)
        {
            var connectionInfosForWorkerAuth = _configuration.GetSection("WorkerAuth");

            var _userName = connectionInfosForWorkerAuth.GetSection("ServiceUser").Value;
            var _password = connectionInfosForWorkerAuth.GetSection("ServiceSecret").Value;
            var authEndpoint = connectionInfosForWorkerAuth.GetSection("WorkerAuthEndpoint").Value;

            using var http = new HttpClient();
            var loginResponse = await http.PostAsJsonAsync(authEndpoint , new
            {
                username = _userName,
                password = _password
            });

            if (!loginResponse.IsSuccessStatusCode)
            {
                _logger.LogError("❌ Worker login başarısız! Status: {status}", loginResponse.StatusCode);
                return;
            }

            var json = await loginResponse.Content.ReadFromJsonAsync<WorkerLoginResponse>();
            var token = json?.Token;

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("❌ Token alınamadı!");
                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"https://localhost:7281/workerhub?access_token={token}", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .WithAutomaticReconnect()
                .Build();

            await _hubConnection.StartAsync(cancellationToken);
            _logger.LogInformation("✅ Worker Service connected to SignalR WorkerHub.");
        }
    }
}
