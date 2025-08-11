using RabbitMQ.Client;
using satin_common;
using System.Text;

namespace satin_mq_send
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: mqtt_send.exe <message>");
                return;
            }
            string message = args[0];

            // Initialize logger and config
            var config = new Config();
            using var logger = new Logger();

            // read configurations
            string host = config.get("Host", "localhost");
            int port = int.Parse(config.get("Port", "8883"));
            string topic = config.get("Queue", "test");
            string username = config.get("Username", "test");
            string password = config.get("Password", "test");

            try
            {
                var factory = new ConnectionFactory { 
                    HostName = host,
                    Port = port,
                    UserName = username,
                    Password = password
                };
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                // Declare queue (durable, non-exclusive, not auto-delete)
                await channel.QueueDeclareAsync(
                    queue: topic,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var body = Encoding.UTF8.GetBytes(message);

                var properties = new BasicProperties
                {
                    Persistent = true
                };

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: topic,
                    mandatory: true,
                    basicProperties: properties,
                    body: body
                );

                logger.write($"Message '{message}' published to topic '{topic}'");
            }
            catch (Exception ex)
            {
                logger.write($"Error: {ex.Message}");
                return;
            }
        }
    }
}
