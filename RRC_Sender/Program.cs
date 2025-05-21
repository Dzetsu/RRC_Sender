using RRC_Sender.Entities;
using RRC_Sender.Repositories;
using RRC_Sender.Services;
using RRC_Sender.Services.BackGroundServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddHostedService<InfoSender>();
builder.Services.AddHostedService<ConfirmClass>();
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("ConsumerConfirmKafka"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();