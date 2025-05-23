﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitConsumerSample
{
    public class TextConsumer : BackgroundService
    {
        private readonly ILogger<TextConsumer> _logger;

        public TextConsumer(ILogger<TextConsumer> logger)
        {
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("👂 TextConsumer is starting...");

            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "sample-queue", durable: false, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"📩 Received message by {Environment.GetEnvironmentVariable("ConsumerName") ?? "default"}: {message}");

                // تأخیر تصادفی قبل از پردازش پیام
                Random rnd = new Random();
                int delay = rnd.Next(500, 3000); // تأخیر تصادفی بین 500 تا 3000 میلی‌ثانیه
                await Task.Delay(delay, stoppingToken);

                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: "sample-queue", autoAck: true, consumer: consumer);
            _logger.LogInformation("✅ Listening for messages ...");

            // فقط منتظر بمون تا سرویس متوقف بشه
            await Task.Delay(Timeout.Infinite, stoppingToken);

            _logger.LogInformation("🛑 TextConsumer is stopping.");
        }
    }
}
