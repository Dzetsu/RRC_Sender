using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storage.Repositories;
using Storage.Services;
using Storage.Services.BackgroundServices;
using Storage.Services.Kafka;
using Storage.Settings;

namespace Storage;

public abstract class Program()
{
    private static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("DefaultConnection")!);
        builder.Services.AddHostedService<Controller>();
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IStorageRepository, StorageRepository>();
        builder.Services.AddSingleton<StorageProducer>();
        builder.Services.Configure<KafkaConsumerSetting>(builder.Configuration.GetSection("ConsumerConfig"));
        builder.Services.Configure<KafkaProducerSetting>(builder.Configuration.GetSection("ProducerConfig"));
        
        var app = builder.Build();
        
        await app.RunAsync();
    }
}