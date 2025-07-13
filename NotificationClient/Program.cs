using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationClient.Repositories;
using NotificationClient.Services;
using NotificationClient.Services.BackgroundServices;
using NotificationClient.Settings;

namespace NotificationClient;

class Program
{
    static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(5001); 
        });
        
        builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("DefaultConnection")!);
        builder.Services.AddHostedService<Controller>();
        builder.Services.AddSingleton<INotificationRepository, NotificationRepository>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.Configure<ConsumerKafkaSetting>(builder.Configuration.GetSection("ConsumerConfig"));
        builder.Services.Configure<TelegramBotSetting>(builder.Configuration.GetSection("TelegramBotConfig"));
        
        var app = builder.Build();
        
        await app.RunAsync();
    }
}