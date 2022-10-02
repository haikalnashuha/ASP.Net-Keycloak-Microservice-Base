using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace App1.RabbitMq
{
    public interface IMessageProducer
    {
        void SendMessage<T>(T message); 
    }

    public class Producer : IMessageProducer
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private ConnectionFactory _factory;
        private IModel _channel;
        public Producer(IConfiguration configuration)
        {
            _configuration = configuration;

            _factory = new ConnectionFactory { HostName = _configuration["RabbitMq:Server"] };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void SendMessage<T>(T message)
        {
            _channel.QueueDeclare(queue: "forApp2", durable: false, exclusive: false, autoDelete: false, arguments: null); //QueueDeclare("forApp2");

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            
            _channel.BasicPublish(exchange: "", routingKey: "forApp2", body: body);
        }
    }
}
