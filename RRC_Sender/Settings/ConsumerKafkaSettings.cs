namespace RRC_Sender.Settings;

public class ConsumerKafkaSettings
{
    public string GroupId { get; set; }
    public string BootstrapServers { get; set; }
    public bool EnableAutoCommit { get; set; }
    public string AutoOffsetReset { get; set; }
}