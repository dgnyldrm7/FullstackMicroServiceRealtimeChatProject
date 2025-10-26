using RealtimeChatApp.ConsumerService;
using RealtimeChatApp.ConsumerService.Db;
using RealtimeChatApp.ConsumerService.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<RabbitMQConnection>();

builder.Services.AddSingleton<IConsumerRabbitMQ, ConsumerRabbitMQ>();

builder.Services.AddSingleton<IDbConfiguration, DbConfiguration>();

var host = builder.Build();
host.Run();
