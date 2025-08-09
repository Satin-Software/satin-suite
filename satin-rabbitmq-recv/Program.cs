using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using satin_common;
using System.Text;

namespace satin_rabbitmq_recv
{
    public class Program
    {
        static async Task Main()
        {
            // Initialize logger and config
            var config = new Config();
            using var logger = new Logger();

            // read configurations
            // read configurations
            string host = config.get("Host", "localhost");
            int port = int.Parse(config.get("Port", "8883"));
            string topic = config.get("Queue", "test");
            string username = config.get("Username", "test");
            string password = config.get("Password", "test");

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = host,
                    Port = port,
                    UserName = username,
                    Password = password
                };
                using var connection = await factory.CreateConnectionAsync();
                using (var channel = await connection.CreateChannelAsync()) {
                    // Declare queue (durable, non-exclusive, not auto-delete)
                    await channel.QueueDeclareAsync(
                        queue: topic,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    // Use BasicGet to retrieve a single message
                    var result = await channel.BasicGetAsync(queue: topic, autoAck: false);

                    if (result != null)
                    {
                        byte[] body = result.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        logger.write($"Message '{message}' received on topic '{topic}'");

                        string file_name = $"{app_env.project_name}_{DateTime.Now:yyyyMMdd_HHmmss}.msg";
                        File.WriteAllText(Path.Combine(app_env.tmp_dir, file_name), message);

                        // Process your message here
                        await Task.Delay(1000);

                        // Acknowledge the message
                        await channel.BasicAckAsync(deliveryTag: result.DeliveryTag, multiple: false);
                    }

                    /* Below code works, but there is a possibility of racearound condition, which causes more than one message to be read and logged */
                    //await channel.BasicQosAsync(
                    //    prefetchSize: 0,
                    //    prefetchCount: 1,
                    //    global: false
                    //);

                    //var messageReceived = new TaskCompletionSource<bool>();

                    //var consumer = new AsyncEventingBasicConsumer(channel);
                    //consumer.ReceivedAsync += async (model, ea) =>
                    //{
                    //    byte[] body = ea.Body.ToArray();
                    //    var message = Encoding.UTF8.GetString(body);

                    //    logger.write($"Message '{message}' received on topic '{topic}'");

                    //    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);

                    //    await Task.Delay(1000);
                    //    messageReceived.SetResult(true); // Signal that processing is complete
                    //};

                    //var consumerTag = await channel.BasicConsumeAsync(topic, autoAck: false, consumer: consumer);

                    //try
                    //{
                    //    // Wait for one message with a timeout (e.g., 3 seconds)
                    //    await messageReceived.Task.WaitAsync(TimeSpan.FromSeconds(3));
                    //}
                    //catch (TimeoutException)
                    //{
                    //    // ignore timeout
                    //}
                    //finally
                    //{
                    //    // Ensure consumer is cancelled
                    //    await channel.BasicCancelAsync(consumerTag);
                    //}
                }

            }
            catch (Exception ex)
            {
                logger.write($"Error: {ex.Message}");
                return;
            }
        }
    }
}
