using System.Net;
using System.Net.Http.Json;
using Petstore.NUnitTests.Constants;
using Petstore.NUnitTests.Fixtures;
using Petstore.NUnitTests.Models;

namespace Petstore.NUnitTests.Tests;

[TestFixture]
public class StoreTests : PetstoreTestBase
{
    [Test]
    public async Task GetInventory_ReturnsOkAndNonEmptyDictionary()
    {
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.StoreInventoryEndpoint);
        Dictionary<string, int>? inventory = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inventory, Is.Not.Null);
        Assert.That(inventory, Is.Not.Empty);
    }

    [Test]
    public async Task PlaceOrder_ValidOrder_ReturnsOkAndMatchesPayload()
    {
        Order order = BuildOrder(ApiConstants.OrderStatusPlaced, ApiConstants.DefaultQuantity);

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);
        Order? created = await response.Content.ReadFromJsonAsync<Order>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Id, Is.EqualTo(order.Id));
        Assert.That(created.PetId, Is.EqualTo(order.PetId));
        Assert.That(created.Quantity, Is.EqualTo(order.Quantity));
        Assert.That(created.Status, Is.EqualTo(order.Status));
    }

    [TestCase(ApiConstants.OrderStatusPlaced)]
    [TestCase(ApiConstants.OrderStatusApproved)]
    [TestCase(ApiConstants.OrderStatusDelivered)]
    public async Task PlaceOrder_ParameterizedStatuses_ReturnsExpectedStatus(string status)
    {
        Order order = BuildOrder(status, ApiConstants.DefaultQuantity);

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);
        Order? created = await response.Content.ReadFromJsonAsync<Order>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Status, Is.EqualTo(status));
    }

    [TestCase(ApiConstants.DefaultQuantity)]
    [TestCase(ApiConstants.BulkQuantity)]
    public async Task PlaceOrder_ParameterizedQuantities_ReturnsExpectedQuantity(int quantity)
    {
        Order order = BuildOrder(ApiConstants.OrderStatusPlaced, quantity);

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);
        Order? created = await response.Content.ReadFromJsonAsync<Order>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Quantity, Is.EqualTo(quantity));
    }

    [Test]
    public async Task GetOrderById_ExistingOrder_ReturnsOk()
    {
        Order order = BuildOrder(ApiConstants.OrderStatusPlaced, ApiConstants.DefaultQuantity);
        await Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);

        HttpResponseMessage response = await GetWithRetryAsync($"{ApiConstants.StoreOrderEndpoint}/{order.Id}");
        Order? fetched = await response.Content.ReadFromJsonAsync<Order>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Id, Is.EqualTo(order.Id));
    }

    [Test]
    public async Task GetOrderById_NonExistentOrder_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.StoreOrderEndpoint}/{ApiConstants.NonExistentOrderId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteOrder_ExistingOrder_ReturnsOk()
    {
        Order order = BuildOrder(ApiConstants.OrderStatusPlaced, ApiConstants.DefaultQuantity);
        await Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);
        await GetWithRetryAsync($"{ApiConstants.StoreOrderEndpoint}/{order.Id}");

        HttpResponseMessage deleteResponse = await Client.DeleteAsync($"{ApiConstants.StoreOrderEndpoint}/{order.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeleteOrder_NonExistentOrder_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.DeleteAsync($"{ApiConstants.StoreOrderEndpoint}/{ApiConstants.NonExistentOrderId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private Order BuildOrder(string status, int quantity)
    {
        return new Order
        {
            Id = GenerateUniqueId(),
            PetId = GenerateUniqueId(),
            Quantity = quantity,
            ShipDate = DateTime.UtcNow,
            Status = status,
            Complete = true
        };
    }
}
