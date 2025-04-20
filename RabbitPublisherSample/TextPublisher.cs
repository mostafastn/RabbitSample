using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitPublisherSample
{
    public class TextPublisher : BackgroundService
    {
        private readonly ILogger<TextPublisher> _logger;

        public TextPublisher(ILogger<TextPublisher> logger)
        {
            _logger = logger;
        }


        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    _logger.LogTrace("Background Service is starting.");

        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        _logger.LogTrace("Background Service is working at: {time}", DateTimeOffset.Now);
        //        await Task.Delay(5000, stoppingToken); // اجرای هر ۵ ثانیه یک بار
        //    }

        //    _logger.LogTrace("Background Service is stopping.");



        //    var factory = new ConnectionFactory() { HostName = "localhost" };

        //    using var connection = await factory.CreateConnectionAsync();
        //    using var channel =await connection.CreateChannelAsync();

        //    // تعریف صف
        //    await channel.QueueDeclareAsync(queue: "sample-queue", durable: false, exclusive: false, autoDelete: false);

        //    // پیام ارسالی
        //    string message = "پیامی از Publisher با .NET 9!";
        //    var body = Encoding.UTF8.GetBytes(message);

        //    // ارسال
        //    await channel.BasicPublishAsync(exchange: "", routingKey: "sample-queue", basicProperties: null, body: body, cancellationToken: stoppingToken);
        //    Console.WriteLine($"✅ پیام ارسال شد: {message}");


        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📡 Background Service is starting.");

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "sample-queue", durable: false, exclusive: false, autoDelete: false);

            int count = 1;

            while (!stoppingToken.IsCancellationRequested)
            {
                string message = $"📨 Message number {count++} from Publisher in {DateTime.Now}";
                var body = Encoding.UTF8.GetBytes(message);
                                
                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: "sample-queue", // فقط نام صف                    
                    body: Encoding.UTF8.GetBytes(message),
                    cancellationToken: stoppingToken
                    );

                _logger.LogInformation("✅ Message sent: {msg}", message);

                await Task.Delay(5000, stoppingToken); // هر ۵ ثانیه یک بار
            }

            _logger.LogInformation("🛑 Background Service is stopping.");
        }
    }
}
