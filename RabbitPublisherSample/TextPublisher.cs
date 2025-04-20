using RabbitMQ.Client;
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


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📡 Background Service is starting.");

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

                await Task.Delay(1000, stoppingToken); // هر ۵ ثانیه یک بار
            }

            _logger.LogInformation("🛑 Background Service is stopping.");
        }
    }
}
