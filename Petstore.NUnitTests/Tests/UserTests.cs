using System.Net;
using System.Net.Http.Json;
using Petstore.NUnitTests.Constants;
using Petstore.NUnitTests.Fixtures;
using Petstore.NUnitTests.Models;

namespace Petstore.NUnitTests.Tests;

[TestFixture]
public class UserTests : PetstoreTestBase
{
    [Test]
    public async Task CreateUser_ValidUser_ReturnsOk()
    {
        User user = BuildUser("nunit_create");

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Code, Is.EqualTo(ApiConstants.ApiSuccessCode));
    }

    [Test]
    public async Task CreateUsersWithArray_ValidUsers_ReturnsOk()
    {
        User[] users = new[]
        {
            BuildUser("nunit_arr_1"),
            BuildUser("nunit_arr_2")
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.UserCreateWithArrayEndpoint, users);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Code, Is.EqualTo(ApiConstants.ApiSuccessCode));
    }

    [Test]
    public async Task CreateUsersWithList_ValidUsers_ReturnsOk()
    {
        List<User> users = new()
        {
            BuildUser("nunit_list_1"),
            BuildUser("nunit_list_2")
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(ApiConstants.UserCreateWithListEndpoint, users);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Code, Is.EqualTo(ApiConstants.ApiSuccessCode));
    }

    [Test]
    public async Task GetUserByUsername_ExistingUser_ReturnsOkAndMatchesUsername()
    {
        User user = BuildUser("nunit_get");
        await Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);

        HttpResponseMessage response = await GetWithRetryAsync($"{ApiConstants.UserEndpoint}/{user.Username}");
        User? fetched = await response.Content.ReadFromJsonAsync<User>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Username, Is.EqualTo(user.Username));
        Assert.That(fetched.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public async Task GetUserByUsername_NonExistentUser_ReturnsNotFound()
    {
        string missingUsername = $"{ApiConstants.NonExistentUsernamePrefix}{GenerateUniqueId()}";

        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.UserEndpoint}/{missingUsername}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateUser_ExistingUser_ReturnsOk()
    {
        User user = BuildUser("nunit_update");
        await Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);

        user.FirstName = "UpdatedName";
        user.Email = "updated@example.com";

        HttpResponseMessage response = await Client.PutAsJsonAsync($"{ApiConstants.UserEndpoint}/{user.Username}", user);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Code, Is.EqualTo(ApiConstants.ApiSuccessCode));
    }

    [Test]
    public async Task LoginUser_ValidCredentials_ReturnsOkAndSessionMessage()
    {
        User user = BuildUser("nunit_login");
        await Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);

        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.UserLoginEndpoint}?username={user.Username}&password={user.Password}");
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Message, Does.Contain(ApiConstants.LoginSessionMarker));
    }

    [TestCase("demo_user_alpha", "Pass1234!")]
    [TestCase("demo_user_beta", "Pass5678!")]
    public async Task LoginUser_ParameterizedCredentials_ReturnsOkAndSessionMessage(string username, string password)
    {
        HttpResponseMessage response = await Client.GetAsync($"{ApiConstants.UserLoginEndpoint}?username={username}&password={password}");
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Message, Does.Contain(ApiConstants.LoginSessionMarker));
    }

    [Test]
    public async Task LogoutUser_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsync(ApiConstants.UserLogoutEndpoint);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Code, Is.EqualTo(ApiConstants.ApiSuccessCode));
    }

    [Test]
    public async Task DeleteUser_ExistingUser_ReturnsOk()
    {
        User user = BuildUser("nunit_delete");
        await Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);
        await GetWithRetryAsync($"{ApiConstants.UserEndpoint}/{user.Username}");

        HttpResponseMessage response = await Client.DeleteAsync($"{ApiConstants.UserEndpoint}/{user.Username}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    private User BuildUser(string prefix)
    {
        long id = GenerateUniqueId();
        string username = GenerateUniqueUsername(prefix);
        return new User
        {
            Id = id,
            Username = username,
            FirstName = "Ivan",
            LastName = "Petrov",
            Email = $"{username}@example.com",
            Password = "Password123!",
            Phone = "+79990001122",
            UserStatus = ApiConstants.DefaultUserStatus
        };
    }
}
