using System.Net;
using System.Net.Http.Json;
using Petstore.XUnitTests.Constants;
using Petstore.XUnitTests.Fixtures;
using Petstore.XUnitTests.Models;

namespace Petstore.XUnitTests.Tests;

public class PetTests : IClassFixture<PetstoreApiFixture>
{
    private readonly PetstoreApiFixture _fixture;

    public PetTests(PetstoreApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreatePet_ValidPayload_ReturnsOkAndMatchesData()
    {
        Pet pet = BuildPet("Rex", ApiConstants.StatusAvailable);

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? created = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(pet.Id, created.Id);
        Assert.Equal(pet.Name, created.Name);
        Assert.Equal(pet.Status, created.Status);
    }

    [Fact]
    public async Task CreatePet_WithCategoryAndTags_PreservesNestedFields()
    {
        Pet pet = BuildPet("Bella", ApiConstants.StatusAvailable);
        pet.Category = new Category { Id = ApiConstants.DefaultCategoryId, Name = "Dogs" };
        pet.Tags = new List<Tag> { new() { Id = ApiConstants.DefaultTagId, Name = "vaccinated" } };

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? created = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(created);
        Assert.NotNull(created.Category);
        Assert.Equal("Dogs", created.Category.Name);
        Assert.Single(created.Tags);
        Assert.Equal("vaccinated", created.Tags[0].Name);
    }

    [Fact]
    public async Task CreatePet_WithMultiplePhotoUrls_ReturnsAllUrls()
    {
        Pet pet = BuildPet("Luna", ApiConstants.StatusPending);
        pet.PhotoUrls = new List<string>
        {
            "https://example.com/photos/luna-1.jpg",
            "https://example.com/photos/luna-2.jpg"
        };

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? created = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(2, created.PhotoUrls.Count);
        Assert.Contains("https://example.com/photos/luna-1.jpg", created.PhotoUrls);
        Assert.Contains("https://example.com/photos/luna-2.jpg", created.PhotoUrls);
    }

    [Theory]
    [InlineData(ApiConstants.StatusAvailable)]
    [InlineData(ApiConstants.StatusPending)]
    [InlineData(ApiConstants.StatusSold)]
    public async Task CreatePet_ParameterizedStatuses_ReturnsExpectedStatus(string status)
    {
        Pet pet = BuildPet($"Pet_{status}", status);

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? created = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(status, created.Status);
    }

    [Fact]
    public async Task GetPetById_ExistingPet_ReturnsOk()
    {
        Pet pet = BuildPet("Charlie", ApiConstants.StatusAvailable);
        await _fixture.Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);

        HttpResponseMessage response = await _fixture.GetWithRetryAsync($"{ApiConstants.PetEndpoint}/{pet.Id}");
        Pet? fetched = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(fetched);
        Assert.Equal(pet.Id, fetched.Id);
        Assert.Equal(pet.Name, fetched.Name);
    }

    [Fact]
    public async Task GetPetById_NonExistentId_ReturnsNotFound()
    {
        HttpResponseMessage response = await _fixture.Client.GetAsync($"{ApiConstants.PetEndpoint}/{ApiConstants.NonExistentPetId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPetById_NegativeId_ReturnsNotFound()
    {
        HttpResponseMessage response = await _fixture.Client.GetAsync($"{ApiConstants.PetEndpoint}/{ApiConstants.NegativePetId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePet_ExistingPet_UpdatesNameAndStatus()
    {
        Pet pet = BuildPet("Milo", ApiConstants.StatusAvailable);
        await _fixture.Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);

        pet.Name = "MiloUpdated";
        pet.Status = ApiConstants.StatusSold;

        HttpResponseMessage response = await _fixture.Client.PutAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? updated = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("MiloUpdated", updated.Name);
        Assert.Equal(ApiConstants.StatusSold, updated.Status);
    }

    [Fact]
    public async Task UpdatePetWithFormData_ExistingPet_ReturnsOk()
    {
        Pet pet = BuildPet("Oscar", ApiConstants.StatusAvailable);
        await _fixture.Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);

        using FormUrlEncodedContent formContent = new(new Dictionary<string, string>
        {
            ["name"] = "OscarRenamed",
            ["status"] = ApiConstants.StatusPending
        });

        HttpResponseMessage response = await _fixture.Client.PostAsync($"{ApiConstants.PetEndpoint}/{pet.Id}", formContent);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(apiResponse);
        Assert.Equal(ApiConstants.ApiSuccessCode, apiResponse.Code);
    }

    [Theory]
    [InlineData(ApiConstants.StatusAvailable)]
    [InlineData(ApiConstants.StatusPending)]
    [InlineData(ApiConstants.StatusSold)]
    public async Task FindPetsByStatus_ValidStatuses_ReturnsOkAndCollection(string status)
    {
        HttpResponseMessage response = await _fixture.Client.GetAsync($"{ApiConstants.PetFindByStatusEndpoint}?status={status}");
        List<Pet>? pets = await response.Content.ReadFromJsonAsync<List<Pet>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(pets);
    }

    [Fact]
    public async Task DeletePet_ExistingPet_ReturnsOk()
    {
        Pet pet = BuildPet("Rocky", ApiConstants.StatusAvailable);
        await _fixture.Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        await _fixture.GetWithRetryAsync($"{ApiConstants.PetEndpoint}/{pet.Id}");

        HttpResponseMessage deleteResponse = await _fixture.Client.DeleteAsync($"{ApiConstants.PetEndpoint}/{pet.Id}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeletePet_NonExistentPet_ReturnsNotFound()
    {
        HttpResponseMessage response = await _fixture.Client.DeleteAsync($"{ApiConstants.PetEndpoint}/{ApiConstants.NonExistentPetId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Pet BuildPet(string name, string status)
    {
        return new Pet
        {
            Id = _fixture.GenerateUniqueId(),
            Name = name,
            Status = status,
            PhotoUrls = new List<string> { "https://example.com/photos/default.jpg" }
        };
    }
}
