using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SV2.VoopAI;
public class ValourConfig
{
    [JsonPropertyName("botpassword")]
    public string BotPassword { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("prefix")]
    public List<string> Prefix { get; set; }
    [JsonPropertyName("prod")]
    public bool Production { get; set; }
}