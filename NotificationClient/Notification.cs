using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NotificationClient;

class Notification
{
    static async Task Main()
    {
        var bootstrapServers = "localhost:9092";
        
        var conf = new ConsumerConfig
        {
            GroupId = "TgMessage",
            BootstrapServers = bootstrapServers,   
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Latest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(conf).Build();
        consumer.Subscribe("TelegramBot");

        var saverOrderId = new HashSet<long>();
        
        while (true)
        {
            try
            {
                var consumeResult = consumer.Consume(new TimeSpan(0, 0, 10));
                
                if (consumeResult == null)
                    throw new TimeoutException("Timed out waiting for consumeResult");
                
                var mes = JsonSerializer.Deserialize<MesTg>(consumeResult.Message.Value);

                if (mes == null)
                    throw new NullReferenceException("Message is null");
                
                if (saverOrderId.Contains(mes.Id))
                {
                    continue;
                }

                string botToken = "7603428024:AAEKsVb_pNvAJ3KGqBW3w8aJxxaikZVqzLI";
                string chatId = "1620966794";

                var botClient = new TelegramBotClient(botToken);

                string messageText =
                    $"Заказ на {mes.Amount} {mes.Name} успешно обратон и ему присвоен номер {mes.Id}";

                try
                {
                    var message = await botClient.SendMessage(
                        chatId: chatId,
                        text: messageText,
                        parseMode: ParseMode.Markdown
                    );
                    
                    saverOrderId.Add(mes.Id);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error! ID: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

public class MesTg
{
    public string Name { get; set; }
    public long Amount { get; set; }
    public long Id { get; set; }
} 