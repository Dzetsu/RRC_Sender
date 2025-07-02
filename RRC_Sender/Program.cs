using RRC_Sender.Entities;
using RRC_Sender.Repositories;
using RRC_Sender.Services;
using RRC_Sender.Services.BackgroundServices;
using RRC_Sender.Services.BackGroundServices;
using RRC_Sender.Services.Kafka;

var builder = WebApplication.CreateBuilder(args);

//Для продюсеров outbox
//Везде добавить IOption
//Везде добавить appsetting.json

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddHostedService<KafkaOutboxSender>();
builder.Services.AddHostedService<StatusChanger>();
builder.Services.Configure<KafkaSetting>(builder.Configuration.GetSection("ConsumerConfirmKafka"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();