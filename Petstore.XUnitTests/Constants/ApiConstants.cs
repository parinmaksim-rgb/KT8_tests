namespace Petstore.XUnitTests.Constants;

public static class ApiConstants
{
    public const string BaseUrl = "https://petstore.swagger.io/v2/";
    public const string ApiKeyHeader = "api_key";
    public const string ApiKeyValue = "special-key";

    public const string PetEndpoint = "pet";
    public const string PetFindByStatusEndpoint = "pet/findByStatus";
    public const string StoreInventoryEndpoint = "store/inventory";
    public const string StoreOrderEndpoint = "store/order";
    public const string UserEndpoint = "user";
    public const string UserCreateWithArrayEndpoint = "user/createWithArray";
    public const string UserCreateWithListEndpoint = "user/createWithList";
    public const string UserLoginEndpoint = "user/login";
    public const string UserLogoutEndpoint = "user/logout";

    public const string StatusAvailable = "available";
    public const string StatusPending = "pending";
    public const string StatusSold = "sold";

    public const string OrderStatusPlaced = "placed";
    public const string OrderStatusApproved = "approved";
    public const string OrderStatusDelivered = "delivered";

    public const long NonExistentPetId = 999999999991;
    public const long NegativePetId = -100;
    public const long NonExistentOrderId = 999999999992;
    public const string NonExistentUsernamePrefix = "nonexistent_user_";

    public const int DefaultCategoryId = 10;
    public const int DefaultTagId = 100;
    public const int DefaultQuantity = 1;
    public const int BulkQuantity = 5;
    public const int DefaultUserStatus = 1;
    public const int ApiSuccessCode = 200;
    public const int MaxRetryAttempts = 3;
    public const int RetryDelayMilliseconds = 300;
    public const int HttpTimeoutSeconds = 30;
    public const string LoginSessionMarker = "logged in user session";
}
