namespace RRC_Sender.Entities;

public class KafkaSettings
{
    public string GroupId { get; set; }
    public string BootstrapServers { get; set; }
    public bool EnableAutoCommit { get; set; }
    public string AutoOffsetReset { get; set; }
}