using Microsoft.Extensions.Options;

namespace RRC_Sender.Settings;

public class ProducerKafkaConfig
{
    public string BootstrapServers { get; set; }
}