using App1.Models;
using App1.RabbitMq;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<Producer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

void SendDataToApp2()
{ 
    Console.WriteLine("Sending data to App2");
    var producer = builder.Services.BuildServiceProvider().GetService<Producer>();

    Test t = new Test { 
       Message = "u wot m8?",
       Title = "test"
    };
    producer.SendMessage(t);
}

//app.UseHttpsRedirection();
app.MapGet("/", () => "Hello World from App1");
app.MapGet("/send", SendDataToApp2);
app.Run();

