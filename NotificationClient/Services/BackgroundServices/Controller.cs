using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NotificationClient.Settings;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NotificationClient.Services.BackgroundServices;

public class Controller(IOptions<ConsumerKafkaSetting> consumerOptions, IOptions<TelegramBotSetting> botOptions, INotificationService service) : BackgroundService
{
    private readonly ConsumerKafkaSetting _consumerConfig = consumerOptions.Value ?? throw new ArgumentNullException(nameof(consumerOptions));
    private readonly TelegramBotSetting _botConfig = botOptions.Value ?? throw new ArgumentNullException(nameof(botOptions));
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botClient = new TelegramBotClient(_botConfig.BotToken);
        
        var conf = new ConsumerConfig
        {
            GroupId = _consumerConfig.GroupId,
            BootstrapServers = _consumerConfig.BootstrapServers,   
            EnableAutoCommit = _consumerConfig.EnableAutoCommit,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_consumerConfig.AutoOffsetReset, true)
        };
        
        using var consumer = new ConsumerBuilder<Ignore, string>(conf).Build();
        consumer.Subscribe(_consumerConfig.Topic);
        
        while (true)
        {
            try
            {
                var consumeResult = consumer.Consume(new TimeSpan(0, 0, 10));
                if (consumeResult == null) throw new TimeoutException("Timed out waiting for consumeResult");
                
                var telegramMessage = JsonSerializer.Deserialize<TelegramMessage>(consumeResult.Message.Value);
                if (telegramMessage == null) throw new NullReferenceException("Message is null");
                
                string messageTextConfirmed = $"Заказ на {telegramMessage.Amount} {telegramMessage.Name} успешно обратон и ему присвоен номер: {telegramMessage.Id}!";
                string messageTextDenied = $"Заказ на {telegramMessage.Amount} {telegramMessage.Name} с Id: {telegramMessage.Id} отменен!";
                
                if (telegramMessage.Status)
                {
                    await SendTextMessage(botClient, messageTextConfirmed);
                }
                else
                {
                    await SendTextMessage(botClient, messageTextDenied);
                }
                
                await service.AddOrderMessage(telegramMessage);
                consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

    private async Task SendTextMessage(ITelegramBotClient botClient, string messageText)
    {
        var message = await botClient.SendMessage(
            chatId: _botConfig.ChatId,
            text: messageText,
            parseMode: ParseMode.Markdown
        );
    }
}