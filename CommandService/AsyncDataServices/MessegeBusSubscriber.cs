﻿
using CommandService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace CommandService.AsyncDataServices
{
    public class MessegeBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IChannel _channel;
        private string _queueName;

        public MessegeBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;
            InitializeRabbitMQ();
        }
        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"]),
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;
            _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
            _queueName = _channel.QueueDeclareAsync().Result.QueueName;
            _channel.QueueBindAsync(
                queue: _queueName,
                exchange: "trigger",
                routingKey: ""
                );
            Console.WriteLine("--> Listening on the Messege Bus...");

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdown;
        }
        private Task RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connecton Shutdown");
            return Task.CompletedTask;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += (ModuleHandle, ea) =>
            {
                Console.WriteLine("--> Event Receirved!");

                var body = ea.Body;
                var notificationMessege = Encoding.UTF8.GetString(body.ToArray());

                _eventProcessor.ProcessEvent(notificationMessege);
                return Task.CompletedTask;
            };

            _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);
            
            return Task.CompletedTask;
        }
        public override void Dispose()
        {
            
            if (_connection.IsOpen)
            {
                _channel.CloseAsync();
                _connection.CloseAsync();

            }
            base.Dispose();
        }
    }
}
