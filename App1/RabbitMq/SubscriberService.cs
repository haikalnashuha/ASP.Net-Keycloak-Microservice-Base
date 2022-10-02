using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace App1.RabbitMq
{
    public class SubscriberService : BackgroundService
    {
        private IServiceProvider _sp;
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private IConfiguration _configuration;

        // initialize the connection, channel and queue 
        // inside the constructor to persist them 
        // for until the service (or the application) runs
        public SubscriberService(IServiceProvider sp, IConfiguration configuration)
        {
            _configuration = configuration;
            _sp = sp;
            var hostname = _configuration["RabbitMq:Server"];
            _factory = new ConnectionFactory() { HostName = hostname };

            _connection = _factory.CreateConnection();

            _channel = _connection.CreateModel();

            //_channel.QueueDeclare(
            //    queue: "forApp2",
            //    durable: false,
            //    exclusive: false,
            //    autoDelete: false,
            //    arguments: null);
            _channel.QueueDeclare(queue: _configuration["RabbitMq:App1Queue"], durable: false, exclusive: false, autoDelete: false, arguments: null);
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("HAIKAL SubscriberService Executing");
            // when the service is stopping
            // dispose these references
            // to prevent leaks
            if (stoppingToken.IsCancellationRequested)
            {
                _channel.Dispose();
                _connection.Dispose();
                return Task.CompletedTask;
            }

            // create a consumer that listens on the channel (queue)
            var consumer = new EventingBasicConsumer(_channel);

            // handle the Received event on the consumer
            // this is triggered whenever a new message
            // is added to the queue by the producer
            consumer.Received += (model, ea) =>
            {
                Console.WriteLine("Message delivered");
                Console.WriteLine("Message delivered");
                // read the message bytes
                var body = ea.Body.ToArray();

                // convert back to the original string
                // {index}|SuperHero{10000+index}|Fly,Eat,Sleep,Manga|1|{DateTime.UtcNow.ToLongDateString()}|0|0
                // is received here
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine(" [x] Received {0}", message);


                Task.Run(() =>
                {
                    // split the incoming message
                    // into chunks which are inserted
                    // into respective columns of the Heroes table
                    var chunks = message.Split("|");
                    Console.WriteLine("Message delivered");
                    foreach (var chunk in chunks)
                    {
                        Console.WriteLine(chunk);
                    }
                    Console.WriteLine("Message ends");
                    //var hero = new Hero();
                    //if (chunks.Length == 7)
                    //{
                    //    hero.Name = chunks[1];
                    //    hero.Powers = chunks[2];
                    //    hero.HasCape = chunks[3] == "1";
                    //    hero.IsAlive = chunks[5] == "1";
                    //    hero.Category = Enum.Parse<Category>(chunks[6]);
                    //}

                    // BackgroundService is a Singleton service
                    // IHeroesRepository is declared a Scoped service
                    // by definition a Scoped service can't be consumed inside a Singleton
                    // to solve this, we create a custom scope inside the Singleton and 
                    // perform the insertion.
                    using (var scope = _sp.CreateScope())
                    {
                        //Some Db activity here
                        //var db = scope.ServiceProvider.GetRequiredService<IHeroesRepository>();
                        //db.Create(hero);
                    }
                });
            };

            _channel.BasicConsume(queue: _configuration["RabbitMq:App1Queue"], autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}

