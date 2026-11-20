using System.Net;
using System.Net.Http.Json;
using Petstore.XUnitTests.Constants;
using Petstore.XUnitTests.Fixtures;
using Petstore.XUnitTests.Models;

namespace Petstore.XUnitTests.Tests;

public class UserTests : IClassFixture<PetstoreApiFixture>
{
    private readonly PetstoreApiFixture _fixture;

    public UserTests(PetstoreApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateUser_ValidUser_ReturnsOk()
    {
        User user = BuildUser("xunit_create");

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(apiResponse);
        Assert.Equal(ApiConstants.ApiSuccessCode, apiResponse.Code);
    }

    [Fact]
    public async Task CreateUsersWithArray_ValidUsers_ReturnsOk()
    {
        User[] users = new[]
        {
            BuildUser("xunit_arr_1"),
            BuildUser("xunit_arr_2")
        };

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.UserCreateWithArrayEndpoint, users);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(apiResponse);
        Assert.Equal(ApiConstants.ApiSuccessCode, apiResponse.Code);
    }

    [Fact]
    public async Task CreateUsersWithList_ValidUsers_ReturnsOk()
    {
        List<User> users = new()
        {
            BuildUser("xunit_list_1"),
            BuildUser("xunit_list_2")
        };

        HttpResponseMessage response = await _fixture.Client.PostAsJsonAsync(ApiConstants.UserCreateWithListEndpoint, users);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(apiResponse);
        Assert.Equal(ApiConstants.ApiSuccessCode, apiResponse.Code);
    }

    [Fact]
    public async Task GetUserByUsername_ExistingUser_ReturnsOkAndMatchesUsername()
    {
        User user = BuildUser("xunit_get");
        await _fixture.Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);

        HttpResponseMessage response = await _fixture.GetWithRetryAsync($"{ApiConstants.UserEndpoint}/{user.Username}");
        User? fetched = await response.Content.ReadFromJsonAsync<User>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(fetched);
        Assert.Equal(user.Username, fetched.Username);
        Assert.Equal(user.Email, fetched.Email);
    }

    [Fact]
    public async Task GetUserByUsername_NonExistentUser_ReturnsNotFound()
    {
        string missingUsername = $"{ApiConstants.NonExistentUsernamePrefix}{_fixture.GenerateUniqueId()}";

        HttpResponseMessage response = await _fixture.Client.GetAsync($"{ApiConstants.UserEndpoint}/{missingUsername}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ExistingUser_ReturnsOk()
    {
        User user = BuildUser("xunit_update");
        await _fixture.Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);

        user.FirstName = "UpdatedName";
        user.Email = "updated@example.com";

        HttpResponseMessage response = await _fixture.Client.PutAsJsonAsync($"{ApiConstants.UserEndpoint}/{user.Username}", user);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(apiResponse);
        Assert.Equal(ApiConstants.ApiSuccessCode, apiResponse.Code);
    }

    [Fact]
    public async Task LoginUser_ValidCredentials_ReturnsOkAndSessionMessage()
    {
        User user = BuildUser("xunit_login");
        await _fixture.Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);

        HttpResponseMessage response = await _fixture.Client.GetAsync($"{ApiConstants.UserLoginEndpoint}?username={user.Username}&password={user.Password}");
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(apiResponse);
        Assert.Contains(ApiConstants.LoginSessionMarker, apiResponse.Message);
    }

    [Theory]
    [InlineData("demo_user_alpha", "Pass1234!")]
    [InlineData("demo_user_beta", "Pass5678!")]
    public async Task LoginUser_ParameterizedCredentials_ReturnsOkAndSessionMessage(string username, string password)
    {
        HttpResponseMessage response = await _fixture.Client.GetAsync($"{ApiConstants.UserLoginEndpoint}?username={username}&password={password}");
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(apiResponse);
        Assert.Contains(ApiConstants.LoginSessionMarker, apiResponse.Message);
    }

    [Fact]
    public async Task LogoutUser_ReturnsOk()
    {
        HttpResponseMessage response = await _fixture.Client.GetAsync(ApiConstants.UserLogoutEndpoint);
        ApiResponse? apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(apiResponse);
        Assert.Equal(ApiConstants.ApiSuccessCode, apiResponse.Code);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsOk()
    {
        User user = BuildUser("xunit_delete");
        await _fixture.Client.PostAsJsonAsync(ApiConstants.UserEndpoint, user);
        await _fixture.GetWithRetryAsync($"{ApiConstants.UserEndpoint}/{user.Username}");

        HttpResponseMessage response = await _fixture.Client.DeleteAsync($"{ApiConstants.UserEndpoint}/{user.Username}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private User BuildUser(string prefix)
    {
        long id = _fixture.GenerateUniqueId();
        string username = _fixture.GenerateUniqueUsername(prefix);
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
