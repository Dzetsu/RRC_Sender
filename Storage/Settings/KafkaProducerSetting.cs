namespace Storage.Settings;

public class KafkaProducerSetting
{
    public string BootstrapServers { get; set; }
    public string TelegramTopic { get; set; }
    public string MainProgramTopic { get; set; }
}