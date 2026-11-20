using System.Net;
using System.Net.Http.Json;
using Petstore.NUnitTests.Constants;
using Petstore.NUnitTests.Fixtures;
using Petstore.NUnitTests.Models;

namespace Petstore.NUnitTests.Tests;

[TestFixture]
public class PetTests : PetstoreTestBase
{
    [Test]
    public async Task CreatePet_ValidPayload_ReturnsOkAndMatchesData()
    {
        Pet pet = BuildPet("Rex", ApiConstants.StatusAvailable);

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? created = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Id, Is.EqualTo(pet.Id));
        Assert.That(created.Name, Is.EqualTo(pet.Name));
        Assert.That(created.Status, Is.EqualTo(pet.Status));
    }

    [Test]
    public async Task CreatePet_WithCategoryAndTags_PreservesNestedFields()
    {
        Pet pet = BuildPet("Bella", ApiConstants.StatusAvailable);
        pet.Category = new Category { Id = ApiConstants.DefaultCategoryId, Name = "Dogs" };
        pet.Tags = new List<Tag> { new() { Id = ApiConstants.DefaultTagId, Name = "vaccinated" } };

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? created = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Category, Is.Not.Null);
        Assert.That(created.Category!.Name, Is.EqualTo("Dogs"));
        Assert.That(created.Tags, Has.Count.EqualTo(1));
        Assert.That(created.Tags[0].Name, Is.EqualTo("vaccinated"));
    }

    [Test]
    public async Task CreatePet_WithMultiplePhotoUrls_ReturnsAllUrls()
    {
        Pet pet = BuildPet("Luna", ApiConstants.StatusPending);
        pet.PhotoUrls = new List<string>
        {
            "https://example.com/photos/luna-1.jpg",
            "https://example.com/photos/luna-2.jpg"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? created = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.PhotoUrls, Has.Count.EqualTo(2));
        Assert.That(created.PhotoUrls, Does.Contain("https://example.com/photos/luna-1.jpg"));
        Assert.That(created.PhotoUrls, Does.Contain("https://example.com/photos/luna-2.jpg"));
    }

    [TestCase(ApiConstants.StatusAvailable)]
    [TestCase(ApiConstants.StatusPending)]
    [TestCase(ApiConstants.StatusSold)]
    public async Task CreatePet_ParameterizedStatuses_ReturnsExpectedStatus(string status)
    {
        Pet pet = BuildPet($"Pet_{status}", status);

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? created = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Status, Is.EqualTo(status));
    }

    [Test]
    public async Task GetPetById_ExistingPet_ReturnsOk()
    {
        Pet pet = BuildPet("Charlie", ApiConstants.StatusAvailable);
        await Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);

        HttpResponseMessage response = await GetWithRetryAsync($"{ApiConstants.PetEndpoint}/{pet.Id}");
        Pet? fetched = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Id, Is.EqualTo(pet.Id));
        Assert.That(fetched.Name, Is.EqualTo(pet.Name));
    }

    [Test]
    public async Task GetPetById_NonExistentId_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.PetEndpoint}/{ApiConstants.NonExistentPetId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPetById_NegativeId_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.PetEndpoint}/{ApiConstants.NegativePetId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdatePet_ExistingPet_UpdatesNameAndStatus()
    {
        Pet pet = BuildPet("Milo", ApiConstants.StatusAvailable);
        await Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);

        pet.Name = "MiloUpdated";
        pet.Status = ApiConstants.StatusSold;

        HttpResponseMessage response = await Client.PutAsJsonAsync(ApiConstants.PetEndpoint, pet);
        Pet? updated = await response.Content.ReadFromJsonAsync<Pet>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("MiloUpdated"));
        Assert.That(updated.Status, Is.EqualTo(ApiConstants.StatusSold));
    }

    [Test]
    public async Task UpdatePetWithFormData_ExistingPet_ReturnsOk()
    {
        Pet pet = BuildPet("Oscar", ApiConstants.StatusAvailable);
        await Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);

        using FormUrlEncodedContent formContent = new(new Dictionary<string, string>
        {
            ["name"] = "OscarRenamed",
            ["status"] = ApiConstants.StatusPending
        });

        HttpResponseMessage response = await Client.PostAsync($"{ApiConstants.PetEndpoint}/{pet.Id}", formContent);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Code, Is.EqualTo(ApiConstants.ApiSuccessCode));
    }

    [TestCase(ApiConstants.StatusAvailable)]
    [TestCase(ApiConstants.StatusPending)]
    [TestCase(ApiConstants.StatusSold)]
    public async Task FindPetsByStatus_ValidStatuses_ReturnsOkAndCollection(string status)
    {
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.PetFindByStatusEndpoint}?status={status}");
        List<Pet>? pets = await response.Content.ReadFromJsonAsync<List<Pet>>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(pets, Is.Not.Null);
    }

    [Test]
    public async Task DeletePet_ExistingPet_ReturnsOk()
    {
        Pet pet = BuildPet("Rocky", ApiConstants.StatusAvailable);
        await Client.PostAsJsonAsync(ApiConstants.PetEndpoint, pet);
        await GetWithRetryAsync($"{ApiConstants.PetEndpoint}/{pet.Id}");

        HttpResponseMessage deleteResponse = await Client.DeleteAsync($"{ApiConstants.PetEndpoint}/{pet.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeletePet_NonExistentPet_ReturnsNotFound()
    {
        HttpResponseMessage response = await Client.DeleteAsync($"{ApiConstants.PetEndpoint}/{ApiConstants.NonExistentPetId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private Pet BuildPet(string name, string status)
    {
        return new Pet
        {
            Id = GenerateUniqueId(),
            Name = name,
            Status = status,
            PhotoUrls = new List<string> { "https://example.com/photos/default.jpg" }
        };
    }
}
