using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata;
using PlatformService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient: IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;


        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeRabbitMQ();
        }
        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };
            try
            {
                _connection = factory.CreateConnectionAsync().Result;
                _channel =  _connection.CreateChannelAsync().Result;
                _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
                _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
                Console.WriteLine("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connection to the Message Bus: {ex.Message}");
            }
        }
        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);
            if (_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Connection Open, sending.. message");
                SendMessage(message);
            } else
            {
                Console.WriteLine("--> RabbitMQ Connection Shutdown");

            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublishAsync(exchange: "trigger",
                routingKey: "",
                body: body);
            Console.WriteLine($"--> Wa have sent {message}");
        }
        public void Dispose()
        {
            Console.WriteLine("MessageBus Dispose");
            if (_connection.IsOpen)
            {
                _channel.CloseAsync();
                _connection.CloseAsync();
            }
        }
        private Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connecton Shutdown");
            return Task.CompletedTask;
        }
    }
}
