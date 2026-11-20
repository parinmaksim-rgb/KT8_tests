using System.Text.Json.Serialization;

namespace Petstore.NUnitTests.Models;

public class Tag
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
