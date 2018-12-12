using System;
using System.Diagnostics;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc;
using RedisRabbitmqDotnetCoreWriter.Models;
using RabbitMQ.Client;
using System.Text;

namespace RedisRabbitmqDotnetCoreWriter.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            Console.Out.WriteLine("Index page rendered.");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index([Bind("Key,Value")] Message message)
        {
            // Redis Write
            try
            {
                var redisHost = Environment.GetEnvironmentVariable("REDIS_HOSTNAME");
                Console.Out.WriteLine("REDIS_HOSTNAME = {0}", redisHost);
                var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT");
                Console.Out.WriteLine("REDIS_PORT = {0}", redisPort);
                if (redisHost == null)
                {
                    throw new Exception("REDIS_HOSTNAME Environment variable is not defined!");
                }
                if (redisPort == null)
                {
                    throw new Exception("REDIS_PORT Environment variable is not defined!");
                }
                ConnectionMultiplexer redis = Redis.GetInstance(redisHost, Int32.Parse(redisPort));
                Console.Out.WriteLine("Got redis connection multiplexer.");
                IDatabase db = redis.GetDatabase(0);
                Console.Out.WriteLine("Connected to redis database 0.");
                db.StringSet(message.Key, message.Value);
                Console.Out.WriteLine($"Wrote to Redis:\nKey: {message.Key}\tValue: {message.Value}");

            }
            catch (Exception e)
            {
                Console.Out.WriteLine("{0} \n\nException caught.", e);
            }
            // RabbitMQ Send
            try
            {
                var rabbitHost = Environment.GetEnvironmentVariable("RABBIT_MQ_HOSTNAME");
                Console.Out.WriteLine("RABBIT_MQ_HOSTNAME = {0}", rabbitHost);
                var rabbitPort = Environment.GetEnvironmentVariable("RABBIT_MQ_PORT");
                Console.Out.WriteLine("RABBIT_MQ_HOSTNAME = {0}", rabbitPort);
                if (rabbitHost == null)
                {
                    throw new Exception("RABBIT_MQ_HOSTNAME Environment variable is not defined!");
                }
                if (rabbitPort == null)
                {
                    throw new Exception("RABBIT_MQ_HOSTNAME Environment variable is not defined!");
                }

                string queue = Environment.GetEnvironmentVariable("env_queue");
                if (queue == null) queue = "queue";
                var factory = new ConnectionFactory() { HostName = rabbitHost, Port = Int32.Parse(rabbitPort) };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: queue,
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);
                        var body = Encoding.UTF8.GetBytes(message.Value);

                        channel.BasicPublish(exchange: "",
                            routingKey: queue,
                            basicProperties: null,
                            body: body);
                        Console.Out.WriteLine("Sent to RabbitMQ: {0}", message.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("{0} \n\nException caught.", e);
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
