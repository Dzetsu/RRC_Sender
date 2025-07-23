using Microsoft.Extensions.Options;

namespace RRC_Sender.Settings;

public class ProducerKafkaSettings
{
    public string BootstrapServers { get; set; }
}