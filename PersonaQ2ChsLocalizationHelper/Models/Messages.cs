using System.Text.Json.Serialization;

namespace PQ2Helper.Models;

internal class Messages
{
  [JsonPropertyName("speakers")]
  public List<string> Speakers { get; set; } = [];

  [JsonPropertyName("texts")]
  public List<Message> Texts { get; set; } = [];
}

internal class Message
{
  [JsonPropertyName("id")]
  public string ID { get; set; } = "";

  [JsonPropertyName("speaker")]
  public string Speaker { get; set; } = "";

  [JsonPropertyName("lines")]
  public List<string> Lines { get; set; } = [];
}
