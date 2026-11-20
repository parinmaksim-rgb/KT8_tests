using System.Text.Json.Serialization;

namespace Petstore.XUnitTests.Models;

public class Order
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("petId")]
    public long PetId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("shipDate")]
    public DateTime ShipDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("complete")]
    public bool Complete { get; set; }
}
