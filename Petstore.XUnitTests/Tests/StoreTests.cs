using System.Net;
using System.Net.Http.Json;
using Petstore.XUnitTests.Constants;
using Petstore.XUnitTests.Fixtures;
using Petstore.XUnitTests.Models;

namespace Petstore.XUnitTests.Tests;

public class StoreTests : IClassFixture<PetstoreApiFixture>
{
    private readonly PetstoreApiFixture _fixture;

    public StoreTests(PetstoreApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetInventory_ReturnsOkAndNonEmptyDictionary()
    {
        HttpResponseMessage response = await _fixture.Client.GetAsync(ApiConstants.StoreInventoryEndpoint);
        Dictionary<string, int>? inventory = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(inventory);
        Assert.NotEmpty(inventory);
    }

    [Fact]
    public async Task PlaceOrder_ValidOrder_ReturnsOkAndMatchesPayload()
    {
        Order order = BuildOrder(ApiConstants.OrderStatusPlaced, ApiConstants.DefaultQuantity);

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);
        Order? created = await response.Content.ReadFromJsonAsync<Order>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(order.Id, created.Id);
        Assert.Equal(order.PetId, created.PetId);
        Assert.Equal(order.Quantity, created.Quantity);
        Assert.Equal(order.Status, created.Status);
    }

    [Theory]
    [InlineData(ApiConstants.OrderStatusPlaced)]
    [InlineData(ApiConstants.OrderStatusApproved)]
    [InlineData(ApiConstants.OrderStatusDelivered)]
    public async Task PlaceOrder_ParameterizedStatuses_ReturnsExpectedStatus(string status)
    {
        Order order = BuildOrder(status, ApiConstants.DefaultQuantity);

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);
        Order? created = await response.Content.ReadFromJsonAsync<Order>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(status, created.Status);
    }

    [Theory]
    [InlineData(ApiConstants.DefaultQuantity)]
    [InlineData(ApiConstants.BulkQuantity)]
    public async Task PlaceOrder_ParameterizedQuantities_ReturnsExpectedQuantity(int quantity)
    {
        Order order = BuildOrder(ApiConstants.OrderStatusPlaced, quantity);

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);
        Order? created = await response.Content.ReadFromJsonAsync<Order>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(quantity, created.Quantity);
    }

    [Fact]
    public async Task GetOrderById_ExistingOrder_ReturnsOk()
    {
        Order order = BuildOrder(ApiConstants.OrderStatusPlaced, ApiConstants.DefaultQuantity);
        await _fixture.Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);

        HttpResponseMessage response = await _fixture.GetWithRetryAsync($"{ApiConstants.StoreOrderEndpoint}/{order.Id}");
        Order? fetched = await response.Content.ReadFromJsonAsync<Order>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(fetched);
        Assert.Equal(order.Id, fetched.Id);
    }

    [Fact]
    public async Task GetOrderById_NonExistentOrder_ReturnsNotFound()
    {
        HttpResponseMessage response = await _fixture.Client.GetAsync($"{ApiConstants.StoreOrderEndpoint}/{ApiConstants.NonExistentOrderId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOrder_ExistingOrder_ReturnsOk()
    {
        Order order = BuildOrder(ApiConstants.OrderStatusPlaced, ApiConstants.DefaultQuantity);
        await _fixture.Client.PostAsJsonAsync(ApiConstants.StoreOrderEndpoint, order);
        await _fixture.GetWithRetryAsync($"{ApiConstants.StoreOrderEndpoint}/{order.Id}");

        HttpResponseMessage deleteResponse = await _fixture.Client.DeleteAsync($"{ApiConstants.StoreOrderEndpoint}/{order.Id}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteOrder_NonExistentOrder_ReturnsNotFound()
    {
        HttpResponseMessage response = await _fixture.Client.DeleteAsync($"{ApiConstants.StoreOrderEndpoint}/{ApiConstants.NonExistentOrderId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Order BuildOrder(string status, int quantity)
    {
        return new Order
        {
            Id = _fixture.GenerateUniqueId(),
            PetId = _fixture.GenerateUniqueId(),
            Quantity = quantity,
            ShipDate = DateTime.UtcNow,
            Status = status,
            Complete = true
        };
    }
}
