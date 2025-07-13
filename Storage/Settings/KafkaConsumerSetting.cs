namespace Storage.Settings;

public class KafkaConsumerSetting
{
    public string GroupId { get; set; }
    public string BootstrapServers { get; set; }
    public bool EnableAutoCommit { get; set; }
    public string AutoOffsetReset { get; set; }
    public string Topic { get; set; }
}