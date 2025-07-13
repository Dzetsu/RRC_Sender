using RRC_Sender.Repositories;
using RRC_Sender.Services;
using RRC_Sender.Services.BackgroundServices;
using RRC_Sender.Services.BackGroundServices;
using RRC_Sender.Services.Kafka;
using RRC_Sender.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddHostedService<KafkaOutboxSender>();
builder.Services.AddHostedService<StatusChanger>();
builder.Services.Configure<ConsumerKafkaSetting>(builder.Configuration.GetSection("ConsumerConfig"));
builder.Services.Configure<ProducerKafkaConfig>(builder.Configuration.GetSection("ProducerConfig"));
builder.Services.AddSingleton<KafkaProducer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();